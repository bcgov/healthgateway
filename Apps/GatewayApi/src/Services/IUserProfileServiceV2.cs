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
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// The user profile service.
    /// </summary>
    public interface IUserProfileServiceV2
    {
        /// <summary>
        /// Gets a user profile.
        /// </summary>
        /// <param name="hdid">The requested user HDID.</param>
        /// <param name="jwtAuthTime">The authenticated login time from the JWT.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The user profile.</returns>
        Task<UserProfileModel> GetUserProfileAsync(string hdid, DateTime jwtAuthTime, CancellationToken ct = default);

        /// <summary>
        /// Creates a user profile.
        /// </summary>
        /// <param name="createProfileRequest">The request to create a user profile model.</param>
        /// <param name="jwtAuthTime">The authenticated login time from the JWT.</param>
        /// <param name="jwtEmailAddress">The email address contained in the JWT.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The user profile.</returns>
        Task<UserProfileModel> CreateUserProfileAsync(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string? jwtEmailAddress, CancellationToken ct = default);

        /// <summary>
        /// Closes a user profile.
        /// </summary>
        /// <param name="hdid">The requested user HDID.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CloseUserProfileAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Recovers a user profile.
        /// </summary>
        /// <param name="hdid">The requested user HDID.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RecoverUserProfileAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Updates a user's profile to capture approval of the terms of service.
        /// </summary>
        /// <param name="hdid">The user HDID.</param>
        /// <param name="termsOfServiceId">The ID of the terms of service to approve.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateAcceptedTermsAsync(string hdid, Guid termsOfServiceId, CancellationToken ct = default);
    }
}
