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
namespace HealthGateway.Immunization
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.AccountDataAccess;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Api;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly StartupConfiguration startupConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.startupConfig = new StartupConfiguration(configuration, env);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureDatabaseServices(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureAuditServices(services);
            this.startupConfig.ConfigureAuthServicesForJwtBearer(services);
            this.startupConfig.ConfigureDelegateAuthorizationServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);
            this.startupConfig.ConfigureTracing(services);
            this.startupConfig.ConfigureAccessControl(services);
            this.startupConfig.ConfigurePatientAccess(services);
            this.startupConfig.ConfigureMessaging(services);
            this.startupConfig.ConfigureHangfireQueue(services);

            // Add Services
            services.AddTransient<IImmunizationMappingService, ImmunizationMappingService>();
            services.AddTransient<IImmunizationService, ImmunizationService>();
            services.AddTransient<IVaccineStatusService, VaccineStatusService>();

            // Add delegates
            services.AddTransient<IImmunizationDelegate, RestImmunizationDelegate>();
            services.AddTransient<IVaccineStatusDelegate, RestVaccineStatusDelegate>();
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();

            // Add API Clients
            PhsaConfig phsaConfig = new();
            this.startupConfig.Configuration.Bind(PhsaConfig.ConfigurationSectionKey, phsaConfig);
            services.AddRefitClient<IImmunizationApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig.BaseUrl);
            services.AddRefitClient<IImmunizationPublicApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig.BaseUrl);

            PhsaConfigV2 phsaConfigV2 = new();
            this.startupConfig.Configuration.Bind(PhsaConfigV2.ConfigurationSectionKey, phsaConfigV2);

            // Access patient repository
            services.AddPatientRepositoryConfiguration(new AccountDataAccessConfiguration(phsaConfigV2.BaseUrl));

            services.AddAutoMapper(typeof(Startup));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public void Configure(IApplicationBuilder app)
        {
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);
            this.startupConfig.UseHttp(app);
            this.startupConfig.UseAuth(app);
            this.startupConfig.EnrichTracing(app);
            this.startupConfig.UseRest(app);
        }
    }
}
