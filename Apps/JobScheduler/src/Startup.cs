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
    using Hangfire;
    using Hangfire.PostgreSql;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Authorization.Admin;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Services;
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
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup
    {
        private readonly IWebHostEnvironment environment;
        private readonly StartupConfiguration startupConfig;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.environment = env;
            this.startupConfig = new StartupConfiguration(configuration, env);
            this.configuration = configuration;
            this.logger = this.startupConfig.Logger;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The passed in Service Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureForwardHeaders(services);
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
            services.AddTransient<IApplicationSettingsDelegate, DBApplicationSettingsDelegate>();
            services.AddTransient<ILegalAgreementDelegate, DBLegalAgreementDelegate>();
            services.AddTransient<IProfileDelegate, DBProfileDelegate>();
            services.AddTransient<IEmailDelegate, DBEmailDelegate>();
            services.AddTransient<IEmailInviteDelegate, DBEmailInviteDelegate>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();

            // TODO: Add injection for KeyCload User

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
            this.logger.LogInformation($"Hosting Environment: {env.EnvironmentName}");
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseAuth(app);
            this.startupConfig.UseHttp(app);
            app.UseEndpoints(endpoints =>
            {
                // Mapping of endpoints goes here:
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            // Empty string signifies the root URL
            app.UseHangfireDashboard(string.Empty, new DashboardOptions
            {
                DashboardTitle = this.configuration.GetValue<string>("DashboardTitle", "Hangfire Dashboard"),
                Authorization = new[] { new AuthorizationDashboardFilter(this.configuration, this.logger) },
                AppPath = $"{this.configuration.GetValue<string>("JobScheduler:AdminHome")}",
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
            SchedulerHelper.ScheduleJob<HNClientTestJob>(this.configuration, "HNClientTest", j => j.Process());
            SchedulerHelper.ScheduleJob<NotifyUpdatedLegalAgreementsJob>(this.configuration, "NotifyUpdatedLegalAgreements", j => j.Process());
            SchedulerHelper.ScheduleJob<CloseAccountJob>(this.configuration, "CloseAccounts", j => j.Process());

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
            string basePath = this.GetBasePath();
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = AuthorizationConstants.CookieName;
                options.LoginPath = $"{basePath}{AuthorizationConstants.LoginPath}";
                options.LogoutPath = $"{basePath}{AuthorizationConstants.LogoutPath}";
            })
            .AddOpenIdConnect(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                };
                this.configuration.GetSection("OpenIdConnect").Bind(options);
                if (string.IsNullOrEmpty(options.Authority))
                {
                    this.logger.LogCritical("OpenIdConnect Authority is missing, bad things are going to occur");
                }

                options.Events = new OpenIdConnectEvents()
                {
                    OnTokenValidated = ctx =>
                    {
                        JwtSecurityToken accessToken = ctx.SecurityToken;
                        if (accessToken != null)
                        {
                            ClaimsIdentity identity = ctx.Principal.Identity as ClaimsIdentity;
                            if (identity != null)
                            {
                                identity.AddClaim(new Claim("access_token", accessToken.RawData));
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProvider = ctx =>
                    {
                        if (!string.IsNullOrEmpty(this.configuration["KeyCloak:IDPHint"]))
                        {
                            this.logger.LogDebug("Adding IDP Hint on Redirect to provider");
                            ctx.ProtocolMessage.SetParameter(this.configuration["KeyCloak:IDPHintKey"], this.configuration["KeyCloak:IDPHint"]);
                        }

                        return Task.FromResult(0);
                    },
                    OnAuthenticationFailed = c =>
                    {
                        c.HandleResponse();
                        c.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        c.Response.ContentType = "text/plain";
                        this.logger.LogError(c.Exception.ToString());
                        return c.Response.WriteAsync(c.Exception.ToString());
                    },
                };
            });
        }

        private string GetBasePath()
        {
            string basePath = string.Empty;
            IConfigurationSection section = this.configuration.GetSection("ForwardProxies");
            if (section.GetValue<bool>("Enabled", false))
            {
                basePath = section.GetValue<string>("BasePath");
            }

            this.logger.LogDebug($"JobScheduler basePath = {basePath}");
            return basePath;
        }
    }
}
