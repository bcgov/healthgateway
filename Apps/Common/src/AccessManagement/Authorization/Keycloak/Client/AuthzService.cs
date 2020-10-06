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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration;


    /// <summary>
    /// This is class serves as an entry point for clients looking for access to Keycloak Authorization Services.
    /// When creating a new instances make sure you have a Keycloak Server running at the location specified in the client
    /// configuration. The client tries to obtain server configuration by invoking the UMA Discovery Endpoint, usually available
    /// from the server at http(s)://{server}:{port}/auth/realms/{realm}/.well-known/uma-configuration.
    /// In the org.keycloak Java libs, this class is called 'AuthZClient'.
    /// </summary>
    public class AuthzService
    {
        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The injected HttpClientService delegate.
        /// </summary>
        private readonly IHttpClientService httpClientService;

        /// <summary>
        /// The  keycloak configuration.
        /// </summary>
        private readonly IKeycloakConfiguration keycloakConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthzService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="keycloakConfiguration">Injected keycloak Configuration.</param>
        public AuthzService(
            ILogger<AuthzService> logger,
            IHttpClientService httpClientService,
            IKeycloakConfiguration keycloakConfiguration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService!;
            this.keycloakConfiguration = keycloakConfiguration;
        }
    }
}