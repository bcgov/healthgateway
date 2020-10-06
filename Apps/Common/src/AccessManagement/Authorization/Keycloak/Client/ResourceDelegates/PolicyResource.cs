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
    /// <summary>An entry point for managing user-managed permissions for a particular resource.</summary>
    ///
    public class PolicyResource : IPolicyResource
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;

        private readonly IServerConfigurationDelegate serverConfigurationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationResource"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="serverConfigurationDelegate">The UMA serverConfiguration delegate.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        public PolicyResource(ILogger<PermissionResource> logger,
            ServerConfigurationDelegate serverConfigurationDelegate,
            HttpClientService httpClientService)
        {
            this.logger = logger;
            this.serverConfigurationDelegate = serverConfigurationDelegate;
            this.httpClientService = httpClientService;
        }

        /// <inherited/>
        public async Task<UmaPermission> create(string resourceId, UmaPermission permission, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.serverConfigurationDelegate.ServerConfiguration.PolicyEndpoint + "/" + resourceId;
            client.BaseAddress = new Uri(requestUrl);

            string jsonOutput = JsonSerializer.Serialize<UmaPermission>(permission);

            using (HttpContent content = new StringContent(jsonOutput))
            {
                HttpResponseMessage response = await client.PostAsync(new Uri(requestUrl), content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    this.logger.LogError($"create() returned with StatusCode := {response.StatusCode}.");
                }

                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                UmaPermission umaPermissionResponse = JsonSerializer.Deserialize<UmaPermission>(result);
                return umaPermissionResponse;
            }
        }

        /// <inherited/>
        public async Task<bool> update(UmaPermission permission, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.serverConfigurationDelegate.ServerConfiguration.PermissionEndpoint + "/" + permission.Id;
            client.BaseAddress = new Uri(requestUrl);

            string jsonOutput = JsonSerializer.Serialize<UmaPermission>(permission);

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

        /// <inherited/>
        public async Task<bool> delete(string permissionId, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.serverConfigurationDelegate.ServerConfiguration.PermissionEndpoint + "/" + permissionId;
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

        /// <inherited/>
        public async Task<List<UmaPermission>> find(string resourceId,
                string name,
                string scope,
                int firstResult,
                int maxResult,
                string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.serverConfigurationDelegate.ServerConfiguration.PermissionEndpoint + "/ticket";
            client.BaseAddress = new Uri(requestUrl);

            requestUrl = QueryHelpers.AddQueryString(requestUrl, "name", name);
            requestUrl = QueryHelpers.AddQueryString(requestUrl, "resource", resourceId);
            requestUrl = QueryHelpers.AddQueryString(requestUrl, "scope", scope);
            requestUrl = QueryHelpers.AddQueryString(requestUrl, "first", firstResult.ToString());
            requestUrl = QueryHelpers.AddQueryString(requestUrl, "max", maxResult.ToString());

            HttpResponseMessage response = await client.GetAsync(new Uri(requestUrl)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"find() returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            List<UmaPermission> umaPermissions = JsonSerializer.Deserialize<List<UmaPermission>>(result);
            return umaPermissions;
        }

        /// <inherited/>
        public async Task<UmaPermission> findById(string id, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.serverConfigurationDelegate.ServerConfiguration.PermissionEndpoint + "/" + id;
            client.BaseAddress = new Uri(requestUrl);

            HttpResponseMessage response = await client.GetAsync(new Uri(requestUrl)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"findById() returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            UmaPermission umaPermission = JsonSerializer.Deserialize<UmaPermission>(result);
            return umaPermission;
        }
    }
}