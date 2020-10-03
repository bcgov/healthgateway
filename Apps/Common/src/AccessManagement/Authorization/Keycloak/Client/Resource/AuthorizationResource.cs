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
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Representation;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation;

    /// <summary>
    /// An entry point for obtaining permissions from the server.
    /// </summary>
    public class AuthorizationResource : Resource
    {
        private readonly ILogger logger;

                /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationResource"/> class.
        /// </summary>
        /// <param name="logger">injected logger service.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="keycloakConfiguration">The keycloak settings configuration.</param>
        /// <param name="uma2ServerConfiguration">uma2 server-side configuration settings.</param>
        public AuthorizationResource(ILogger<AuthorizationResource> logger, KeycloakConfiguration keycloakConfiguration,
            Uma2ServerConfiguration uma2ServerConfiguration,
            HttpClientService httpClientService) : base(keycloakConfiguration, uma2ServerConfiguration, httpClientService)
        {
            this.logger = logger;
        }

        /// Query the server for all permissions.
        /// <returns> A <cref="AuthorizationResponse"/> with a RPT holding all granted permissions.</returns>
        public AuthorizationResponse authorize()
        {
            return authorize(new AuthorizationRequest());
        }
        /**
      * Query the server for permissions given an {@link AuthorizationRequest}.
      *
      * @param request an {@link AuthorizationRequest} (not {@code null})
      * @return an {@link AuthorizationResponse} with a RPT holding all granted permissions
      * @throws AuthorizationDeniedException in case the request was denied by the server
      */
        public AuthorizationResponse authorize(AuthorizationRequest? request)
        {
            if (request == null)
            {
                // throw new IllegalArgumentException("Authorization request must not be null");
            }

            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", @"Bearer " + request.Token);
            client.BaseAddress = this.uma2ServerConfiguration.TokenEndpoint;

            if (request.Audience == string.Empty)
            {
                request.Audience = this.configuration.Audience;
            }
            // POST the call to the resource.
        }

    //HttpMethod<AuthorizationResponse> method = http.< AuthorizationResponse > post(serverConfiguration.getTokenEndpoint());

               // if (token != null) {
            //method = method.authorizationBearer(token.call());
       // }
}