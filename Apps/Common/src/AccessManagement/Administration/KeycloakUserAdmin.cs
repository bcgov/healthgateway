//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
namespace HealthGateway.Common.AccessManagment.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;

    using HealthGateway.Common.AccessManagment.Administration.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class implementation of <see cref="IUserAdmin"/> that uses http REST to connect to Keycloak.
    /// </summary>
    public class KeycloakUserAdmin : IUserAdmin
    {
        /// <summary>
        /// Configuration Key for the KeycloakAdmin entry point.
        /// </summary>
        public const string KEYCLOAKADMIN = "KeycloakAdmin";
        /// <summary>
        /// Configuration Key for the Find User Url.
        /// </summary>
        public const string FINDUSERURL = "FindUserUrl";
        /// <summary>
        /// Configuration Key for the Delete User Url.
        /// </summary>
        public const string DELETEUSERURL = "DeleteUserUrl";
        /// <summary>
        /// Configuration Key for the Get User Url.
        /// </summary>
        public const string GETUSERURL = "GetUserUrl";

        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The injected HttpClientService delegate.
        /// </summary>
        private readonly IHttpClientService httpClientService;

        /// <summary>
        /// The injected configuration delegate.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeycloakUserAdmin"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="configuration">Injected configuration.</param>
        public KeycloakUserAdmin(
            ILogger<KeycloakUserAdmin> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<List<UserRepresentation>> FindUser(string username, string authorization)
        {
            List<UserRepresentation> usersReturned;

            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(FINDUSERURL));

            using HttpClient client = this.GethttpClient(baseUri, authorization);

            using HttpResponseMessage response = await client.GetAsync(new Uri($"?username={username}", UriKind.Relative)).ConfigureAwait(true);

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                };
                usersReturned = JsonSerializer.Deserialize<List<UserRepresentation>>(json, options);
            }
            else
            {
                this.logger.LogError($"Error finding user '{username}'");
                throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
            }

            return usersReturned;

        }

        /// <inheritdoc/>
        public async Task<UserRepresentation> GetUser(string userId, string authorization)
        {
            UserRepresentation userReturned;
            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(GETUSERURL));

            using HttpClient client = this.GethttpClient(baseUri, authorization);
            using HttpResponseMessage response = await client.GetAsync(new Uri($"/{userId}", UriKind.Relative)).ConfigureAwait(true);

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                };
                userReturned = JsonSerializer.Deserialize<UserRepresentation>(json, options);
            }
            else
            {
                this.logger.LogError($"Error getting user '{userId}'");
                throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
            }
            return userReturned;
        }

        /// <inheritdoc/>
        public async Task<int> DeleteUser(string userId, string base64BearerToken)
        {
            int returnCode = 0;
            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(DELETEUSERURL));

            using HttpClient client = this.GethttpClient(baseUri, base64BearerToken);
            using HttpResponseMessage response = await client.GetAsync(new Uri($"/{userId}", UriKind.Relative)).ConfigureAwait(true);
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                };
            }
            else
            {
                returnCode = -1;
                this.logger.LogError($"Error getting user '{userId}'");
                throw new HttpRequestException($"Unable to connect to DeleteUser: ${response.StatusCode}");
            }
            return returnCode;
        }

        /// <summary>
        /// Retuns an HttpClient for the AuthService to be invoked.
        /// </summary>
        /// <param name="baseUri">The uri of the service endpoint.</param>
        /// <param name="base64BearerToken">The JSON Web Token.</param>
        private HttpClient GethttpClient(Uri baseUri, string base64BearerToken)
        {
            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", @"Bearer " + base64BearerToken);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            client.BaseAddress = baseUri;
            return client;

        }
    }
}