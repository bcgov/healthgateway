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
    using HealthGateway.Database.Models;

    /// <summary>
    /// The user email service.
    /// </summary>
    public interface IUserEmailServiceV2
    {
        /// <summary>
        /// Verifies an email address using the given invite key.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="inviteKey">The email invite key.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A boolean value indicating whether the verification was successful.</returns>
        Task<bool> VerifyEmailAddressAsync(string hdid, Guid inviteKey, CancellationToken ct = default);

        /// <summary>
        /// Updates user's email address.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="emailAddress">Email address to be set for the user.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateEmailAddressAsync(string hdid, string emailAddress, CancellationToken ct = default);

        /// <summary>
        /// Generates a messaging verification and email template using the provided HDID, email address, and invite key.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="emailAddress">Email address to verify.</param>
        /// <param name="inviteKey">The email invite key.</param>
        /// <param name="isVerified">
        /// If the address is already verified, the verification will be marked as already validated
        /// and the generated email will be marked as already sent.
        /// </param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>The generated messaging verification.</returns>
        Task<MessagingVerification> GenerateMessagingVerificationAsync(string hdid, string emailAddress, Guid inviteKey, bool isVerified, CancellationToken ct = default);
    }
}
