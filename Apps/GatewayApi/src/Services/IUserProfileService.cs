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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// The User Profile service.
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Gets the user profile model.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="jwtAuthTime">The date of last jwt authorization time.</param>
        /// <returns>The wrappeed user profile.</returns>
        RequestResult<UserProfileModel> GetUserProfile(string hdid, DateTime jwtAuthTime);

        /// <summary>
        /// Saves the user profile to the database.
        /// </summary>
        /// <param name="createProfileRequest">The request to create a user profile model.</param>
        /// <param name="jwtAuthTime">The date of last jwt authorization time.</param>
        /// <param name="jwtEmailAddress">The email address contained by the jwt.</param>
        /// <returns>The wrapped user profile.</returns>
        Task<RequestResult<UserProfileModel>> CreateUserProfile(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string jwtEmailAddress);

        /// <summary>
        /// Closed the user profile.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>The wrapped user profile.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team Decision")]
        RequestResult<UserProfileModel> CloseUserProfile(string hdid, Guid userId);

        /// <summary>
        /// Recovers the user profile.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <returns>The wrapped user profile.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team Decision")]
        RequestResult<UserProfileModel> RecoverUserProfile(string hdid);

        /// <summary>
        /// Gets the most recent active terms of service.
        /// </summary>
        /// <returns>The wrapped terms of service.</returns>
        RequestResult<TermsOfServiceModel> GetActiveTermsOfService();

        /// <summary>
        /// Updates a User Preference in the backend.
        /// </summary>
        /// <param name="userPreferenceModel">The user preference to update.</param>
        /// <returns>A userPreference wrapped in a RequestResult.</returns>
        RequestResult<UserPreferenceModel> UpdateUserPreference(UserPreferenceModel userPreferenceModel);

        /// <summary>
        /// Create a User Preference in the backend.
        /// </summary>
        /// <param name="userPreferenceModel">The user preference to create.</param>
        /// <returns>A userPreference wrapped in a RequestResult.</returns>
        RequestResult<UserPreferenceModel> CreateUserPreference(UserPreferenceModel userPreferenceModel);

        /// <summary>
        /// Gets the user preference model.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <returns>The wrappeed user reference.</returns>
        RequestResult<Dictionary<string, UserPreferenceModel>> GetUserPreferences(string hdid);

        /// <summary>
        /// Gets a value indicating if the patient age is valid for registration.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <returns>A boolean result.</returns>
        Task<PrimitiveRequestResult<bool>> ValidateMinimumAge(string hdid);

        /// <summary>
        /// Updates the user profile and sets the acceepted terms of service to the supplied value.
        /// </summary>
        /// <param name="hdid">The users hdid.</param>
        /// <param name="termsOfServiceId">The terms of service id accepted.</param>
        /// <returns>A user profile model wrapped in a RequestResult.</returns>
        RequestResult<UserProfileModel> UpdateAcceptedTerms(string hdid, Guid termsOfServiceId);
    }
}
