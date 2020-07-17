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
namespace HealthGateway.WebClient.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Models;

    /// <summary>
    /// The User Profile service.
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Gets the user profile model.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="lastLogin">The date of last login performed by the user.</param>
        /// <returns>The wrappeed user profile.</returns>
        RequestResult<UserProfileModel> GetUserProfile(string hdid, DateTime? lastLogin = null);

        /// <summary>
        /// Saves the user profile to the database.
        /// </summary>
        /// <param name="createProfileRequest">The request to create a user profile model.</param>
        /// <param name="hostUri">The host of the email validation endpoint.</param>
        /// <param name="bearerToken">The access token of the authenticated user.</param>
        /// <returns>The wrapped user profile.</returns>
        RequestResult<UserProfileModel> CreateUserProfile(CreateUserRequest createProfileRequest, Uri hostUri, string bearerToken);

        /// <summary>
        /// Closed the user profile.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="hostUrl">The host of the email validation endpoint.</param>
        /// <returns>The wrapped user profile.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team Decision")]
        RequestResult<UserProfileModel> CloseUserProfile(string hdid, Guid userId, string hostUrl);

        /// <summary>
        /// Recovers the user profile.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="hostUrl">The host of the email validation endpoint.</param>
        /// <returns>The wrapped user profile.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team Decision")]
        RequestResult<UserProfileModel> RecoverUserProfile(string hdid, string hostUrl);

        /// <summary>
        /// Gets the most recent active terms of service.
        /// </summary>
        /// <returns>The wrapped terms of service.</returns>
        RequestResult<TermsOfServiceModel> GetActiveTermsOfService();

        /// <summary>
        /// Creates a User Preference in the backend.
        /// </summary>
        /// <param name="userPreference">The userPreference to create.</param>
        /// <returns>A userPreference wrapped in a RequestResult.</returns>
        bool UpdateUserPreference(string hdid, string name, string value);

        /// <summary>
        /// Gets the user preference model.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <returns>The wrappeed user reference.</returns>
        RequestResult<Dictionary<string, string>> GetUserPreferences(string hdid);
    }
}
