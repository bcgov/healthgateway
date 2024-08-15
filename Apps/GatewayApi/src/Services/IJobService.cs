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
    /// The Job service.
    /// </summary>
    public interface IJobService
    {
        /// <summary>
        /// Creates an event to notify that the account was created.
        /// </summary>
        /// <param name="hdid">The hdid associated with the account.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task NotifyAccountCreationAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Notifies email verification was successful.
        /// </summary>
        /// <param name="hdid">The hdid associated with the email.</param>
        /// <param name="email">The email associated with the verification.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task NotifyEmailVerificationAsync(string hdid, string email, CancellationToken ct = default);

        /// <summary>
        /// Queues email to be sent.
        /// </summary>
        /// <param name="email">The email to queue.</param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendEmailAsync(Email email, bool shouldCommit = true, CancellationToken ct = default);

        /// <summary>
        /// Queues email to be sent.
        /// </summary>
        /// <param name="emailAddress">The to email address.</param>
        /// <param name="emailTemplateName">The email template to use.</param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendEmailAsync(string emailAddress, string emailTemplateName, bool shouldCommit = true, CancellationToken ct = default);

        /// <summary>
        /// Queues job to push notification settings to PHSA.
        /// </summary>
        /// <param name="userProfile">The user profile for the notification settings.</param>
        /// <param name="email">Email address for the notification settings.</param>
        /// <param name="smsNumber">SMS Number for the notification settings.</param>
        /// <param name="smsVerificationCode">The code used to validate the ownership of the number.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task PushNotificationSettingsToPhsaAsync(UserProfile userProfile, string? email, string? smsNumber, string? smsVerificationCode = null, CancellationToken ct = default);
    }
}
