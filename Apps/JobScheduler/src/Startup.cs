//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.JobScheduler
{
    using System;
    using System.Diagnostics.Contracts;
    using Hangfire;
    using Hangfire.PostgreSql;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.DrugMaintainer;
    using HealthGateway.JobScheduler.Authorization;
    using Healthgateway.JobScheduler.Jobs;
    using Healthgateway.JobScheduler.Utils;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup
    {
        private readonly StartupConfiguration startupConfig;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        public Startup(IHostingEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            this.startupConfig = new StartupConfiguration(configuration, env, logger);
            this.configuration = configuration;
            this.logger = logger;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The passed in Service Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureHttpServices(services);
            this.ConfigureAuthentication(services);

            services.AddDbContextPool<GatewayDbContext>(options =>
                options.UseNpgsql(
                    this.configuration.GetConnectionString("GatewayConnection"),
                    b => b.MigrationsAssembly(nameof(Database))));

            services.AddTransient<IEmailDelegate, DBEmailDelegate>();
            services.AddTransient<IEmailJob, EmailJob>();

            // DB Maintainer Services
            services.AddTransient<IFileDownloadService, FileDownloadService>();
            services.AddTransient<IDrugProductParser, FederalDrugProductParser>();
            services.AddTransient<IPharmaCareDrugParser, PharmaCareDrugParser>();

            // Add app
            services.AddTransient<FedDrugJob>();
            services.AddTransient<ProvincialDrugJob>();

            // Enable Hangfire
            services.AddHangfire(x => x.UsePostgreSqlStorage(this.configuration.GetConnectionString("GatewayConnection")));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The passed in Application Builder.</param>
        /// <param name="env">The passed in Environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Contract.Requires(env != null);
            this.logger.LogInformation($"Hosting Environment: {env.EnvironmentName}");

            app.UseAuthentication();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None,
            });

            // Empty string signifies the root URL
            app.UseHangfireDashboard(string.Empty, new DashboardOptions
            {
                Authorization = new[] { new AuthorizationDashboardFilter(this.configuration, this.logger) },
            });
            app.UseHangfireServer();

            // Schedule Health Gateway Jobs
            BackgroundJob.Enqueue<DBMigrationsJob>(j => j.Migrate());
            SchedulerHelper.ScheduleJob<IEmailJob>(this.configuration, "SendLowPriorityEmail", j => j.SendLowPriorityEmails());
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedApprovedDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedMarketedDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedCancelledDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedDormantDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<ProvincialDrugJob>(this.configuration, "PharmaCareDrugFile");

            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseHttp(app);
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = (content) =>
                {
                    var headers = content.Context.Response.Headers;
                    var contentType = headers["Content-Type"];
                    if (contentType != "application/x-gzip" && !content.File.Name.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return;
                    }

                    var mimeTypeProvider = new FileExtensionContentTypeProvider();
                    var fileNameToTry = content.File.Name.Substring(0, content.File.Name.Length - 3);
                    if (mimeTypeProvider.TryGetContentType(fileNameToTry, out var mimeType))
                    {
                        headers.Add("Content-Encoding", "gzip");
                        headers["Content-Type"] = mimeType;
                    }
                },
            });

            // This is needed if running behind a reverse proxy
            // like ngrok which is great for testing while developing
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                RequireHeaderSymmetry = false,
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            });

            app.UseCors();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=JobScheduler}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// This sets up the OIDC authentication for Hangfire.
        /// </summary>
        /// <param name="services">The passed in IServiceCollection.</param>
        private void ConfigureAuthentication(IServiceCollection services)
        {
            string authorityEndPoint = this.configuration.GetValue<string>("OpenIdConnect:Authority");
            bool requireHttpsMetadata = this.configuration.GetValue<bool>("OpenIdConnect:RequireHttpsMetadata");
            string clientId = this.configuration.GetValue<string>("OpenIdConnect:ClientId");
            string clientSecret = this.configuration.GetValue<string>("OpenIdConnect:ClientSecret");

            services.AddAuthentication(auth =>
            {
                auth.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "HealthGateway_JobScheduler";
            })
            .AddOpenIdConnect(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = authorityEndPoint;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.ClientId = clientId;
                options.SaveTokens = false;
                options.ClientSecret = clientSecret;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.RequireHttpsMetadata = requireHttpsMetadata;
                options.Scope.Add("openid");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                };
            });
        }
    }
}
