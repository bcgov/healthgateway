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
namespace HealthGateway.AdminWebClient
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Services;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Authorization.Admin;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SpaServices;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using VueCliMiddleware;

    /// <summary>
    /// Configures the application during startup.
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
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.startupConfig = new StartupConfiguration(configuration, env);
            this.logger = this.startupConfig.Logger;
            this.configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // To show detail of error and see the problem.
            IdentityModelEventSource.ShowPII = true;

            this.logger.LogDebug("Configure Services...");

            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureAuditServices(services);
            this.ConfigureAuthenticationService(services);
            this.startupConfig.ConfigureSwaggerServices(services);
            this.startupConfig.ConfigureHangfireQueue(services);

            // Add services
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IBetaRequestService, BetaRequestService>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();
            services.AddTransient<IUserFeedbackService, UserFeedbackService>();
            services.AddTransient<IDashboardService, DashboardService>();
            services.AddTransient<IEmailAdminService, EmailAdminService>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<ICsvExportService, CsvExportService>();

            // Add delegates
            services.AddTransient<IBetaRequestDelegate, DBBetaRequestDelegate>();
            services.AddTransient<IEmailDelegate, DBEmailDelegate>();
            services.AddTransient<IMessagingVerificationDelegate, DBMessagingVerificationDelegate>();
            services.AddTransient<IFeedbackDelegate, DBFeedbackDelegate>();
            services.AddTransient<IRatingDelegate, DBRatingDelegate>();
            services.AddTransient<IUserProfileDelegate, DBProfileDelegate>();
            services.AddTransient<ICommunicationDelegate, DBCommunicationDelegate>();
            services.AddTransient<INoteDelegate, DBNoteDelegate>();
            services.AddTransient<ICommentDelegate, DBCommentDelegate>();

            // Configure SPA
            services.AddControllersWithViews();

            // In production, the Vue files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The passed in Application Builder.</param>
        /// <param name="env">The passed in Environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);
            this.startupConfig.UseHttp(app);
            this.startupConfig.UseContentSecurityPolicy(app);
            this.startupConfig.UseAuth(app);

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            if (!env.IsDevelopment())
            {
                app.UseResponseCompression();
            }

            bool debugerAttached = System.Diagnostics.Debugger.IsAttached;

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller}/{action=Index}/{id?}");

                    if (env.IsDevelopment() && debugerAttached)
                    {
                        endpoints.MapToVueCliProxy(
                            "{*path}",
                            new SpaOptions { SourcePath = "ClientApp" },
                            npmScript: "serve",
                            regex: "Compiled successfully",
                            forceKill: true);
                    }
                });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (env.IsDevelopment() && !debugerAttached)
                {
                    // change this to whatever webpack dev server says it's running on
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:8080");
                }
            });
        }

        /// <summary>
        /// This sets up the OIDC authentication for Hangfire.
        /// </summary>
        /// <param name="services">The passed in IServiceCollection.</param>
        private void ConfigureAuthenticationService(IServiceCollection services)
        {
            string basePath = this.GetBasePath();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
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
                            ClaimsIdentity? identity = ctx.Principal.Identity as ClaimsIdentity;
                            if (identity != null)
                            {
                                identity.AddClaim(new Claim("access_token", accessToken.RawData));
                            }
                        }

                        return Task.FromResult(0);
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
