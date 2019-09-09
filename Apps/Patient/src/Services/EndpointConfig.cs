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
namespace ServiceReference
{
    using System.ServiceModel;
    using System.ServiceModel.Description;

#pragma warning disable CA1707 // Identifiers should not contain underscores
                              /// <summary>
                              /// Port type client.
                              /// </summary>
    public partial class QUPA_AR101102_PortTypeClient
#pragma warning restore CA1707 // Identifiers should not contain underscores
    {
        /// <summary>
        /// Configures the GetDemographics client.
        /// </summary>
        /// <param name="serviceEndpoint">the service endpoint.</param>
        /// <param name="clientCredentials">the client credentials.</param>
        static partial void ConfigureEndpoint(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
        {
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            serviceEndpoint.Binding = binding;
        }
    }
}