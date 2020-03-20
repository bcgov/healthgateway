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
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System;
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
            this.logger.LogInformation($"Keycloak GetUser : {userId.ToString()}");

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int DeleteUser(Guid userId, string token)
        {
            this.logger.LogInformation($"Keycloak DeleteUser : {userId.ToString()}");
            //this.logger.LogDebug($"Keycloak jwt : {base64BearerToken}");

            Task<int> task = Task.Run( () => this.DeleteUserAsync(userId.ToString(), token));
            return task.Result;

        }

        /// <summary>
        /// Invokes HTTP DELETE on RESTful API
        /// </summary>
        /// <param name="client">The HttpClient to invoke.</param>
        /// <param name="uri">The Uri</param>
        private async Task<int> DeleteUserAsync(string userId, string token)
        {

            Uri baseUri = new Uri(this.configuration.GetSection(KEYCLOAKADMIN).GetValue<string>(DELETEUSERURL));

            using HttpClient client = this.GetHttpClient(baseUri, token);
            {
                try
                {
                    HttpResponseMessage response = await client.DeleteAsync(userId).ConfigureAwait(true);
                    if (!response.IsSuccessStatusCode) //in this case we are getting a HTTP 204 no content success
                    {
                        this.logger.LogError($"Error performing DELETE Request: {userId}, HTTP StatusCode: {response.StatusCode}");
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Error performing DELETE Request: {userId}");
                    return -1;
                }
            }

            this.logger.LogInformation($"UserDelete completed");

            return 0;
        }

        /// <summary>
        /// Retuns an HttpClient for the AuthService to be invoked.
        /// </summary>
        /// <param name="baseUri">The uri of the service endpoint.</param>
        /// <param name="base64BearerToken">The JSON Web Token.</param>
        private HttpClient GetHttpClient(Uri baseUri, string base64BearerToken)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", @"Bearer " + base64BearerToken);
            client.BaseAddress = baseUri;

            return client;
        }

        /// <summary>
        /// Combines baseUri adding the relative path, avoiding missing forward slashes
        /// </summary>
        /// <param name="baseUri">The Uri to add to</param>
        /// <param name="relativePath">The string representing the relative path to append.</param>    
        /// <returns>a new Uri combining the baseUri and relativePath.</returns>
        private Uri Combine(Uri baseUri, string relativePath)
        {
            string original = baseUri.OriginalString;
            string relative = relativePath.StartsWith(@"/") ? relativePath.Substring(1) : relativePath;
            string path = original.EndsWith(@"/") ? $"{original}{relativePath}" : $"{original}/{relativePath}";
            Uri newUri = new Uri(path);
            return newUri;
        }
    }
}
