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
    using System.Threading.Tasks;
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
        public Startup(IWebHostEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Contract.Requires(env != null);
            this.logger.LogInformation($"Hosting Environment: {env.EnvironmentName}");

            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseAuth(app);
            this.startupConfig.UseHttp(app);

            app.Use(async (context, next) =>
            {
                this.logger.LogDebug($"Current Protocol: {context.Request.Protocol}");
                context.Request.Scheme = Uri.UriSchemeHttps;
                this.logger.LogDebug($"New Protocol: {context.Request.Protocol}");
                await next.Invoke().ConfigureAwait(true);
            });

            // Empty string signifies the root URL
            app.UseHangfireDashboard(string.Empty, new DashboardOptions
            {
                DashboardTitle = this.configuration.GetValue<string>("DashboardTitle", "Hangfire Dashboard"),
                Authorization = new[] { new AuthorizationDashboardFilter(this.configuration, this.logger) },
                AppPath = AuthorizationConstants.LogoutPath,
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
        }

        /// <summary>
        /// This sets up the OIDC authentication for Hangfire.
        /// </summary>
        /// <param name="services">The passed in IServiceCollection.</param>
        private void ConfigureAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = AuthorizationConstants.CookieName;
                options.LoginPath = AuthorizationConstants.LoginPath;
                options.LogoutPath = AuthorizationConstants.LogoutPath;
            })
            .AddOpenIdConnect(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = false;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add("openid");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                };
                this.configuration.GetSection("OpenIdConnect").Bind(options);
                options.Events = new OpenIdConnectEvents()
                {
                    OnRedirectToIdentityProvider = ctx =>
                    {
                        this.logger.LogDebug("Redirecting to identity provider");
                        ctx.ProtocolMessage.RedirectUri = ctx.ProtocolMessage.RedirectUri.Replace(Uri.UriSchemeHttp, Uri.UriSchemeHttps, StringComparison.Ordinal);
                        this.logger.LogDebug($"Sending Redirect URI: {ctx.ProtocolMessage.RedirectUri}");
                        return Task.FromResult(0);
                    },
                };
            });
        }
    }
}
