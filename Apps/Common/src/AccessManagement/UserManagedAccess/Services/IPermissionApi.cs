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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.UserManagedAccess.Models;
    using HealthGateway.Common.AccessManagement.UserManagedAccess.Models.Tokens;

    using Refit;

    /// <summary>
    /// An entry point for obtaining permissions from the server.
    /// </summary>
    [Headers("Authorization: Bearer")]
    public interface IPermissionApi
    {
        /// <summary>Creates a new permission ticket for a single resource and scope(s).</summary>
        /// <param name="request"> the <see cref="PermissionRequest"/> representing the resource and scope(s).</param>
        /// <returns>Permission response holding a permission ticket with the requested permissions.</returns>
        [Post("/authz/protection/permission")]
        public Task<PermissionResponse> Create([Body] PermissionRequest request);

        /// <summary>Creates new permission ticket(s) for a set of resources and scope(s).</summary>
        /// <param name="requests"> a List of <see cref="PermissionRequest"/> representing the resource and scope(s).</param>
        /// <returns>Permission response holding a permission ticket with the requested permissions.</returns>
        [Post("/authz/protection/permission")]
        public Task<PermissionResponse> Create([Body] ICollection<PermissionRequest> requests);

        /// <summary>Creates a new uma permission for a single resource and scope(s).</summary>
        /// <param name="ticket">The <see cref="PermissionTicket"/> representing the resource and scope(s).</param>
        /// <returns>A permission response holding the permission ticket representation.</returns>
        [Post("/authz/protection/permission")]
        public Task<PermissionTicket> Create([Body] PermissionTicket ticket);

        /// <summary>Query the server for any permission ticket associated with the given scopeId.</summary>
        /// <param name="scopeId">The scopeId the scope id.</param>
        /// <returns>A list of permission tickets associated with the given scopeId.</returns>
        [Get("/authz/protection/permission?scopeId={scopeId}")]
        public Task<ICollection<PermissionTicket>> FindByScope(string scopeId);

        /// <summary>Query the server for any permission ticket associated with the given scopeId.</summary>
        /// <param name="resourceId">The resourceId.</param>
        /// <returns>A list of permission tickets associated with the given scopeId.</returns>
        [Get("/authz/protection/permission?resourceId={resourceId}")]
        public Task<ICollection<PermissionTicket>> FindByResourceId(string resourceId);

        /// <summary>Query the server for any permission ticket with the matching arguments.</summary>
        /// <param name="resourceId">The resource id or name.</param>
        /// <param name="scopeId">The scope id or name.</param>
        /// <param name="owner">The owner id or name.</param>
        /// <param name="requester">The requester id or name.</param>
        /// <param name="granted">If true, only permission tickets marked as granted are returned.</param>
        /// <param name="returnNames">If the response should include names for resource, scope and owner.</param>
        /// <param name="firstResult">The position of the first resource to retrieve.</param>
        /// <param name="maxResult">The maximum number of resources to retrieve.</param>
        /// <returns>A list of permission tickets with the matching arguments.</returns>
        [Get("/authz/protection/permission?resourceId={resourceId}&scopeId={scopeId}&owner={owner}&requester={requester}&granted={granted}&returnName={returnNames}&firstResult={firstResult}&maxResult={maxResult}")]
        public Task<ICollection<PermissionTicket>> Find(
            string resourceId,
            string scopeId,
            string owner,
            string requester,
            bool granted,
            bool returnNames,
            int firstResult,
            int maxResult);

        /// <summary>Updates a permission ticket.</summary>
        /// <param name="ticket">The permission ticket.</param>
        /// <returns>True when the permission ticket is updated.</returns>
        [Put("/authz/protection/permission")]
        public Task<bool> Update([Body] PermissionTicket ticket);

        /// <summary>Delete a permission ticket.</summary>
        /// <param name="ticketId">The id of the permission ticket to delete.</param>
        /// <returns>True if the delete succeeded.</returns>
        [Delete("/authz/protection/permission?ticketId={ticketId}")]
        public Task<bool> Delete(string ticketId);
    }
}
