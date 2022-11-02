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
namespace HealthGateway.Common.AspNetConfiguration
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The startup configuration class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class StartupConfiguration
    {
        private readonly IWebHostEnvironment environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupConfiguration"/> class.
        /// </summary>
        /// <param name="config">The configuration provider.</param>
        /// <param name="env">The environment variables provider.</param>
        public StartupConfiguration(IConfiguration config, IWebHostEnvironment env)
        {
            this.environment = env;
            this.Configuration = config;
            this.Logger = ProgramConfiguration.GetInitialLogger(this.Configuration);
        }

        /// <summary>
        /// Gets the startup configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the Startup Logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Configures the http services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        /// <param name="dbHealth">If true, DB Health checks will be added.</param>
        public void ConfigureHttpServices(IServiceCollection services, bool dbHealth = true)
        {
            HttpWeb.ConfigureHttpServices(services, this.Logger, dbHealth);
        }

        /// <summary>
        /// Configures the authorization services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureAuthorizationServices(IServiceCollection services)
        {
            Auth.ConfigureAuthorizationServices(services, this.Logger, this.Configuration);
        }

        /// <summary>
        /// Configures the authorization services with user delegation access.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureDelegateAuthorizationServices(IServiceCollection services)
        {
            Auth.ConfigureDelegateAuthorizationServices(services, this.Logger, this.Configuration);
        }

        /// <summary>
        /// Configures the auth services for json web token bearer.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureAuthServicesForJwtBearer(IServiceCollection services)
        {
            Auth.ConfigureAuthServicesForJwtBearer(services, this.Logger, this.Configuration, this.environment);
        }

        /// <summary>
        /// This sets up the OIDC authentication.
        /// </summary>
        /// <param name="services">The passed in IServiceCollection.</param>
        public void ConfigureOpenIdConnectServices(IServiceCollection services)
        {
            Auth.ConfigureOpenIdConnectServices(services, this.Logger, this.Configuration, this.environment);
        }

        /// <summary>
        /// Configures the audit services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureAuditServices(IServiceCollection services)
        {
            Audit.ConfigureAuditServices(services, this.Logger, this.Configuration);
        }

        /// <summary>
        /// Configures the Database services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureDatabaseServices(IServiceCollection services)
        {
            Db.ConfigureDatabaseServices(services, this.Logger, this.Configuration);
        }

        /// <summary>
        /// Configures the swagger services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        public void ConfigureSwaggerServices(IServiceCollection services)
        {
            SwaggerDoc.ConfigureSwaggerServices(services, this.Configuration);
        }

        /// <summary>
        /// Configures Forward proxies.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureForwardHeaders(IServiceCollection services)
        {
            HttpWeb.ConfigureForwardHeaders(services, this.Logger, this.Configuration);
        }

        /// <summary>
        /// Configures the ability to trigger Hangfire jobs.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureHangfireQueue(IServiceCollection services)
        {
            JobScheduler.ConfigureHangfireQueue(services, this.Configuration);
        }

        /// <summary>
        /// Configures the ability to use Patient services.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigurePatientAccess(IServiceCollection services)
        {
            Patient.ConfigurePatientAccess(services, this.Logger, this.Configuration);
        }

        /// <summary>
        /// Configures the ability to use PHSA v2 services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        public void ConfigurePhsaV2Access(IServiceCollection services)
        {
            PhsaV2.ConfigurePhsaV2Access(services, this.Logger, this.Configuration);
        }

        /// <summary>
        /// Configures OpenTelemetry tracing.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureTracing(IServiceCollection services)
        {
            Utility.ConfigureTracing(services, this.Logger, this.Configuration);
        }

        /// <summary>
        /// Configures Access control that allows any origin, header and method.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureAccessControl(IServiceCollection services)
        {
            HttpWeb.ConfigureAccessControl(services, this.Logger);
        }

        /// <summary>
        /// Configures the app to use auth.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseAuth(IApplicationBuilder app)
        {
            Auth.UseAuth(app, this.Logger);
        }

        /// <summary>
        /// Configures the app to use x-forwarded-for headers to obtain the real client IP.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseForwardHeaders(IApplicationBuilder app)
        {
            HttpWeb.UseForwardHeaders(app, this.Logger, this.Configuration);
        }

        /// <summary>
        /// Configures the app to use http.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseHttp(IApplicationBuilder app)
        {
            HttpWeb.UseHttp(app, this.Logger, this.Configuration, this.environment);
        }

        /// <summary>
        /// Configures the app to to use content security policies.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseContentSecurityPolicy(IApplicationBuilder app)
        {
            HttpWeb.UseContentSecurityPolicy(app, this.Configuration);
        }

        /// <summary>
        /// Configures the app to use swagger.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseSwagger(IApplicationBuilder app)
        {
            SwaggerDoc.UseSwagger(app, this.Logger);
        }

        /// <summary>
        /// Configures the app to use Rest services.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseRest(IApplicationBuilder app)
        {
            HttpWeb.UseRest(app, this.Logger);
        }
    }
}
