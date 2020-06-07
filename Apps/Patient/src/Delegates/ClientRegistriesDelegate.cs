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
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using Microsoft.Extensions.Configuration;
    using ServiceReference;

    /// <summary>
    /// The Client Registries delegate.
    /// </summary>
    public class ClientRegistriesDelegate : IClientRegistriesDelegate, IDisposable
    {
        private readonly IEndpointBehavior loggingEndpointBehaviour;
        private readonly X509Certificate2 clientCertificate;
        private readonly EndpointAddress endpoint;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRegistriesDelegate"/> class.
        /// Constructor that requires an IEndpointBehavior for dependency injection.
        /// </summary>
        /// <param name="configuration">The service configuration.</param>
        /// <param name="loggingEndpointBehaviour">Endpoint behaviour for logging purposes.</param>
        public ClientRegistriesDelegate(IConfiguration configuration, IEndpointBehavior loggingEndpointBehaviour)
        {
            IConfigurationSection clientConfiguration = configuration.GetSection("ClientRegistries");
            this.loggingEndpointBehaviour = loggingEndpointBehaviour;

            // Load Certificate
            string clientCertificatePath = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Path");
            string certificatePassword = clientConfiguration.GetSection("ClientCertificate").GetValue<string>("Password");
            this.clientCertificate = new X509Certificate2(System.IO.File.ReadAllBytes(clientCertificatePath), certificatePassword);

            string serviceUrl = clientConfiguration.GetValue<string>("ServiceUrl");
            this.endpoint = new EndpointAddress(new Uri(serviceUrl));
        }

        /// <inheritdoc />
        public async System.Threading.Tasks.Task<HCIM_IN_GetDemographicsResponse1> GetDemographicsAsync(HCIM_IN_GetDemographics request)
        {
            // Create client
            using QUPA_AR101102_PortTypeClient client = new QUPA_AR101102_PortTypeClient(QUPA_AR101102_PortTypeClient.EndpointConfiguration.QUPA_AR101102_Port, this.endpoint);
            client.Endpoint.EndpointBehaviors.Add(this.loggingEndpointBehaviour);
            client.ClientCredentials.ClientCertificate.Certificate = this.clientCertificate;

            // TODO: - HACK - Remove this once we can get the server certificate to be trusted.
            client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication =
                new X509ServiceCertificateAuthentication()
                {
                    CertificateValidationMode = X509CertificateValidationMode.None,
                    RevocationMode = X509RevocationMode.NoCheck,
                };

            // Perform the request
            HCIM_IN_GetDemographicsResponse1 reply = await client.HCIM_IN_GetDemographicsAsync(request).ConfigureAwait(true);
            return reply;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
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
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.clientCertificate.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}