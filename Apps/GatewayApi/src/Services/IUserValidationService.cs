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

    /// <summary>
    /// The user validation service.
    /// </summary>
    public interface IUserValidationService
    {
        /// <summary>
        /// Determines whether a phone number is valid.
        /// </summary>
        /// <param name="phoneNumber">Phone number stripped of any mask characters.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A boolean value indicating whether the phone number is valid.</returns>
        Task<bool> IsPhoneNumberValidAsync(string phoneNumber, CancellationToken ct = default);

        /// <summary>
        /// Determines whether a user is eligible to create a Health Gateway account.
        /// </summary>
        /// <param name="hdid">The requested user HDID.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A boolean result.</returns>
        Task<bool> ValidateEligibilityAsync(string hdid, CancellationToken ct = default);
    }
}
