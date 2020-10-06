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

    using Microsoft.Extensions.Logging;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Util;

    using HealthGateway.Common.Services;
    ///
    /// <summary>An entry point for managing permission tickets using the Protection API.</summary>
    ///
    public class PermissionResource
    {
        private readonly ILogger logger;

        private readonly KeycloakConfiguration keycloakConfiguration;

        private readonly Uma2ServerConfiguration uma2ServerConfiguration;


        /// <summary>The injected HttpClientService.</summary>
        private readonly IHttpClientService httpClientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationResource"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="keycloakConfiguration">The keyCloak configuration.</param>
        /// <param name="uma2ServerConfiguration">The UMA configuration.</param>
        public PermissionResource(ILogger<PermissionResource> logger,
            KeycloakConfiguration keycloakConfiguration,
            Uma2ServerConfiguration uma2ServerConfiguration,
            IHttpClientService httpClientService)
        {
            this.logger = logger;
            this.keycloakConfiguration = keycloakConfiguration;
            this.uma2ServerConfiguration = uma2ServerConfiguration;
            this.httpClientService = httpClientService;
        }

        /// <summary>Creates a new permission ticket for a single resource and scope(s).</summary>
        /// <param name="request"> the <cref name="PermissionRequest"/> representing the resource and scope(s).</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>Permission response holding a permission ticket with the requested permissions.</returns>
        public async Task<PermissionResponse> create(PermissionRequest request, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.PermissionEndpoint;
            client.BaseAddress = new Uri(requestUrl);

            string jsonOutput = JsonSerializer.Serialize<PermissionRequest>(request);

            using (HttpContent content = new StringContent(jsonOutput))
            {
                HttpResponseMessage response = await client.PostAsync(new Uri(requestUrl), content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    this.logger.LogError($"createPermissionTicket(PermissionRequest ) returned with StatusCode := {response.StatusCode}.");
                }

                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                PermissionResponse permissionResponse = JsonSerializer.Deserialize<PermissionResponse>(result);
                return permissionResponse;
            }
        }

        /// <summary>Creates new permission ticket(s) for a set of resources and scope(s).</summary>
        /// <param name="requests"> a List of <cref name="PermissionRequest"/> representing the resource and scope(s).</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>Permission response holding a permission ticket with the requested permissions.</returns>
        public async Task<PermissionResponse> create(List<PermissionRequest> requests, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.PermissionEndpoint;
            client.BaseAddress = new Uri(requestUrl);

            string jsonOutput = JsonSerializer.Serialize<List<PermissionRequest>>(requests);

            using (HttpContent content = new StringContent(jsonOutput))
            {
                HttpResponseMessage response = await client.PostAsync(new Uri(requestUrl), content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    this.logger.LogError($"createPermissionTicket(List<PermissionRequest> ) returned with StatusCode := {response.StatusCode}.");
                }

                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                PermissionResponse permissionResponse = JsonSerializer.Deserialize<PermissionResponse>(result);
                return permissionResponse;
            }
        }

        /// <summary>Creates a new uma permission for a single resource and scope(s).</summary>
        /// <param name="ticket">The <cref name="PermissionTicketRepresentation"/> representing the resource and scope(s).</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>A permission response holding the permission ticket representation</returns>
        public async Task<PermissionTicket> create(PermissionTicket ticket, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.PermissionEndpoint;
            client.BaseAddress = new Uri(requestUrl);

            string jsonOutput = JsonSerializer.Serialize<PermissionTicket>(ticket);

            using (HttpContent content = new StringContent(jsonOutput))
            {
                HttpResponseMessage response = await client.PostAsync(new Uri(requestUrl), content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    this.logger.LogError($"createPermissionTicket(PermissionTicket ) returned with StatusCode := {response.StatusCode}.");
                }

                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                PermissionTicket permissionResponse = JsonSerializer.Deserialize<PermissionTicket>(result);
                return permissionResponse;
            }
        }

        /// <summary>Query the server for any permission ticket associated with the given scopeId.</summary>
        /// <param name="scopeId">The scopeId the scope id</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>A list of permission tickets associated with the given scopeId</returns>
        public async Task<List<PermissionTicket>> findByScope(string scopeId, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.PermissionEndpoint + "/ticket";
            client.BaseAddress = new Uri(requestUrl);

            UriBuilder builder = new UriBuilder(requestUrl);
            builder.Query = "scopeId=" + scopeId;

            HttpResponseMessage response = await client.GetAsync(builder.Uri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"createPermissionTicket(PermissionTicket ) returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            List<PermissionTicket> permissionTickets = JsonSerializer.Deserialize<List<PermissionTicket>>(result);
            return permissionTickets;

        }

        /// <summary>Query the server for any permission ticket associated with the given scopeId.</summary>
        /// <param name="resourceId">The resourceId.</param>
        /// <param name="token">A valid base64 access_token from authenticing the caller.</param>
        /// <returns>A list of permission tickets associated with the given scopeId</returns>
        public async Task<List<PermissionTicket>> findByResourceId(string resourceId, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.PermissionEndpoint + "/ticket";
            client.BaseAddress = new Uri(requestUrl);

            UriBuilder builder = new UriBuilder(requestUrl);
            builder.Query = "resourceId=" + resourceId;

            HttpResponseMessage response = await client.GetAsync(builder.Uri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"createPermissionTicket(PermissionTicket ) returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            List<PermissionTicket> permissionTickets = JsonSerializer.Deserialize<List<PermissionTicket>>(result);
            return permissionTickets;
        }

        /// <summary>Query the server for any permission ticket with the matching arguments.</summary>
        /// <param name="resourceId">The resource id or name.</param>
        /// <param name="scopeId">The scope id or name.</param>
        /// <param name="owner">The owner id or name.</param>
        /// <param name="requester">The requester id or name.</param>
        /// <param name="granted">If true, only permission tickets marked as granted are returned.</param>
        /// <param name="returnNames">If the response should include names for resource, scope and owner.</param>
        /// <param name="firstResult">The position of the first resource to retrieve.</param>
        /// <param name="maxResult">The maximum number of resources to retrieve.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>A list of permission tickets with the matching arguments.</returns>
        public async Task<List<PermissionTicket>> find(
            string resourceId,
            string scopeId,
            string owner,
            string requester,
            bool granted,
            bool returnNames,
            int firstResult,
            int maxResult,
            string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.PermissionEndpoint + "/ticket";
            client.BaseAddress = new Uri(requestUrl);

            UriBuilder builder = new UriBuilder(requestUrl);
            builder.Query = "resourceId=" + resourceId +
                "&scopeId" + scopeId +
                "&owner" + owner +
                "&requester" + requester +
                "&granted" + granted.ToString() +
                "&returnNames" + returnNames.ToString() +
                "&first" + firstResult +
                "&max" + maxResult;

            HttpResponseMessage response = await client.GetAsync(builder.Uri).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"createPermissionTicket(PermissionTicket ) returned with StatusCode := {response.StatusCode}.");
            }

            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            List<PermissionTicket> permissionTickets = JsonSerializer.Deserialize<List<PermissionTicket>>(result);
            return permissionTickets;
        }

        /// <summary>Updates a permission ticket.</summary>
        /// <param name="ticket">The permission ticket</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        public async Task<bool> update(PermissionTicket ticket, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.PermissionEndpoint + "/ticket";
            client.BaseAddress = new Uri(requestUrl);

            string jsonOutput = JsonSerializer.Serialize<PermissionTicket>(ticket);

            using (HttpContent content = new StringContent(jsonOutput))
            {
                HttpResponseMessage response = await client.PutAsync(new Uri(requestUrl), content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    string msg = $"updatePermissionTicket() returned with StatusCode := {response.StatusCode}.";
                    this.logger.LogError(msg);
                    throw new HttpRequestException(msg);
                }
                return true;
            }
        }

        /// <summary>Delete a permission ticket.</summary>
        /// <param name="ticketId">The id of the permission ticket to delete.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>True if the delete succeeded.</returns>
        public async Task<bool> delete(string ticketId, string token)
        {
            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BearerTokenAuthorization(token);
            string requestUrl = this.uma2ServerConfiguration.PermissionEndpoint + "/ticket/" + ticketId;
            client.BaseAddress = new Uri(requestUrl);

            HttpResponseMessage response = await client.DeleteAsync(new Uri(requestUrl)).ConfigureAwait(true);
            if (!response.IsSuccessStatusCode)
            {
                string msg = $"deletePermissionTicket() returned with StatusCode := {response.StatusCode}.";
                this.logger.LogError(msg);
                throw new HttpRequestException(msg);
            }
            return true;
        }
    }
}