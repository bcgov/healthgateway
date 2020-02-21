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
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
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
        /// <returns>The wrappeed user profile.</returns>
        RequestResult<UserProfileModel> GetUserProfile(string hdid);

        /// <summary>
        /// Saves the user profile to the database.
        /// </summary>
        /// <param name="createProfileRequest">The request to create a user profile model.</param>
        /// <param name="hostUri">The host of the email validation endpoint.</param>
        /// <returns>The wrapped user profile.</returns>
        RequestResult<UserProfileModel> CreateUserProfile(CreateUserRequest createProfileRequest, Uri hostUri);

        /// <summary>
        /// Gets the most recent updated terms of service.
        /// </summary>
        /// <returns>The wrapped terms of service.</returns>
        RequestResult<TermsOfServiceModel> GetLastTermsOfService();
    }
}
