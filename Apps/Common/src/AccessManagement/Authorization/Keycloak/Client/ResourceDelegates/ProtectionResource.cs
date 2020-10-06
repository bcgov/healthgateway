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
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Representation;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Util;

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
        public ProtectionResource(ILogger<ProtectionResource> logger,
            KeycloakConfiguration keycloakConfiguration,
            Uma2ServerConfiguration uma2ServerConfiguration,
            HttpClientService httpClientService)
        {
            this.logger = logger;
            this.keycloakConfiguration = keycloakConfiguration;
            this.uma2ServerConfiguration = uma2ServerConfiguration;
            this.httpClientService = httpClientService;
        }

        /// <summary>Introspects the given <code>rpt</code> using the token introspection endpoint.</summary>
        /// <param name="rpt">the Requesting Party Token to Introspect.</param>
        /// <param name="token">The bearer token to use for authorization.</param>
        /// <returns>A TokenIntrospectionResponse.</returns>
        public async Task<TokenIntrospectionResponse> introspectRequestingPartyToken(string rpt, string token)
        {

            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.IntrospectionEndpoint;
            client.BaseAddress = new Uri(requestUrl);

            MultipartFormDataContent multiForm = new MultipartFormDataContent();

            multiForm.Add(new StringContent(OAuth2Constants.UMA_GRANT_TYPE), OAuth2Constants.GRANT_TYPE);
            multiForm.Add(new StringContent("requesting_party_token"), "token_type_hint");
            multiForm.Add(new StringContent(rpt), "token");

            HttpResponseMessage response = await client.PostAsync(new Uri(requestUrl), multiForm).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"introspectRequestingPartyToken() returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            TokenIntrospectionResponse introspectionResponse = JsonSerializer.Deserialize<TokenIntrospectionResponse>(result);
            return introspectionResponse;
        }
    }
}