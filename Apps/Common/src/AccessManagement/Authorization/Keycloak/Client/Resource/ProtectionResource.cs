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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Resource
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Representation;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation;
    using HealthGateway.Common.Services;
    ///
    /// <summary>An entry point for managing permission tickets using the Protection API.</summary>
    ///
    public class ProtectionResource
    {
        private readonly ILogger logger;

        /// <summary>The injected HttpClientService.</summary>
        private readonly IHttpClientService httpClientService;

        /// <summary>The keycloak configuration settings read from the injected app settings.</summary>
        private readonly KeycloakConfiguration keycloakConfiguration;

        /// <summary>The keycloak UMA server configuration.</summary>
        private readonly Uma2ServerConfiguration uma2ServerConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationResource"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="keycloakConfiguration">The keycloak configuration.</param>
        /// <param name="uma2ServerConfiguration">uma2 server-side configuration settings.</param>
        public ProtectionResource(ILogger<PermissionResource> logger,
            KeycloakConfiguration keycloakConfiguration,
            Uma2ServerConfiguration uma2ServerConfiguration,
            HttpClientService httpClientService)
        {
            this.logger = logger;
            this.keycloakConfiguration = keycloakConfiguration;
            this.uma2ServerConfiguration = uma2ServerConfiguration;
            this.httpClientService = httpClientService;
        }
    }
}