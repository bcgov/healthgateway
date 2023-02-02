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
namespace HealthGateway.Admin
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Api;
    using HealthGateway.Admin.Delegates;
    using HealthGateway.Admin.Services;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authorization.Admin;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.MapProfiles;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.AspNetCore.SpaServices;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Refit;
    using VueCliMiddleware;
    using AuthenticationService = HealthGateway.Admin.Services.AuthenticationService;
    using IAuthenticationService = HealthGateway.Admin.Services.IAuthenticationService;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    public class Startup
    {
        private const string PhsaConfigSectionKey = "PHSA";
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger logger;
        private readonly StartupConfiguration startupConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.startupConfig = new StartupConfiguration(configuration, env);
            this.logger = this.startupConfig.Logger;
            this.environment = env;
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
            this.startupConfig.ConfigureDatabaseServices(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureAuditServices(services);
            this.ConfigureAdminAuthenticationService(services);
            this.startupConfig.ConfigureSwaggerServices(services);
            this.startupConfig.ConfigureHangfireQueue(services);
            this.startupConfig.ConfigurePatientAccess(services);

            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<ISupportService, SupportService>();
            services.AddTransient<ICovidSupportService, CovidSupportService>();
            services.AddTransient<IUserProfileDelegate, DbProfileDelegate>();

            // Add delegates
            services.AddTransient<IMessagingVerificationDelegate, DbMessagingVerificationDelegate>();
            services.AddTransient<IImmunizationAdminDelegate, RestImmunizationAdminDelegate>();
            services.AddTransient<IVaccineStatusDelegate, RestVaccineStatusDelegate>();
            services.AddTransient<IVaccineProofDelegate, VaccineProofDelegate>();
            services.AddTransient<IAdminUserProfileDelegate, DbAdminUserProfileDelegate>();
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
            services.AddTransient<IResourceDelegateDelegate, DbResourceDelegateDelegate>();

            // Configure SPA
            services.AddControllersWithViews();

            // In production, the Vue files will be served from this directory
            services.AddSpaStaticFiles(options => { options.RootPath = "ClientApp/dist"; });

            // Add API Clients
            PhsaConfig phsaConfig = new();
            this.startupConfig.Configuration.Bind(PhsaConfigSectionKey, phsaConfig);
            services.AddRefitClient<IImmunizationAdminApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig.BaseUrl);

            services.AddAutoMapper(typeof(Startup), typeof(UserProfileProfile), typeof(MessagingVerificationProfile));
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

            DisableTraceMethod(app);

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            if (!env.IsDevelopment())
            {
                app.UseResponseCompression();
            }

            bool debugerAttached = Debugger.IsAttached;
            bool serverOnly = bool.Parse(Environment.GetEnvironmentVariable("ServerOnly") ?? "false");

            bool launchDevSpa = debugerAttached && !serverOnly;

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapControllerRoute(
                        "default",
                        "{controller}/{action=Index}/{id?}");

                    if (env.IsDevelopment() && launchDevSpa)
                    {
                        endpoints.MapToVueCliProxy(
                            "{*path}",
                            new SpaOptions { SourcePath = "ClientApp" },
                            regex: "Compiled successfully",
                            forceKill: true);
                    }
                });

            app.UseSpa(
                spa =>
                {
                    spa.Options.SourcePath = "ClientApp";
                    if (env.IsDevelopment() && !launchDevSpa)
                    {
                        // change this to whatever webpack dev server says it's running on
#pragma warning disable S1075
                        spa.UseProxyToSpaDevelopmentServer("http://localhost:8080");
#pragma warning restore S1075
                    }
                });

            RewriteOptions rewriteOption = new RewriteOptions()
                .AddRedirect("(.*[^/])$", "$1/");
            app.UseRewriter(rewriteOption);
        }

        private static void DisableTraceMethod(IApplicationBuilder app)
        {
            app.Use(
                async (context, next) =>
                {
                    if (context.Request.Method == "TRACE")
                    {
                        context.Response.StatusCode = 405;
                        return;
                    }

                    await next.Invoke().ConfigureAwait(true);
                });
        }

        /// <summary>
        /// This sets up the OIDC authentication for Hangfire.
        /// </summary>
        /// <param name="services">The passed in IServiceCollection.</param>
        private void ConfigureAdminAuthenticationService(IServiceCollection services)
        {
            string basePath = this.GetBasePath();

            AuthenticationBuilder builder = services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    })
                .AddCookie(
                    options =>
                    {
                        options.Cookie.Name = AuthorizationConstants.CookieName;
                        options.LoginPath = $"{basePath}{AuthorizationConstants.LoginPath}";
                        options.LogoutPath = $"{basePath}{AuthorizationConstants.LogoutPath}";
                    });

            this.ConfigureOpenId(builder);
        }

        private void ConfigureOpenId(AuthenticationBuilder services)
        {
            services.AddOpenIdConnect(
                options =>
                {
                    // Allows http://localhost to work on Chromium and Edge.
                    if (this.environment.IsDevelopment())
                    {
                        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                        options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                        options.NonceCookie.SameSite = SameSiteMode.Unspecified;
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                    };
                    this.configuration.GetSection("OpenIdConnect").Bind(options);
                    if (string.IsNullOrEmpty(options.Authority))
                    {
                        this.logger.LogCritical("OpenIdConnect Authority is missing, bad things are going to occur");
                    }

                    options.Events = new OpenIdConnectEvents
                    {
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
                            this.logger.LogError("{Exception}", c.Exception);
                            return c.Response.WriteAsync(c.Exception.ToString());
                        },
                    };
                });
        }

        private string GetBasePath()
        {
            string basePath = string.Empty;
            IConfigurationSection section = this.configuration.GetSection("ForwardProxies");
            if (section.GetValue("Enabled", false))
            {
                basePath = section.GetValue<string>("BasePath") ?? string.Empty;
            }

            this.logger.LogDebug("basePath = {BasePath}", basePath);
            return basePath;
        }
    }
}
