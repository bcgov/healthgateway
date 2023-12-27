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
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A simple service to queue and send email.
    /// </summary>
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly IBackgroundJobClient jobClient;
        private readonly ILogger<NotificationSettingsService> logger;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="jobClient">The JobScheduler queue client.</param>
        /// <param name="resourceDelegateDelegate">The injected db user delegate delegate.</param>
        public NotificationSettingsService(
            ILogger<NotificationSettingsService> logger,
            IBackgroundJobClient jobClient,
            IResourceDelegateDelegate resourceDelegateDelegate)
        {
            this.logger = logger;
            this.jobClient = jobClient;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
        }

        /// <inheritdoc/>
        public async Task QueueNotificationSettingsAsync(NotificationSettingsRequest notificationSettings, CancellationToken ct = default)
        {
            if (notificationSettings.SmsEnabled && !notificationSettings.SmsVerified && string.IsNullOrEmpty(notificationSettings.SmsVerificationCode))
            {
                throw new InvalidOperationException();
            }

            this.logger.LogTrace("Queueing Notification Settings push to PHSA...");
            string json = JsonSerializer.Serialize(notificationSettings);
            this.jobClient.Enqueue<INotificationSettingsJob>(j => j.PushNotificationSettings(json));

            // Update the notification settings for any dependents
            IEnumerable<ResourceDelegate> resourceDelegates = await this.resourceDelegateDelegate.GetAsync(notificationSettings.SubjectHdid, 0, 500, ct);
            foreach (ResourceDelegate resourceDelegate in resourceDelegates)
            {
                this.logger.LogDebug("Queueing Dependent Notification Settings");
                NotificationSettingsRequest dependentNotificationSettings = new()
                {
                    SubjectHdid = resourceDelegate.ResourceOwnerHdid,
                    EmailAddress = notificationSettings.EmailAddress,
                    EmailEnabled = notificationSettings.EmailEnabled,
                    EmailScope = notificationSettings.EmailScope,
                };

                // Only populate SMS number if it has been verified
                if (notificationSettings.SmsVerified)
                {
                    dependentNotificationSettings.SmsNumber = notificationSettings.SmsNumber;
                    dependentNotificationSettings.SmsEnabled = notificationSettings.SmsEnabled;
                    dependentNotificationSettings.SmsScope = notificationSettings.SmsScope;
                    dependentNotificationSettings.SmsVerified = notificationSettings.SmsVerified;
                }

                string delegateJson = JsonSerializer.Serialize(dependentNotificationSettings);
                this.jobClient.Enqueue<INotificationSettingsJob>(j => j.PushNotificationSettings(delegateJson));
            }

            this.logger.LogDebug("Finished queueing Notification Settings push");
        }
    }
}
