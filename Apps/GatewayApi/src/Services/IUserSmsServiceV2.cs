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
    /// The user SMS service.
    /// </summary>
    public interface IUserSmsServiceV2
    {
        /// <summary>
        /// Verifies an SMS number for a user with the given verification code.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="verificationCode">The SMS verification code.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A value indicating whether the verification was successful.</returns>
        Task<bool> VerifySmsNumberAsync(string hdid, string verificationCode, CancellationToken ct = default);

        /// <summary>
        /// Updates the user SMS number.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="sms">SMS number to be set for the user.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateSmsNumberAsync(string hdid, string sms, CancellationToken ct = default);
    }
}
