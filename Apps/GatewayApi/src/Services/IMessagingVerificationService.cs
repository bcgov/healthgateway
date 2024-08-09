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
    using HealthGateway.Database.Models;

    /// <summary>
    /// The Messaging Verification service.
    /// </summary>
    public interface IMessagingVerificationService
    {
        /// <summary>
        /// Creates a messaging verification using the supplied hdid and email.
        /// </summary>
        /// <param name="hdid">The hdid associated with the email.</param>
        /// <param name="email">The email for the messaging verification./></param>
        /// <param name="isEmailVerified">The value determines whether email has been verified or not./></param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A messaging verification model.</returns>
        Task<MessagingVerification> AddEmailVerificationAsync(string hdid, string email, bool isEmailVerified, bool shouldCommit = true, CancellationToken ct = default);

        /// <summary>
        /// Creates a messaging verification using the supplied hdid and sms number.
        /// </summary>
        /// <param name="hdid">The hdid associated with the sms number.</param>
        /// <param name="smsNumber">The sms number for the messaging verification./></param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A messaging verificaiton model.</returns>
        Task<MessagingVerification> AddSmsVerificationAsync(string hdid, string smsNumber, bool shouldCommit = true, CancellationToken ct = default);
    }
}
