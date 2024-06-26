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

    /// <summary>
    /// The user email service.
    /// </summary>
    public interface IUserEmailServiceV2
    {
        /// <summary>
        /// Validates an email address using the given invite key.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="inviteKey">The email invite key.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>Returns a boolean value indicating whether the validation was successful.</returns>
        Task<bool> ValidateEmailAsync(string hdid, Guid inviteKey, CancellationToken ct = default);

        /// <summary>
        /// Initializes user's email address.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="emailAddress">Email address to be set for the user.</param>
        /// <param name="isVerified">Indicates whether the email address is verified.</param>
        /// <param name="commit">If set to true the changes to database are persisted immediately.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>returns true if the email was successfully created.</returns>
        Task<bool> CreateUserEmailAsync(string hdid, string emailAddress, bool isVerified, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Updates the user email.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="emailAddress">Email address to be set for the user.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateUserEmailAsync(string hdid, string emailAddress, CancellationToken ct = default);
    }
}
