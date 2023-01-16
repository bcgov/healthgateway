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
        /// <returns>A list of UserRepresentation objects.</returns>
        [Get("/users?briefRepresentation=true&username={username}&exact=true")]
        Task<List<UserRepresentation>> GetUsersByUsernameAsync(string username, [Authorize] string token);

        /// <summary>
        /// Returns users for the role passed in.
        /// </summary>
        /// <param name="role">The requested users role.</param>
        /// <param name="first">The first record to return.</param>
        /// <param name="max">The maximum results to return.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>A List of UserRepresentation objects.</returns>
        [Get("/roles/{role}/users?first={first}&max={max}")]
        Task<List<UserRepresentation>> GetUsersByRoleAsync(string role, int first, int max, [Authorize] string token);

        /// <summary>
        /// Look up User accounts by username, first or last name, or email.
        /// </summary>
        /// <param name="searchCriteria">The text string to search by.</param>
        /// <param name="first">The first record to return.</param>
        /// <param name="max">The maximum results to return.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>A List of UserRepresentation objects.</returns>
        [Get("/users?briefRepresentation=true&first={first}&max={max}&search={searchCriteria}")]
        Task<List<UserRepresentation>> GetUsersSearchAsync(string searchCriteria, int first, int max, [Authorize] string token);

        /// <summary>
        /// Adds a user.
        /// </summary>
        /// <param name="userRepresentation">The UserRepresentation of new user.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>Returns true when user created.</returns>
        [Post("/users")]
        Task AddUserAsync([Body] UserRepresentation userRepresentation, [Authorize] string token);

        /// <summary>
        /// Get realm roles for user.
        /// </summary>
        /// <param name="userId">The user's id.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>A List of RoleRepresentation objects.</returns>
        [Get("/users/{userId}/role-mappings/realm")]
        Task<List<RoleRepresentation>> GetUserRolesAsync(Guid userId, [Authorize] string token);

        /// <summary>
        /// Adds realm roles to user.
        /// </summary>
        /// <param name="userId">The user's id.</param>
        /// <param name="roles">List of RoleRepresentations to add to user.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>Returns true when roles added.</returns>
        [Post("/users/{userId}/role-mappings/realm")]
        Task AddUserRolesAsync(Guid userId, [Body] IEnumerable<RoleRepresentation> roles, [Authorize] string token);

        /// <summary>
        /// Deletes realm roles from user.
        /// </summary>
        /// <param name="userId">The user's id.</param>
        /// <param name="roles">List of RoleRepresentations to delete from the user.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>Returns true when roles deleted.</returns>
        [Delete("/users/{userId}/role-mappings/realm")]
        Task DeleteUserRolesAsync(Guid userId, [Body] IEnumerable<RoleRepresentation> roles, [Authorize] string token);

        /// <summary>
        /// Get all realm roles.
        /// </summary>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>A list of RoleRepresentations.</returns>
        [Get("/roles")]
        Task<List<RoleRepresentation>> GetRealmRolesAsync([Authorize] string token);

        /// <summary>
        /// Delete a User account from the Identity and Access Management system.
        /// </summary>
        /// <param name="userId">The unique userId (surrogate key) of the User account in Authorization Server.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>Returns true when user deleted.</returns>
        [Delete("/users/{userId}")]
        Task DeleteUserAsync(Guid userId, [Authorize] string token);
    }
}
