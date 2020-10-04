
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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Util
{
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Representation;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation;

    /// <summary>
    /// An entry point for obtaining permissions from the server.
    /// </summary>
    public class TokenCallable
    {
        private string? username = null;
        private string? password = null;

        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;

        private readonly KeycloakConfiguration keycloakConfiguration;

        private readonly Uma2ServerConfiguration uma2ServerConfiguration;

        /// <summary>Create a <cref name="TokenCallable"/></summary>
        public TokenCallable(
            ILogger<TokenCallable> logger,
            HttpClientService httpClientService,
            KeycloakConfiguration keycloakConfiguration,
            Uma2ServerConfiguration uma2ServerConfiguration,
            string username,
            string password) :  this(logger, httpClientService, keycloakConfiguration, uma2ServerConfiguration)
        {
            this.username = username;
            this.password = password;
        }

        /// <summary>Create a <cref name="TokenCallable"/></summary>
        public TokenCallable(
            ILogger<TokenCallable> logger,
            HttpClientService httpClientService,
            KeycloakConfiguration keycloakConfiguration,
            Uma2ServerConfiguration uma2ServerConfiguration) 
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.keycloakConfiguration = keycloakConfiguration;
            this.uma2ServerConfiguration = uma2ServerConfiguration;
        }

        public async Task<AccessToken> call() 
        {

        }
    }
}