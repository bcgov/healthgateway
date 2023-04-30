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
namespace HealthGateway.JobScheduler.Api
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.JobScheduler.Models.Notify;
    using Refit;

    /// <summary>
    /// Provides Email and SMS using GC Notify Services.
    /// </summary>
    public interface INotifyApi
    {
        /// <summary>
        /// Sends an SMS using the GC Notify service.
        /// </summary>
        /// <param name="smsRequest">The SMS request to send.</param>
        /// <returns>The SMS Response.</returns>
        [Post("/v2/notifications/sms")]
        Task<SmsResponse> SendSms(SmsRequest smsRequest);

        /// <summary>
        /// Sends an Email using the GC Notify service.
        /// </summary>
        /// <param name="emailRequest">The Email request to send.</param>
        /// <returns>The Email Response.</returns>
        [Post("/v2/notifications/email")]
        Task<EmailResponse> SendEmail(EmailRequest emailRequest);

        /// <summary>
        /// Gets the status of a notification.
        /// </summary>
        /// <param name="notificationId">The id of the notification to query.</param>
        /// <returns>The status of the notification.</returns>
        [Get("/v2/notifications/{notificationId}")]
        Task<NotificationStatus> GetStatus(Guid notificationId);
    }
}
