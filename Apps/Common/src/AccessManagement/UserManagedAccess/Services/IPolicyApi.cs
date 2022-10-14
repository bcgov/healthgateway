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
    using Refit;

    /// <summary>An entry point for managing user-managed access permissions for a particular resource.</summary>
    [Headers("Authorization: Bearer")]
    public interface IPolicyApi
    {
        /// <summary>Creates a user-managed policy.</summary>
        /// <param name="resourceId">The resource identifier of the user managed access policy.</param>
        /// <param name="policy">The uma Permission to add.</param>
        /// <returns>The created UmaPermission.</returns>
        [Post("/authz/protection/uma-policy/{resourceId}")]
        public Task<AccessPolicy> Create(string resourceId, [Body] AccessPolicy policy);

        /// <summary>Updates an existing user-managed permission.</summary>
        /// <param name="permission">The uma Permission to update.</param>
        /// <returns>True if the delete was successful.</returns>
        [Put("/uathz/protection/uma-policy/")]
        public Task<bool> Update([Body] UmaPermission permission);

        /// <summary>Deletes an existing user-managed permission.</summary>
        /// <param name="permissionId">The uma Permission identifier.</param>
        /// <returns>True if the delete was successful.</returns>
        [Delete("/authz/protection/uma-policy/{permissionId}")]
        public Task<bool> Delete(string permissionId);

        /// <summary>Queries the server for permission matching the given parameters.</summary>
        /// <param name="resourceId">The resource identifier in context.</param>
        /// <param name="name">The name of the permission.</param>
        /// <param name="scope">scope the scope associated with the permission.</param>
        /// <param name="firstResult">firstResult the position of the first resource to retrieve.</param>
        /// <param name="maxResult">maxResult the maximum number of resources to retrieve.</param>
        /// <returns>A list of UmaPermissions, if found.</returns>
        [Get("/uathz/protection/uma-policy?rsid={resourceId}")]
        public Task<List<UmaPermission>> Find(
                string resourceId,
                string name,
                string scope,
                int firstResult,
                int maxResult,
                string token);

        /// <summary>Queries the server for a permission with the given ID.</summary>
        /// <param name="id">The uma permission identifier to find.</param>
        /// <returns>An UmaPermission, if found.</returns>
        [Get("/uathz/protection/uma-policy?permissionId={id}")]
        public Task<UmaPermission> FindById(string id, string token);
    }
}