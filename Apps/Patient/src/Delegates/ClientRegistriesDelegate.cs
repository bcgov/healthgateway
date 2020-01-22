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
namespace HealthGateway.Patient.Delegates
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using Microsoft.Extensions.Configuration;
    using ServiceReference;

    /// <summary>
    /// The Client Registries delegate.
    /// </summary>
    public class ClientRegistriesDelegate : IClientRegistriesDelegate
    {
        private readonly QUPA_AR101102_PortTypeClient getDemographicsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRegistriesDelegate"/> class.
        /// Constructor that requires an IEndpointBehavior for dependency injection.
        /// </summary>
        /// <param name="configuration">The service configuration.</param>
        /// <param name="loggingEndpointBehaviour">Endpoint behaviour for logging purposes.</param>
        public ClientRegistriesDelegate(IConfiguration configuration, IEndpointBehavior loggingEndpointBehaviour)
        {
            Contract.Requires(configuration != null);
            IConfigurationSection clientConfiguration = configuration.GetSection("ClientRegistries");

            // Load Certificate
            string clientCertificatePath = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Path");
            string certificatePassword = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Password");
            X509Certificate2 clientCertificate = new X509Certificate2(System.IO.File.ReadAllBytes(clientCertificatePath), certificatePassword);

            string serviceUrl = clientConfiguration.GetValue<string>("ServiceUrl");
            EndpointAddress endpoint = new EndpointAddress(new Uri(serviceUrl));

            // Create client
            this.getDemographicsClient = new QUPA_AR101102_PortTypeClient(QUPA_AR101102_PortTypeClient.EndpointConfiguration.QUPA_AR101102_Port, endpoint);
            this.getDemographicsClient.Endpoint.EndpointBehaviors.Add(loggingEndpointBehaviour);
            this.getDemographicsClient.ClientCredentials.ClientCertificate.Certificate = clientCertificate;

            // TODO: - HACK - Remove this once we can get the server certificate to be trusted.
            this.getDemographicsClient.ClientCredentials.ServiceCertificate.SslCertificateAuthentication =
                new X509ServiceCertificateAuthentication()
                {
                    CertificateValidationMode = X509CertificateValidationMode.None,
                    RevocationMode = X509RevocationMode.NoCheck,
                };
        }

        /// <inheritdoc />
        public async System.Threading.Tasks.Task<HCIM_IN_GetDemographicsResponse1> GetDemographicsAsync(HCIM_IN_GetDemographics request)
        {
            // Perform the request
            HCIM_IN_GetDemographicsResponse1 reply = await this.getDemographicsClient.HCIM_IN_GetDemographicsAsync(request).ConfigureAwait(true);
            return reply;
        }
    }
}