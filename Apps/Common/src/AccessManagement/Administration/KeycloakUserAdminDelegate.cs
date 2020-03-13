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
namespace HealthGateway.Common.AccessManagement.Administration
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;

    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class implementation of <see cref="IUserAdminDelegate"/> that uses http REST to connect to Keycloak.
    /// </summary>
    public class KeycloakUserAdminDelegate : IUserAdminDelegate
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
        /// Initializes a new instance of the <see cref="KeycloakUserAdminDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="configuration">Injected configuration.</param>
        public KeycloakUserAdminDelegate(
            ILogger<KeycloakUserAdminDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService!;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public UserRepresentation GetUser(Guid userId, string base64BearerToken)
        {
            UserRepresentation userReturned;

            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(GETUSERURL));
            UriBuilder uriBuild = new UriBuilder(baseUri);
            uriBuild.Path = $"/{userId}";
            Uri requestUri = uriBuild.Uri;

            using HttpClient client = this.GethttpClient(baseUri, base64BearerToken);

            try
            {
                Task<string> jsonResult = this.Get(client, requestUri);
                string json = jsonResult.Result;
                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                };
                userReturned = JsonSerializer.Deserialize<UserRepresentation>(json, options);

                return userReturned;
            }
            catch (Exception e)
            {
                this.logger.LogError($"DeleteUser failed: ${e.ToString()}");

                return new UserRepresentation();
            }
        }

        /// <inheritdoc/>
        public int DeleteUser(Guid userId, string base64BearerToken)
        {
            this.logger.LogInformation($"Start DeleteUser : ${userId.ToString()}");

            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(DELETEUSERURL));
            Uri requestUri = new Uri(baseUri, new Uri($"{userId}", UriKind.Relative));

            using HttpClient client = this.GethttpClient(baseUri, base64BearerToken);
            try
            {
                Task<string> jsonResult = this.Delete(client, requestUri);
            }
            catch (Exception e)
            {
                this.logger.LogError($"DeleteUser failed: ${e.ToString()}");

                return -1;
            }

            this.logger.LogInformation($"End DeleteUser : ${userId.ToString()}");

            return 0;
        }

        private async Task<string> Get(HttpClient client, Uri uri)
        {
            HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(true);
            string jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"Error performing Get Request to Keycloak Admin API: ${uri.ToString()}'");
                throw new HttpRequestException($"Unable to connect to Keycloak: ${response.StatusCode}");
            }

            return jsonString;
        }

        private async Task<string> Delete(HttpClient client, Uri uri)
        {
            HttpResponseMessage response = await client.DeleteAsync(uri).ConfigureAwait(true);
            string jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"Error performing Delete Request to Keycloak Admin API: ${uri.ToString()}'");
                throw new HttpRequestException($"Unable to connect to Keycloak: ${response.StatusCode}");
            }
            this.logger.LogDebug($"Delete completed");

            return jsonString;
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