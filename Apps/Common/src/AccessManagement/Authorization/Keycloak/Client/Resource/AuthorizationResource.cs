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

    using HealthGateway.Common.Services;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Representation;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation;

    /// <summary>
    /// An entry point for obtaining permissions from the server.
    /// </summary>
    public class AuthorizationResource
    {
        private readonly ILogger logger;

        private readonly KeycloakConfiguration keycloakConfiguration;

        private readonly Uma2ServerConfiguration uma2ServerConfiguration;

        private readonly IHttpClientService httpClientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationResource"/> class.
        /// </summary>
        /// <param name="logger">injected logger service.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="keycloakConfiguration">The keycloak settings configuration.</param>
        /// <param name="uma2ServerConfiguration">uma2 server-side configuration settings.</param>
        public AuthorizationResource(ILogger<AuthorizationResource> logger, KeycloakConfiguration keycloakConfiguration,
            Uma2ServerConfiguration uma2ServerConfiguration,
            IHttpClientService httpClientService)
        {
            this.logger = logger;
            this.keycloakConfiguration = keycloakConfiguration;
            this.uma2ServerConfiguration = uma2ServerConfiguration;
            this.httpClientService = httpClientService;
        }

        /// <summary>Query the server for permissions given an <cref name="AuthorizationRequest"/>.</summary>
        /// <param name="request"> an <cref name="AuthorizationRequest"/></param>
        /// <returns>An <cref name="AuthorizationRequest"/>with a RPT holding all granted permissions.</returns>
        public async Task<AuthorizationResponse> authorize(AuthorizationRequest request)
        {
            if (request.Audience == null)
            {
                request.Audience = keycloakConfiguration.Audience;
            }

            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Add("Authorization", @"Bearer " + request.Token);
            client.BaseAddress = new Uri(this.uma2ServerConfiguration.TokenEndpoint);
            string requestUri = this.uma2ServerConfiguration.TokenEndpoint;

            if (request.Audience == string.Empty)
            {
                request.Audience = this.keycloakConfiguration.Audience;
            }
            string jsonOutput = JsonSerializer.Serialize<AuthorizationRequest>(request);

            using (HttpContent content = new StringContent(jsonOutput))
            {
                HttpResponseMessage response = await client.PostAsync(new Uri(requestUri), content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    this.logger.LogError($"AuthorizationResource.authorize() returned with StatusCode := {response.StatusCode}.");
                }

                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                AuthorizationResponse authorizationResponse = JsonSerializer.Deserialize<AuthorizationResponse>(result);
                return authorizationResponse;
            }
        }
    }
}