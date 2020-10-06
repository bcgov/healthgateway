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
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation;

    /// <summary>
    /// An entry point for obtaining permissions from the server.
    /// </summary>
    public interface IPermissionResource
    {
        /// <summary>Creates a new permission ticket for a single resource and scope(s).</summary>
        /// <param name="request"> the <cref name="PermissionRequest"/> representing the resource and scope(s).</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>Permission response holding a permission ticket with the requested permissions.</returns>
        public Task<PermissionResponse> create(PermissionRequest request, string token);

        /// <summary>Creates new permission ticket(s) for a set of resources and scope(s).</summary>
        /// <param name="requests"> a List of <cref name="PermissionRequest"/> representing the resource and scope(s).</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>Permission response holding a permission ticket with the requested permissions.</returns>
        public Task<PermissionResponse> create(List<PermissionRequest> requests, string token);

        /// <summary>Creates a new uma permission for a single resource and scope(s).</summary>
        /// <param name="ticket">The <cref name="PermissionTicketRepresentation"/> representing the resource and scope(s).</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>A permission response holding the permission ticket representation</returns>
        public Task<PermissionTicket> create(PermissionTicket ticket, string token);

        /// <summary>Query the server for any permission ticket associated with the given scopeId.</summary>
        /// <param name="scopeId">The scopeId the scope id</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>A list of permission tickets associated with the given scopeId</returns>
        public Task<List<PermissionTicket>> findByScope(string scopeId, string token);

        /// <summary>Query the server for any permission ticket associated with the given scopeId.</summary>
        /// <param name="resourceId">The resourceId.</param>
        /// <param name="token">A valid base64 access_token from authenticing the caller.</param>
        /// <returns>A list of permission tickets associated with the given scopeId</returns>
        public Task<List<PermissionTicket>> findByResourceId(string resourceId, string token);

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
        public Task<List<PermissionTicket>> find(
            string resourceId,
            string scopeId,
            string owner,
            string requester,
            bool granted,
            bool returnNames,
            int firstResult,
            int maxResult,
            string token);

        /// <summary>Updates a permission ticket.</summary>
        /// <param name="ticket">The permission ticket</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        public Task<bool> update(PermissionTicket ticket, string token);

        /// <summary>Delete a permission ticket.</summary>
        /// <param name="ticketId">The id of the permission ticket to delete.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>True if the delete succeeded.</returns>
        public Task<bool> delete(string ticketId, string token);
    }
}