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
    public class ClientRegistriesDelegate : IClientRegistriesDelegate
    {
        private readonly QUPA_AR101102_PortType clientRegistriesClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRegistriesDelegate"/> class.
        /// Constructor that requires an IEndpointBehavior for dependency injection.
        /// </summary>
        /// <param name="clientRegistriesClient">The injected client registries soap client.</param>
        public ClientRegistriesDelegate(QUPA_AR101102_PortType clientRegistriesClient)
        {
            this.clientRegistriesClient = clientRegistriesClient;
        }

        /// <inheritdoc />
        public async System.Threading.Tasks.Task<HCIM_IN_GetDemographicsResponse1> GetDemographicsAsync(HCIM_IN_GetDemographicsRequest request)
        {
            // Perform the request
            HCIM_IN_GetDemographicsResponse1 reply = await this.clientRegistriesClient.HCIM_IN_GetDemographicsAsync(request).ConfigureAwait(true);
            return reply;
        }
    }
}