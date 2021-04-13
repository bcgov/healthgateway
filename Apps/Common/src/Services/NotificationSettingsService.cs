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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A simple service to queue and send email.
    /// </summary>
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly ILogger<NotificationSettingsService> logger;
        private readonly IBackgroundJobClient jobClient;
        private readonly INotificationSettingsDelegate notificationSettingsDelegate;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="jobClient">The JobScheduler queue client.</param>
        /// <param name="notificationSettingsDelegate">Notification Settings delegate to be used.</param>
        /// <param name="resourceDelegateDelegate">The injected db user delegate delegate.</param>
        public NotificationSettingsService(
            ILogger<NotificationSettingsService> logger,
            IBackgroundJobClient jobClient,
            INotificationSettingsDelegate notificationSettingsDelegate,
            IResourceDelegateDelegate resourceDelegateDelegate)
        {
            this.logger = logger;
            this.jobClient = jobClient;
            this.notificationSettingsDelegate = notificationSettingsDelegate;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
        }

        /// <summary>
        /// Creates a new 6 digit verification code.
        /// </summary>
        /// <returns>The verification code.</returns>
        public static string CreateVerificationCode()
        {
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                generator.GetBytes(data);
                return
                    BitConverter
                        .ToUInt32(data)
                        .ToString("D6", CultureInfo.InvariantCulture)
                        .Substring(0, 6);
            }
        }

        /// <inheritdoc />
        public NotificationSettingsRequest QueueNotificationSettings(NotificationSettingsRequest notificationSettings)
        {
            this.logger.LogTrace($"Queueing Notification Settings push to PHSA...");
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            string json = JsonSerializer.Serialize(ValidateSMSVerificationCode(notificationSettings), options);
            this.jobClient.Enqueue<INotificationSettingsJob>(j => j.PushNotificationSettings(json));

            // Retrieve and update each delegate notification setting
            DBResult<IEnumerable<ResourceDelegate>> dbResult = this.resourceDelegateDelegate.Get(notificationSettings.SubjectHdid, 0, 500);
            foreach (ResourceDelegate resourceDelegate in dbResult.Payload)
            {
                this.logger.LogDebug($"Queueing Dependent Notification Settings.");
                NotificationSettingsRequest dependentNotificationSettings = new ()
                {
                    SubjectHdid = resourceDelegate.ProfileHdid,
                    EmailAddress = notificationSettings.EmailAddress,
                    EmailEnabled = notificationSettings.EmailEnabled,
                    EmailScope = notificationSettings.EmailScope,
                };

                // Only send dependents sms number if it has been verified
                if (notificationSettings.SMSVerified)
                {
                    dependentNotificationSettings.SMSNumber = notificationSettings.SMSNumber;
                    dependentNotificationSettings.SMSEnabled = notificationSettings.SMSEnabled;
                    dependentNotificationSettings.SMSScope = notificationSettings.SMSScope;
                    dependentNotificationSettings.SMSVerified = notificationSettings.SMSVerified;
                }

                string delegateJson = JsonSerializer.Serialize(dependentNotificationSettings, options);
                this.jobClient.Enqueue<INotificationSettingsJob>(j => j.PushNotificationSettings(delegateJson));
            }

            this.logger.LogDebug($"Finished queueing Notification Settings push.");
            return notificationSettings;
        }

        /// <inheritdoc />
        public async Task<RequestResult<NotificationSettingsResponse>> SendNotificationSettings(NotificationSettingsRequest notificationSettings, string bearerToken)
        {
            this.logger.LogTrace($"Queueing Notification Settings push to PHSA...");
            RequestResult<NotificationSettingsResponse> retVal = await this.notificationSettingsDelegate.
                            SetNotificationSettings(ValidateSMSVerificationCode(notificationSettings), bearerToken).ConfigureAwait(true);
            this.logger.LogDebug($"Finished queueing Notification Settings push.");
            return retVal;
        }

        [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Team decision")]
        private static NotificationSettingsRequest ValidateSMSVerificationCode(NotificationSettingsRequest notificationSettings)
        {
            if (notificationSettings.SMSEnabled && string.IsNullOrEmpty(notificationSettings.SMSVerificationCode))
            {
                // Create the SMS validation code if the SMS is not verified and the caller didn't set it.
                notificationSettings.SMSVerificationCode = CreateVerificationCode();
            }

            return notificationSettings;
        }
    }
}
