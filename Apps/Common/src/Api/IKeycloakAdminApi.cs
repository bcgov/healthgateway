// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.Api
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using Refit;

    /// <summary>
    /// Provides access to the Keycloak Admin API.
    /// </summary>
    public interface IKeycloakAdminApi
    {
        /// <summary>
        /// Look up a User account by username in the Identity and Access Management system.
        /// </summary>
        /// <param name="username">The unique username (surrogate key) of the User account in Authorization Server.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>A resulting UserRepresentation object.</returns>
        [Get("/users?briefRepresentation=true&username={username}&exact=true")]
        Task<IApiResponse<UserRepresentation>> GetUser(string username, [Authorize] string token);

        /// <summary>
        /// Returns users for the role passed in.
        /// </summary>
        /// <param name="role">The requested users role.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <param name="first">The first record to return.</param>
        /// <param name="max">The maximum results to return.</param>
        /// <returns>An Enumerable of UserRepresentation objects.</returns>
        [Get("/roles/{role}/users?first={first}&max={max}")]
        Task<IApiResponse<IEnumerable<UserRepresentation>>> GetUsers(string role, [Authorize] string token, int first = 0, int max = -1);

        /// <summary>
        /// Delete a User account from the Identity and Access Management system.
        /// </summary>
        /// <param name="userId">The unique userId (surrogate key) of the User account in Authorization Server.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>Returns true when user deleted.</returns>
        [Delete("/users/{userId}")]
        Task<IApiResponse> DeleteUser(Guid userId, [Authorize] string token);
    }
}
