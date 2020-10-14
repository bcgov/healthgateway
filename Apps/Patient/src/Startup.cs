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
#pragma warning disable CA1303 //disable literal strings check
namespace HealthGateway.Patient
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Patient.Delegates;
    using HealthGateway.Patient.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceReference;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup : IDisposable
    {
        private readonly StartupConfiguration startupConfig;
        private readonly EndpointAddress clientRegistriesEndpoint;
        private readonly X509Certificate2 clientRegistriesCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.startupConfig = new StartupConfiguration(configuration, env);
            IConfigurationSection clientConfiguration = configuration.GetSection("ClientRegistries");
            this.clientRegistriesEndpoint = new EndpointAddress(new Uri(clientConfiguration.GetValue<string>("ServiceUrl")));

            // Load Certificate
            string clientCertificatePath = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Path");
            string certificatePassword = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Password");
            this.clientRegistriesCertificate = new X509Certificate2(System.IO.File.ReadAllBytes(clientCertificatePath), certificatePassword);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureAuditServices(services);
            this.startupConfig.ConfigureAuthServicesForJwtBearer(services);
            this.startupConfig.ConfigureAuthorizationServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);

            services.AddTransient<IEndpointBehavior, LoggingEndpointBehaviour>();
            services.AddTransient<IClientMessageInspector, LoggingMessageInspector>();

            services.AddTransient<QUPA_AR101102_PortType>(s =>
            {
                QUPA_AR101102_PortTypeClient client = new QUPA_AR101102_PortTypeClient(
                    QUPA_AR101102_PortTypeClient.EndpointConfiguration.QUPA_AR101102_Port,
                    this.clientRegistriesEndpoint);
                client.ClientCredentials.ClientCertificate.Certificate = this.clientRegistriesCertificate;
                client.Endpoint.EndpointBehaviors.Add(s.GetService<IEndpointBehavior>());

                // TODO: - HACK - Remove this once we can get the server certificate to be trusted.
                client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication =
                    new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck,
                    };

                return client;
            });

            services.AddTransient<IPatientDelegate, ClientRegistriesDelegate>();
            services.AddTransient<IPatientService, PatientService>();
            services.AddSingleton<ITraceService, TimedTraceService>();
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
            this.startupConfig.UseRest(app);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// </summary>
        /// <param name="disposing">Indicates if the method has been called directly or indirectly.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.clientRegistriesCertificate.Dispose();
            }
        }
    }
}
#pragma warning restore CA1303
