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
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupConfiguration"/> class.
        /// </summary>
        /// <param name="config">The configuration provider.</param>
        /// <param name="env">The environment variables provider.</param>
        public StartupConfiguration(IConfiguration config, IWebHostEnvironment env)
        {
            this.environment = env;
            this.configuration = config;
            this.Logger = ProgramConfiguration.GetInitialLogger(this.configuration);
        }

        /// <summary>
        /// Gets the startup configuration.
        /// </summary>
        public IConfiguration Configuration
        {
            get => this.configuration;
        }

        /// <summary>
        /// Gets the Startup Logger.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Configures the http services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        /// <param name="dbHealth">If true, DB Health checks will be added.</param>
        public void ConfigureHttpServices(IServiceCollection services, bool dbHealth = true)
        {
            Modules.HttpWeb.ConfigureHttpServices(services, this.Logger, dbHealth);
        }

        /// <summary>
        /// Configures the authorization services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureAuthorizationServices(IServiceCollection services)
        {
            Modules.Auth.ConfigureAuthorizationServices(services, this.Logger, this.configuration);
        }

        /// <summary>
        /// Configures the authorization services with user delegation access.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureDelegateAuthorizationServices(IServiceCollection services)
        {
            Modules.Auth.ConfigureDelegateAuthorizationServices(services, this.Logger, this.configuration);
        }

        /// <summary>
        /// Configures the auth services for json web token bearer.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureAuthServicesForJwtBearer(IServiceCollection services)
        {
            Modules.Auth.ConfigureAuthServicesForJwtBearer(services, this.Logger, this.configuration, this.environment);
        }

        /// <summary>
        /// This sets up the OIDC authentication.
        /// </summary>
        /// <param name="services">The passed in IServiceCollection.</param>
        public void ConfigureOpenIdConnectServices(IServiceCollection services)
        {
            Modules.Auth.ConfigureOpenIdConnectServices(services, this.Logger, this.configuration, this.environment);
        }

        /// <summary>
        /// Configures the audit services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureAuditServices(IServiceCollection services)
        {
            Modules.Audit.ConfigureAuditServices(services, this.Logger);
        }

        /// <summary>
        /// Configures the Database services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureDatabaseServices(IServiceCollection services)
        {
            Modules.Db.ConfigureDatabaseServices(services, this.Logger, this.configuration);
        }

        /// <summary>
        /// Configures the swagger services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        public void ConfigureSwaggerServices(IServiceCollection services)
        {
            Modules.SwaggerDoc.ConfigureSwaggerServices(services, this.configuration);
        }

        /// <summary>
        /// Configures Forward proxies.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureForwardHeaders(IServiceCollection services)
        {
            Modules.HttpWeb.ConfigureForwardHeaders(services, this.Logger, this.configuration);
        }

        /// <summary>
        /// Configures the ability to trigger Hangfire jobs.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureHangfireQueue(IServiceCollection services)
        {
            Modules.JobScheduler.ConfigureHangfireQueue(services, this.configuration);
        }

        /// <summary>
        /// Configures the ability to use Patient services.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigurePatientAccess(IServiceCollection services)
        {
            Modules.Patient.ConfigurePatientAccess(services, this.configuration);
        }

        /// <summary>
        /// Configures OpenTelemetry tracing.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureTracing(IServiceCollection services)
        {
            Modules.Utility.ConfigureTracing(services, this.Logger, this.configuration);
        }

        /// <summary>
        /// Configures Access control that allows any origin, header and method.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public void ConfigureAccessControl(IServiceCollection services)
        {
            Modules.HttpWeb.ConfigureAccessControl(services, this.Logger);
        }

        /// <summary>
        /// Configures the app to use auth.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseAuth(IApplicationBuilder app)
        {
            Modules.Auth.UseAuth(app, this.Logger);
        }

        /// <summary>
        /// Configures the app to use x-forwarded-for headers to obtain the real client IP.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseForwardHeaders(IApplicationBuilder app)
        {
            Modules.HttpWeb.UseForwardHeaders(app, this.Logger, this.configuration);
        }

        /// <summary>
        /// Configures the app to use http.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseHttp(IApplicationBuilder app)
        {
            Modules.HttpWeb.UseHttp(app, this.Logger, this.configuration, this.environment);
        }

        /// <summary>
        /// Configures the app to to use content security policies.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseContentSecurityPolicy(IApplicationBuilder app)
        {
            Modules.HttpWeb.UseContentSecurityPolicy(app, this.configuration);
        }

        /// <summary>
        /// Configures the app to use swagger.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseSwagger(IApplicationBuilder app)
        {
            Modules.SwaggerDoc.UseSwagger(app, this.Logger);
        }

        /// <summary>
        /// Configures the app to use Rest services.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseRest(IApplicationBuilder app)
        {
            Modules.HttpWeb.UseRest(app, this.Logger);
        }
    }
}
