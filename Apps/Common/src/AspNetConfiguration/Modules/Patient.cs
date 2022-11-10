// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.AspNetConfiguration.Modules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Services;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using ServiceReference;

    /// <summary>
    /// Provides ASP.Net Services for Patient access.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Patient
    {
        /// <summary>
        /// Configures the ability to use Patient services.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        public static void ConfigurePatientAccess(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            services.AddTransient<IEndpointBehavior, LoggingEndpointBehaviour>();
            services.AddTransient<IClientMessageInspector, LoggingMessageInspector>();
            services.AddTransient<QUPA_AR101102_PortType>(
                s =>
                {
                    IConfigurationSection clientConfiguration = configuration.GetSection("PatientService:ClientRegistry");
                    EndpointAddress clientRegistriesEndpoint = new(new Uri(clientConfiguration.GetValue<string>("ServiceUrl")));

                    QUPA_AR101102_PortTypeClient client = new(QUPA_AR101102_PortTypeClient.EndpointConfiguration.QUPA_AR101102_Port, clientRegistriesEndpoint);
                    if (clientConfiguration.GetValue("IsSecure", true))
                    {
                        // Load Certificate
                        // Note: As per reading we do not have to dispose of the certificate.
                        string clientCertificatePath = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Path");
                        string certificatePassword = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Password");
                        X509Certificate2 clientRegistriesCertificate = new(File.ReadAllBytes(clientCertificatePath), certificatePassword);
                        client.ClientCredentials.ClientCertificate.Certificate = clientRegistriesCertificate;
                        client.Endpoint.EndpointBehaviors.Add(s.GetService<IEndpointBehavior>());
                        client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication =
                            new X509ServiceCertificateAuthentication
                            {
                                CertificateValidationMode = X509CertificateValidationMode.None,
                                RevocationMode = X509RevocationMode.NoCheck,
                            };

                        BasicHttpBinding binding = new(BasicHttpSecurityMode.Transport);
                        binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
                        client.Endpoint.Binding = binding;
                    }

                    return client;
                });

            services.AddTransient<IClientRegistriesDelegate, ClientRegistriesDelegate>();
            services.AddTransient<IPatientService, PatientService>();
            GatewayCache.ConfigureCaching(services, logger, configuration);
        }

        /// <summary>
        /// Configures patient service to use Problem Details exception handling.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="environment">The environment the services are associated with.</param>
        public static void ConfigurePatientExceptionHandling(IServiceCollection services, IWebHostEnvironment environment)
        {
            services.AddProblemDetails(
                setup =>
                {
                    setup.IncludeExceptionDetails = (ctx, env) => environment.IsDevelopment();

                    setup.Map<ApiException>(
                        exception => new ApiProblemDetails
                        {
                            Title = exception.Title,
                            Detail = exception.Detail,
                            Status = exception.StatusCode,
                            Type = exception.ProblemType,
                            Instance = exception.Instance,
                            AdditionalInfo = exception.AdditionalInfo,
                        });
                });
        }

        /// <summary>
        /// Configures patient service to use the specified exception handling.
        /// </summary>
        /// <param name="app">The application builder where modules are specified to be used.</param>
        /// <param name="environment">The environment the services are associated with.</param>
        public static void ConfigurePatientExceptionHandling(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseProblemDetails();
        }
    }
}
