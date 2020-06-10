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
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class UserSMSService : IUserSMSService
    {
        /// <summary>
        /// The maximum verification attempts.
        /// </summary>
        public const int MaxVerificationAttempts = 5;
        private const int VerificationExpiryDays = 5;
        private readonly ILogger logger;
        private readonly IProfileDelegate profileDelegate;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSMSService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        public UserSMSService(
            ILogger<UserSMSService> logger,
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
            this.logger.LogTrace($"Validating sms... {validationCode}");
            bool retVal = false;
            MessagingVerification? smsInvite = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.SMS);

            if (smsInvite != null &&
                smsInvite.HdId == hdid &&
                !smsInvite.Validated &&
                !smsInvite.Deleted &&
                smsInvite.VerificationAttempts < MaxVerificationAttempts &&
                smsInvite.SMSValidationCode == validationCode &&
                smsInvite.ExpireDate >= DateTime.UtcNow)
            {
                smsInvite.Validated = true;
                this.messageVerificationDelegate.Update(smsInvite);
                UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
                userProfile.SMSNumber = smsInvite.SMSNumber; // Gets the user sms number from the message sent.
                this.profileDelegate.Update(userProfile);
                retVal = true;

                // Update the notification settings
                this.UpdateNotificationSettings(userProfile, userProfile.Email, userProfile.SMSNumber, bearerToken);
            }
            else
            {
                smsInvite = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.SMS);
                if (smsInvite != null &&
                    !smsInvite.Validated)
                {
                    smsInvite.VerificationAttempts++;
                    this.messageVerificationDelegate.Update(smsInvite);
                }
            }

            this.logger.LogDebug($"Finished validating sms: {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public bool UpdateUserSMS(string hdid, string sms, Uri hostUri, string bearerToken)
        {
            this.logger.LogTrace($"Removing user sms number ${hdid}");
            UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
            userProfile.SMSNumber = null;
            this.profileDelegate.Update(userProfile);
            MessagingVerification? smsInvite = this.RetrieveLastInvite(hdid);

            // Update the notification settings
            NotificationSettingsRequest notificationRequest = this.UpdateNotificationSettings(userProfile, userProfile.Email, sms, bearerToken);

            if (smsInvite != null && smsInvite.ExpireDate >= DateTime.UtcNow)
            {
                this.logger.LogInformation($"Expiring old sms validation for user ${hdid}");
                smsInvite.ExpireDate = DateTime.UtcNow;
                smsInvite.Deleted = string.IsNullOrEmpty(sms);
                this.messageVerificationDelegate.Update(smsInvite);
            }

            if (!string.IsNullOrEmpty(sms))
            {
                this.logger.LogInformation($"Sending new sms invite for user ${hdid}");
                MessagingVerification messagingVerification = new MessagingVerification();
                messagingVerification.HdId = hdid;
                messagingVerification.SMSNumber = sms;
                messagingVerification.SMSValidationCode = notificationRequest.SMSVerificationCode;
                messagingVerification.VerificationType = MessagingVerificationType.SMS;
                messagingVerification.ExpireDate = DateTime.UtcNow.AddDays(VerificationExpiryDays);
                this.messageVerificationDelegate.Insert(messagingVerification);
            }

            this.logger.LogDebug($"Finished updating user sms");
            return true;
        }

        /// <inheritdoc />
        public MessagingVerification? RetrieveLastInvite(string hdid)
        {
            MessagingVerification? smsInvite = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.SMS);
            return smsInvite;
        }

        private NotificationSettingsRequest UpdateNotificationSettings(UserProfile userProfile, string? email, string? smsNumber, string bearerToken)
        {
            // Update the notification settings
            NotificationSettingsRequest request = new NotificationSettingsRequest(userProfile, email, smsNumber);
            RequestResult<NotificationSettingsResponse> response = Task.Run(async () => await this.notificationSettingsService.SendNotificationSettings(request, bearerToken).ConfigureAwait(true)).Result;
            if (response.ResultStatus == ResultType.Error)
            {
                this.notificationSettingsService.QueueNotificationSettings(request);
            }

            return request;
        }
    }
}
