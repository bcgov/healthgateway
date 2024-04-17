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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
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
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The wrapped user profile.</returns>
        Task<RequestResult<UserProfileModel>> GetUserProfileAsync(string hdid, DateTime jwtAuthTime, CancellationToken ct = default);

        /// <summary>
        /// Saves the user profile to the database.
        /// </summary>
        /// <param name="createProfileRequest">The request to create a user profile model.</param>
        /// <param name="jwtAuthTime">The date of last jwt authorization time.</param>
        /// <param name="jwtEmailAddress">The email address contained by the jwt.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The wrapped user profile.</returns>
        Task<RequestResult<UserProfileModel>> CreateUserProfileAsync(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string? jwtEmailAddress, CancellationToken ct = default);

        /// <summary>
        /// Closed the user profile.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The wrapped user profile.</returns>
        Task<RequestResult<UserProfileModel>> CloseUserProfileAsync(string hdid, Guid userId, CancellationToken ct = default);

        /// <summary>
        /// Recovers the user profile.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The wrapped user profile.</returns>
        Task<RequestResult<UserProfileModel>> RecoverUserProfileAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Gets the most recent active terms of service.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The wrapped terms of service.</returns>
        Task<RequestResult<TermsOfServiceModel>> GetActiveTermsOfServiceAsync(CancellationToken ct = default);

        /// <summary>
        /// Updates a User Preference in the backend.
        /// </summary>
        /// <param name="userPreferenceModel">The user preference to update.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A userPreference wrapped in a RequestResult.</returns>
        Task<RequestResult<UserPreferenceModel>> UpdateUserPreferenceAsync(UserPreferenceModel userPreferenceModel, CancellationToken ct = default);

        /// <summary>
        /// Create a User Preference in the backend.
        /// </summary>
        /// <param name="userPreferenceModel">The user preference to create.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A userPreference wrapped in a RequestResult.</returns>
        Task<RequestResult<UserPreferenceModel>> CreateUserPreferenceAsync(UserPreferenceModel userPreferenceModel, CancellationToken ct = default);

        /// <summary>
        /// Gets a value indicating if the patient age is valid for registration.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A boolean result.</returns>
        Task<RequestResult<bool>> ValidateMinimumAgeAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Updates the user profile and sets the accepted terms of service to the supplied value.
        /// </summary>
        /// <param name="hdid">The users hdid.</param>
        /// <param name="termsOfServiceId">The terms of service id accepted.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A user profile model wrapped in a RequestResult.</returns>
        Task<RequestResult<UserProfileModel>> UpdateAcceptedTermsAsync(string hdid, Guid termsOfServiceId, CancellationToken ct = default);

        /// <summary>
        /// Validates a phone number against the system wide accepted number validation logic.
        /// </summary>
        /// <param name="phoneNumber">This should be a phone number without a mask.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>True if the phone number is valid.</returns>
        Task<bool> IsPhoneNumberValidAsync(string phoneNumber, CancellationToken ct = default);
    }
}
