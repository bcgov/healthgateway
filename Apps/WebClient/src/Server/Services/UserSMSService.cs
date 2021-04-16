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
    using System.Globalization;
    using System.Text.RegularExpressions;
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
        private readonly Regex validSMSRegex;
        private readonly ILogger logger;
        private readonly IUserProfileDelegate profileDelegate;
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
            IUserProfileDelegate profileDelegate,
            INotificationSettingsService notificationSettingsService)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.notificationSettingsService = notificationSettingsService;

            this.validSMSRegex = new Regex("[^0-9]");
        }

        /// <inheritdoc />
        public bool ValidateSMS(string hdid, string validationCode)
        {
            this.logger.LogTrace($"Validating sms... {validationCode}");
            bool isValid = false;
            MessagingVerification? smsVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.SMS);

            if (smsVerification != null &&
                smsVerification.HdId == hdid &&
                !smsVerification.Validated &&
                !smsVerification.Deleted &&
                smsVerification.VerificationAttempts < MaxVerificationAttempts &&
                smsVerification.SMSValidationCode == validationCode &&
                smsVerification.ExpireDate >= DateTime.UtcNow)
            {
                smsVerification.Validated = true;
                this.messageVerificationDelegate.Update(smsVerification);
                UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
                userProfile.SMSNumber = smsVerification.SMSNumber; // Gets the user sms number from the message sent.
                this.profileDelegate.Update(userProfile);
                isValid = true;

                // Update the notification settings
                this.notificationSettingsService.QueueNotificationSettings(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SMSNumber));
            }
            else
            {
                smsVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.SMS);
                if (smsVerification != null &&
                    !smsVerification.Validated)
                {
                    smsVerification.VerificationAttempts++;
                    this.messageVerificationDelegate.Update(smsVerification);
                }
            }

            this.logger.LogDebug($"Finished validating sms: {JsonConvert.SerializeObject(isValid)}");
            return isValid;
        }

        /// <inheritdoc />
        public bool CreateUserSMS(string hdid, string sms)
        {
            this.logger.LogInformation($"Adding new sms verification for user ${hdid}");
            sms = this.SanitizeSMS(sms);
            string verificationCode = NotificationSettingsService.CreateVerificationCode();
            this.AddVerificationSMS(hdid, sms, verificationCode);
            this.logger.LogDebug($"Finished updating user sms");
            return true;
        }

        /// <inheritdoc />
        public bool UpdateUserSMS(string hdid, string sms)
        {
            this.logger.LogTrace($"Removing user sms number ${hdid}");
            sms = this.SanitizeSMS(sms);
            UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
            userProfile.SMSNumber = null;
            this.profileDelegate.Update(userProfile);

            // Update the notification settings
            NotificationSettingsRequest queuedNotification = this.notificationSettingsService.QueueNotificationSettings(new NotificationSettingsRequest(userProfile, userProfile.Email, sms));

            bool isDeleted = string.IsNullOrEmpty(sms);
            MessagingVerification? lastSMSVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.SMS);
            if (lastSMSVerification != null)
            {
                this.logger.LogInformation($"Expiring old sms validation for user ${hdid}");
                this.messageVerificationDelegate.Expire(lastSMSVerification, isDeleted);
            }

            if (!isDeleted)
            {
                this.logger.LogInformation($"Sending new sms verification for user ${hdid}");
                this.AddVerificationSMS(hdid, sms, queuedNotification.SMSVerificationCode);
            }

            this.logger.LogDebug($"Finished updating user sms");
            return true;
        }

        private string SanitizeSMS(string smsNumber)
        {
            return this.validSMSRegex.Replace(smsNumber, string.Empty);
        }

        private void AddVerificationSMS(string hdid, string sms, string smsVerificationCode)
        {
            this.logger.LogInformation($"Sending new sms verification for user ${hdid}");
            MessagingVerification messagingVerification = new ()
            {
                HdId = hdid,
                SMSNumber = sms,
                SMSValidationCode = smsVerificationCode,
                VerificationType = MessagingVerificationType.SMS,
                ExpireDate = DateTime.UtcNow.AddDays(VerificationExpiryDays),
            };

            this.messageVerificationDelegate.Insert(messagingVerification);
        }
    }
}
