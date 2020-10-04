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
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Representation;
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

        private readonly TokenCallable pat;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationResource"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">injected HTTP client service.</param>
        /// <param name="keycloakConfiguration">The keyCloak configuration.</param>
        /// <param name="uma2ServerConfiguration">The UMA configuration.</param>
        /// <param name="pat">A <cref name="TokenCallable"/> pat.</param>
        public PermissionResource(ILogger<PermissionResource> logger,
            KeycloakConfiguration keycloakConfiguration,
            Uma2ServerConfiguration uma2ServerConfiguration,
            IHttpClientService httpClientService,
            TokenCallable pat)
        {
            this.logger = logger;
            this.keycloakConfiguration = keycloakConfiguration;
            this.uma2ServerConfiguration = uma2ServerConfiguration;
            this.httpClientService = httpClientService;
            this.pat = pat;
        }

        /// <summary>Creates a new permission ticket for a single resource and scope(s).</summary>
        /// <param name="request"> the <cref name="PermissionRequest"/> representing the resource and scope(s).</param>
        /// <returns>Permission response holding a permission ticket with the requested permissions.</returns>
        public async Task<PermissionResponse>  createPermissionTicket(PermissionRequest request)
        {

            HttpClient client = this.httpClientService.CreateDefaultHttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            // We need to get a Bearer Token to make this call. Use the CallablePat
            pat.
            //client.DefaultRequestHeaders.Add("Authorization", @"Bearer " + request.Token);
            string requestUri = this.uma2ServerConfiguration.PermissionEndpoint;
            client.BaseAddress = new Uri(requestUri);

            string jsonOutput = JsonSerializer.Serialize<PermissionRequest>(request);

            using (HttpContent content = new StringContent(jsonOutput))
            {
                HttpResponseMessage response = await client.PostAsync(new Uri(requestUri), content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    this.logger.LogError($"AuthorizationResource.authorize() returned with StatusCode := {response.StatusCode}.");
                }

                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                PermissionResponse permissionResponse = JsonSerializer.Deserialize<PermissionResponse>(result);
                return permissionResponse;
            }
        }

        /// <summary>Creates new permission ticket(s) for a set of resources and scope(s).</summary>
        /// <param name="requests"> a List of <cref name="PermissionRequest"/> representing the resource and scope(s).</param>
        /// <returns>Permission response holding a permission ticket with the requested permissions.</returns>
        public PermissionResponse createPermissionTicket(List<PermissionRequest> requests)
        {

        }

        /// Creates a new uma permission for a single resource and scope(s).
        /// <param name="ticket" the <cref name="PermissionTicketRepresentation"/> representing the resource and scope(s).</param>
        /// <returns>A permission response holding the permission ticket representation</returns>
        public PermissionTicket createPermissionTicket(PermissionTicket ticket)
        {

        }

        /// <summary>Query the server for any permission ticket associated with the given scopeId.</summary>
        /// <param name="param">The scopeId the scope id</param>
        /// <returns>A list of permission tickets associated with the given scopeId</returns>
        public List<PermissionTicket> findByResourceId(string resourceId)
        {

        }

        /// Query the server for any permission ticket with the matching arguments.
        ///
        /// <param name="resourceId">The resource id or name.</param>
        /// <param name="scopeId">The scope id or name.</param>
        /// <param name="owner">The owner id or name.</param>
        /// <param name="requester">The requester id or name.</param>
        /// <param name="granted">If true, only permission tickets marked as granted are returned.</param>
        /// <param name="returnNames">If the response should include names for resource, scope and owner.</param>
        /// <param name="firstResult">The position of the first resource to retrieve.</param>
        /// <param name="maxResult">The maximum number of resources to retrieve.</param>
        /// <returns>A list of permission tickets with the matching arguments.</returns>
        public List<PermissionTicket> findPermissionTicket(
            string resourceId,
            string scopeId,
            string owner,
            string requester,
            bool granted,
            bool returnNames,
            int firstResult,
            int maxResult)
        {

        }

        /// <summary>Updates a permission ticket.</summary>
        /// <param name="ticket">The permission ticket</param>
        public void updatePermissionTicket(PermissionTicket ticket)
        {
        }

        /// Delete a permission ticket
        /// <param name="ticketId">The id of the permission ticket to delete.</param>
        public void deletePermissionTicket(string ticketId)
        {
        }
    }
}