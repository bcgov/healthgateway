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
    using System;
    using System.Net.Http;
    using System.Text.Json;

    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Util;

    using HealthGateway.Common.Services;
    ///
    /// <summary>Gets the uma2 server configuration settings from the well-known-endppoint.</summary>
    ///
    public class ServerConfigurationDelegate
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;

        private readonly IKeycloakConfiguration keycloakConfiguration;

        public ServerConfigurationDelegate(ILogger<ServerConfigurationDelegate> logger,
            IHttpClientService httpClientService,
            IKeycloakConfiguration keycloakConfiguration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.keycloakConfiguration = keycloakConfiguration;
        }

        /// <summary>Gets the UMA 2.0 Server Configuration from teh well-known Keycloak server end point.</summary>
        /// <returns>An instance of a <cref name="Uma2ServerConfiguration"/>.</returns>
        public async Task<Uma2ServerConfiguration> getServerConfiguration()
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            Uri configUri = KeycloakUriBuilder.buildUri(this.keycloakConfiguration, ServiceUrlConstants.AUTHZ_DISCOVERY_URL);

            HttpResponseMessage response = await client.GetAsync(configUri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"getServerConfiguration() returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Uma2ServerConfiguration configurationResponse = JsonSerializer.Deserialize<Uma2ServerConfiguration>(result);
            return configurationResponse;
        }
    }
}