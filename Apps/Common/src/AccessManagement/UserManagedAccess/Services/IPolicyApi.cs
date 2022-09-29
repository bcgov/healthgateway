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
namespace HealthGateway.Common.UserManagedAccess.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.UserManagedAccess.Models;
    using Refit;

    /// <summary>An entry point for managing user-managed access permissions for a particular resource.</summary>
    [Headers("Authorization: Bearer")]
    public interface IPolicyApi
    {
        /// <summary>Creates a user-managed policy.</summary>
        /// <param name="resourceId">The resource identifier of the user managed access policy.</param>
        /// <param name="permission">The uma Permission to add.</param>
        /// <param name="headers"> Http Headers, which includes the bearer token.</param>
        /// <returns>The created UmaPermission.</returns>
        [Get("/authz/protection/uma-policy/{resourceId}")]
        public Task<UmaPolicy> Create(string resourceId, [Body] UmaPolicy policy, string token);

        /// <summary>Updates an existing user-managed permission.</summary>
        /// <param name="permission">The uma Permission to update.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>True if the delete was successful.</returns>
        public Task<bool> Update([Body] UmaPermission permission, string token);

        /// <summary>Deletes an existing user-managed permission.</summary>
        /// <param name="permissionId">The uma Permission identifier.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>True if the delete was successful.</returns>
        public Task<bool> Delete(string permissionId, string token);

        /// <summary>Queries the server for permission matching the given parameters.</summary>
        /// <param name="resourceId">The resource identifier in context.</param>
        /// <param name="name">The name of the permission.</param>
        /// <param name="scope">scope the scope associated with the permission.</param>
        /// <param name="firstResult">firstResult the position of the first resource to retrieve.</param>
        /// <param name="maxResult">maxResult the maximum number of resources to retrieve.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>A list of UmaPermissions, if found.</returns>
        public Task<List<UmaPermission>> Find(
                string resourceId,
                string name,
                string scope,
                int firstResult,
                int maxResult,
                string token);

        /// <summary>Queries the server for a permission with the given ID.</summary>
        /// <param name="id">The uma permission identifier to find.</param>
        /// <param name="token"> A valid base64 access_token from authenticing the caller.</param>
        /// <returns>An UmaPermission, if found.</returns>
        public Task<UmaPermission> FindById(string id, string token);
    }
}