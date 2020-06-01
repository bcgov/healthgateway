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
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class UserPhoneService : IUserPhoneService
    {
        private readonly ILogger logger;
        private readonly IProfileDelegate profileDelegate;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPhoneService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        public UserPhoneService(ILogger<UserPhoneService> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IProfileDelegate profileDelegate,
            INotificationSettingsService notificationSettingsService)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.notificationSettingsService = notificationSettingsService;
        }

        /// <inheritdoc />
        public bool ValidateSMS(string hdid, string validationCode, string bearerToken)
        {
            this.logger.LogTrace($"Validating phone... {validationCode}");
            bool retVal = false;
            MessagingVerification phoneInvite = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.SMS);

            if (phoneInvite != null &&
                phoneInvite.HdId == hdid &&
                !phoneInvite.Validated &&
                phoneInvite.SMSValidationCode == validationCode &&
                phoneInvite.ExpireDate >= DateTime.UtcNow)
            {
                phoneInvite.Validated = true;
                this.messageVerificationDelegate.Update(phoneInvite);
                UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
                userProfile.PhoneNumber = phoneInvite.SMSNumber; // Gets the user sms number from the message sent.
                this.profileDelegate.Update(userProfile);
                retVal = true;

                // Update the notification settings
                this.UpdateNotificationSettings(userProfile.Email, userProfile.PhoneNumber, bearerToken);
            }

            this.logger.LogDebug($"Finished validating sms: {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public bool UpdateUserPhone(string hdid, string phone, Uri hostUri, string bearerToken)
        {
            this.logger.LogTrace($"Removing user sms number ${hdid}");
            UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
            userProfile.PhoneNumber = null;
            this.profileDelegate.Update(userProfile);
            MessagingVerification smsInvite = this.RetrieveLastInvite(hdid);

            // Update the notification settings
            this.logger.LogInformation($"Sending new sms invite for user ${hdid}");
            this.UpdateNotificationSettings(userProfile.Email, phone, bearerToken);

            if (smsInvite != null && !smsInvite.Validated && smsInvite.ExpireDate >= DateTime.UtcNow)
            {
                this.logger.LogInformation($"Expiring old sms validation for user ${hdid}");
                smsInvite.ExpireDate = DateTime.UtcNow;
                this.messageVerificationDelegate.Update(smsInvite);
            }

            this.logger.LogDebug($"Finished updating user sms");
            return true;
        }

        private async void UpdateNotificationSettings(string? email, string? smsNumber, string bearerToken)
        {
            // Update the notification settings
            NotificationSettingsRequest request = new NotificationSettingsRequest(email, smsNumber);
            RequestResult<NotificationSettingsResponse> response = await this.notificationSettingsService.SendNotificationSettings(request, bearerToken).ConfigureAwait(true);
            if (response.ResultStatus == ResultType.Error)
            {
                this.notificationSettingsService.QueueNotificationSettings(request, bearerToken);
            }
        }

        /// <inheritdoc />
        public MessagingVerification RetrieveLastInvite(string hdid)
        {
            MessagingVerification phoneInvite = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.SMS);
            return phoneInvite;
        }
    }
}
