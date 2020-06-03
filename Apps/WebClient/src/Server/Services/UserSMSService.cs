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
namespace HealthGateway.WebClient.Services
{
    using System;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class UserSMSService : IUserSMSService
    {
        private readonly ILogger logger;
        private readonly IProfileDelegate profileDelegate;
        private readonly INotificationSettingsService notificationSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSMSService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        public UserSMSService(
            ILogger<UserEmailService> logger,
            IProfileDelegate profileDelegate,
            INotificationSettingsService notificationSettingsService)
        {
            this.logger = logger;
            this.profileDelegate = profileDelegate;
            this.notificationSettingsService = notificationSettingsService;
        }

        /// <inheritdoc />
        public bool UpdateUserSMS(string hdid, string sms, Uri hostUri, string bearerToken)
        {
            this.logger.LogTrace($"Updating user sms numbner...");
            UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
            userProfile.SMSNumber = sms;
            DBResult<UserProfile> updateResult = this.profileDelegate.Update(userProfile);

            if (updateResult.Status == DBStatusCode.Updated)
            {
                // Update the notification settings
                this.UpdateNotificationSettings(userProfile, bearerToken);
                this.logger.LogDebug($"Finished updating user sms number");
                return true;
            }

            return false;
        }

        private async void UpdateNotificationSettings(UserProfile userProfile, string bearerToken)
        {
            // Update the notification settings
            NotificationSettingsRequest request = new NotificationSettingsRequest(userProfile.Email, userProfile.SMSNumber);
            RequestResult<NotificationSettingsResponse> response = await this.notificationSettingsService.SendNotificationSettings(request, bearerToken).ConfigureAwait(true);
            if (response.ResultStatus == ResultType.Error)
            {
                this.notificationSettingsService.QueueNotificationSettings(request, bearerToken);
            }
        }
    }
}
