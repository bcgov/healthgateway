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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// The User Profile Validator service.
    /// </summary>
    public interface IUserProfileValidatorService
    {
        /// <summary>
        /// Validates a phone number against the system wide accepted number validation logic.
        /// </summary>
        /// <param name="phoneNumber">This should be a phone number without a mask.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>True if the phone number is valid.</returns>
        Task<bool> IsPhoneNumberValidAsync(string phoneNumber, CancellationToken ct = default);

        /// <summary>
        /// Gets a value indicating if the patient age is valid for registration.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A boolean result.</returns>
        Task<RequestResult<bool>> ValidateMinimumAgeAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Validates a create profile request.
        /// </summary>
        /// <param name="createProfileRequest">The request to create a user profile model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A user profile model wrapped in a Request Result.</returns>
        Task<RequestResult<UserProfileModel>?> ValidateUserProfileAsync(CreateUserRequest createProfileRequest, CancellationToken ct = default);
    }
}
