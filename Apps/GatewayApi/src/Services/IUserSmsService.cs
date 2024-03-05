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
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Models;

    /// <summary>
    /// The User SMS service.
    /// </summary>
    public interface IUserSmsService
    {
        /// <summary>
        /// Validates the SMS number that matches the given validation code.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="validationCode">The SMS validation code.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>Returns a request result containing true if the SMS verification was found and validated.</returns>
        Task<RequestResult<bool>> ValidateSmsAsync(string hdid, string validationCode, CancellationToken ct = default);

        /// <summary>
        /// Create the user SMS number.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="sms">SMS number to be set for the user.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>returns true if the sms number was successfully created.</returns>
        Task<MessagingVerification> CreateUserSmsAsync(string hdid, string sms, CancellationToken ct = default);

        /// <summary>
        /// Updates the user SMS number.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="sms">SMS number to be set for the user.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>returns true if the sms number was successfully updated.</returns>
        Task<bool> UpdateUserSmsAsync(string hdid, string sms, CancellationToken ct = default);
    }
}
