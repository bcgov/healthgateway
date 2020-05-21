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
namespace HealthGateway.Common.Services
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A simple service to queue and send email.
    /// </summary>
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly INotificationSettingsDelegate notificationSettingsDelegate;
        private readonly ILogger<NotificationSettingsService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="notificationSettingsDelegate">Notification Settings delegate to be used.</param>
        public NotificationSettingsService(
            ILogger<NotificationSettingsService> logger,
            INotificationSettingsDelegate notificationSettingsDelegate)
        {
            this.logger = logger;
            this.notificationSettingsDelegate = notificationSettingsDelegate;
        }

        /// <inheritdoc />
        public void QueueNotificationSettings(NotificationSettingsRequest notificationSettings, string bearerToken)
        {
            this.logger.LogTrace($"Queueing Notification Settings push to PHSA...");
            BackgroundJob.Enqueue<INotificationSettingsJob>(j => j.PushNotificationSettings(this.ValidateVerificationCode(notificationSettings), bearerToken));
            this.logger.LogDebug($"Finished queueing Notification Settings push.");
        }

        /// <inheritdoc />
        public async Task<RequestResult<NotificationSettingsResponse>> SendNotificationSettings(NotificationSettingsRequest notificationSettings, string bearerToken)
        {
            this.logger.LogTrace($"Queueing Notification Settings push to PHSA...");
            RequestResult<NotificationSettingsResponse> retVal = await this.notificationSettingsDelegate.
                            SetNotificationSettings(this.ValidateVerificationCode(notificationSettings), bearerToken).ConfigureAwait(true);
            this.logger.LogDebug($"Finished queueing Notification Settings push.");
            return retVal;
        }

        private NotificationSettingsRequest ValidateVerificationCode(NotificationSettingsRequest notificationSettings)
        {
            if (!notificationSettings.SMSVerified && string.IsNullOrEmpty(notificationSettings.SMSVerificationCode))
            {
                // Create the SMS validation code if the SMS is not verified and the caller didn't set it.
                Random generator = new Random();
                notificationSettings.SMSVerificationCode = generator.Next(0, 999999).ToString("D6", CultureInfo.InvariantCulture);
            }

            return notificationSettings;
        }
    }
}
