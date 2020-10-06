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
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.Json;

    using System.Threading.Tasks;

    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Util;

    using HealthGateway.Common.Services;
    ///
    /// <summary>An entry point for managing permission tickets using the Protection API.</summary>
    ///
    public class ProtectedResource
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;

        private readonly KeycloakConfiguration keycloakConfiguration;

        private readonly Uma2ServerConfiguration uma2ServerConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationResource"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="keycloakConfiguration">The Keycloak configuration.</param>
        /// <param name="uma2ServerConfiguration">uma2 server-side configuration settings.</param>
        public ProtectedResource(ILogger<PermissionResource> logger,
            KeycloakConfiguration keycloakConfiguration,
            Uma2ServerConfiguration uma2ServerConfiguration,
            IHttpClientService httpClientService)
        {
            this.logger = logger;
            this.keycloakConfiguration = keycloakConfiguration;
            this.uma2ServerConfiguration = uma2ServerConfiguration;
            this.httpClientService = httpClientService;
        }

        /// <summary>
        /// Creates a new Resource on the authorization server. See <see cref="Resource"/> class.
        /// </summary>
        /// <param name="resource">The Resource data.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>The Resource created.</returns>
        public async Task<Resource> create(Resource resource, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.ResourceRegistrationEndpoint;
            client.BaseAddress = new Uri(requestUrl);

            string jsonOutput = JsonSerializer.Serialize<Resource>(resource);

            using (HttpContent content = new StringContent(jsonOutput))
            {
                HttpResponseMessage response = await client.PostAsync(new Uri(requestUrl), content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    this.logger.LogError($"create() returned with StatusCode := {response.StatusCode}.");
                }

                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Resource resourceResponse = JsonSerializer.Deserialize<Resource>(result);
                return resourceResponse;
            }
        }

        /// <summary>
        /// Updates an existing Resource on the authorization server. See <see cref="Resource"/> class.
        /// </summary>
        /// <param name="resource">The Resource to be updated.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>True when updated.</returns>
        public async Task<bool> update(Resource resource, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.PermissionEndpoint + "/" + resource.Id;
            client.BaseAddress = new Uri(requestUrl);

            string jsonOutput = JsonSerializer.Serialize<Resource>(resource);

            using (HttpContent content = new StringContent(jsonOutput))
            {
                HttpResponseMessage response = await client.PutAsync(new Uri(requestUrl), content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    string msg = $"update() returned with StatusCode := {response.StatusCode}.";
                    this.logger.LogError(msg);
                    throw new HttpRequestException(msg);
                }
                return true;
            }
        }

        /// <summary>Deletes an existing user-managed Resource from the server.</summary>
        /// <param name="resourceId">The Resource identifier.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>True if the delete was successful.</returns>
        public async Task<bool> delete(string resourceId, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.ResourceRegistrationEndpoint + "/" + resourceId;
            client.BaseAddress = new Uri(requestUrl);

            HttpResponseMessage response = await client.DeleteAsync(new Uri(requestUrl)).ConfigureAwait(true);
            if (!response.IsSuccessStatusCode)
            {
                string msg = $"delete() returned with StatusCode := {response.StatusCode}.";
                this.logger.LogError(msg);
                throw new HttpRequestException(msg);
            }
            return true;
        }

        /// <summary>
        /// Query the server for a resource given its id.
        /// </summary>
        /// <param name="resourceId">The Resource  ID to be found.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>The Resource found.</returns>
        public async Task<Resource> findById(string resourceId, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.ResourceRegistrationEndpoint + "/" + resourceId;
            client.BaseAddress = new Uri(requestUrl);

            HttpResponseMessage response = await client.GetAsync(new Uri(requestUrl)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"findById() returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Resource resourceResponse = JsonSerializer.Deserialize<Resource>(result);
            return resourceResponse;
        }

        /// <summary>
        /// Query the server for a Resource with a given Uri.
        /// This method queries the server for resources whose
        /// </summary>
        /// <param name="resourceId">The resource ID.</param>
        /// <param name="name">The resource name.</param>
        /// <param name="uri">The resource uri.</param>
        /// <param name="owner">The resource owner.</param>
        /// <param name="type">The resource type.</param>
        /// <param name="scope">The resource scope.</param>
        /// <param name="matchingUri">Boolean to use best matching for Uri.</param>
        /// <param name="exactName">Boolean to indicate exact matching on name.</param>
        /// <param name="deep">Boolean to use deep matching.</param>
        /// <param name="firstResult">first Result index.</param>
        /// <param name="maxResult">Max Result.  -1 for no limit.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>Returns a list of Resources that best matches the given Uri.</returns>
        private async Task<List<Resource>> find(string? resourceId,
                string? name,
                string? uri,
                string? owner,
                string? type,
                string? scope,
                bool matchingUri,
                bool exactName,
                bool deep,
                Int32? firstResult,
                Int32? maxResult,
                string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.ResourceRegistrationEndpoint;
            client.BaseAddress = new Uri(requestUrl);

            if (resourceId != null)
            {
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "_id", resourceId);
            }
            if (uri != null)
            {
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "uri", uri);
            }
            if (owner != null)
            {
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "owner", owner);
            }
            if (type != null)
            {
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "type", type);
            }
            if (scope != null)
            {
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "scope", scope);
            }
            requestUrl = QueryHelpers.AddQueryString(requestUrl, "matchingUri", matchingUri.ToString());
            requestUrl = QueryHelpers.AddQueryString(requestUrl, "exactName", exactName.ToString());
            requestUrl = QueryHelpers.AddQueryString(requestUrl, "deep", deep.ToString());
            if (firstResult != null)
            {
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "first", firstResult.ToString());
            }
            if (maxResult != null)
            {
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "max", maxResult.ToString());
            }
            else
            {
                requestUrl = QueryHelpers.AddQueryString(requestUrl, "max", "-1");
            }
            HttpResponseMessage response = await client.GetAsync(new Uri(requestUrl)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"find() returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            List<Resource> resourceResponse = JsonSerializer.Deserialize<List<Resource>>(result);
            return resourceResponse;
        }

        /// <summary>
        /// Query the server for a Resource with a given Uri.
        /// This method queries the server for resources whose
        /// </summary>
        /// <param name="uri">The string url to be found.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>Returns a list of Resources that best matches the given Uri.</returns>
        public async Task<List<Resource>> findByUri(string uri, string token)
        {
            return await this.find(null, null, uri, null, null, null, false, false, true, null, null, token);
        }

        /// <summary>
        /// Query the server for a Resource with a given Uri.
        /// This method queries the server for resources whose
        /// </summary>
        /// <param name="uri">The string url to be found.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>Returns a list of Resources that best matches the given Uri.</returns>
        public async Task<List<Resource>> findByMatchingUri(string uri, string token)
        {
            return await this.find(null, null, uri, null, null, null, true, false, true, null, null, token);
        }

        /// <summary>Query the server for all resources.</summary>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns> @return an array of strings with the resource ids</returns>
        public async Task<string[]> findAll(string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.ResourceRegistrationEndpoint;
            client.BaseAddress = new Uri(requestUrl);
            requestUrl = QueryHelpers.AddQueryString(requestUrl, "deep", "false");

            HttpResponseMessage response = await client.GetAsync(new Uri(requestUrl)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"findAll() returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            string[] resourceIds = JsonSerializer.Deserialize<string[]>(result);
            return resourceIds;
        }

    }
}