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
#pragma warning disable CA1054 // Uri parameters should not be strings
        Task<RequestResult<UserProfileModel>> CreateUserProfile(CreateUserRequest createProfileRequest, Uri hostUri, string bearerToken);
#pragma warning restore CA1054 // Uri parameters should not be strings

        /// <summary>
        /// Closed the user profile.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="hostUrl">The host of the email validation endpoint.</param>
        /// <returns>The wrapped user profile.</returns>
#pragma warning disable CA1054 // Uri parameters should not be strings
        RequestResult<UserProfileModel> CloseUserProfile(string hdid, Guid userId, string hostUrl);
#pragma warning restore CA1054 // Uri parameters should not be strings

        /// <summary>
        /// Recovers the user profile.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="hostUrl">The host of the email validation endpoint.</param>
        /// <returns>The wrapped user profile.</returns>
#pragma warning disable CA1054 // Uri parameters should not be strings
        RequestResult<UserProfileModel> RecoverUserProfile(string hdid, string hostUrl);
#pragma warning restore CA1054 // Uri parameters should not be strings

        /// <summary>
        /// Gets the most recent active terms of service.
        /// </summary>
        /// <returns>The wrapped terms of service.</returns>
        RequestResult<TermsOfServiceModel> GetActiveTermsOfService();
    }
}
