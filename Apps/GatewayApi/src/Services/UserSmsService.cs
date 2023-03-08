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
    using System;
    using System.Globalization;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class UserSmsService : IUserSmsService
    {
        /// <summary>
        /// The maximum verification attempts.
        /// </summary>
        public const int MaxVerificationAttempts = 5;
        private const int VerificationExpiryDays = 5;
        private readonly ILogger logger;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly Regex validSmsRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSmsService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        public UserSmsService(
            ILogger<UserSmsService> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IUserProfileDelegate profileDelegate,
            INotificationSettingsService notificationSettingsService)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.notificationSettingsService = notificationSettingsService;

            this.validSmsRegex = new Regex("[^0-9]");
        }

        /// <inheritdoc/>
        public RequestResult<bool> ValidateSms(string hdid, string validationCode)
        {
            this.logger.LogTrace("Validating sms... {ValidationCode}", validationCode);

            RequestResult<bool> retVal = new() { ResourcePayload = false, ResultStatus = ResultType.Success };
            MessagingVerification? smsVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Sms);

            if (smsVerification != null &&
                smsVerification.UserProfileId == hdid &&
                !smsVerification.Validated &&
                !smsVerification.Deleted &&
                smsVerification.VerificationAttempts < MaxVerificationAttempts &&
                smsVerification.SmsValidationCode == validationCode &&
                smsVerification.ExpireDate >= DateTime.UtcNow)
            {
                smsVerification.Validated = true;
                this.messageVerificationDelegate.Update(smsVerification);
                UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
                userProfile.SmsNumber = smsVerification.SmsNumber; // Gets the user sms number from the message sent.
                this.profileDelegate.Update(userProfile);
                retVal.ResourcePayload = true;

                // Update the notification settings
                this.notificationSettingsService.QueueNotificationSettings(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SmsNumber));
            }
            else
            {
                smsVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Sms);
                if (smsVerification != null &&
                    !smsVerification.Validated)
                {
                    smsVerification.VerificationAttempts++;
                    this.messageVerificationDelegate.Update(smsVerification);
                }
            }

            this.logger.LogDebug("Finished validating sms");
            return retVal;
        }

        /// <inheritdoc/>
        public MessagingVerification CreateUserSms(string hdid, string sms)
        {
            this.logger.LogInformation("Adding new sms verification for user {Hdid}", hdid);
            string sanitizedSms = this.SanitizeSms(sms);
            MessagingVerification messagingVerification = this.AddVerificationSms(hdid, sanitizedSms);
            this.logger.LogDebug("Finished updating user sms");
            return messagingVerification;
        }

        /// <inheritdoc/>
        public bool UpdateUserSms(string hdid, string sms)
        {
            this.logger.LogTrace("Removing user sms number {Hdid}", hdid);
            string sanitizedSms = this.SanitizeSms(sms);
            if (!UserProfileValidator.ValidateUserProfileSmsNumber(sanitizedSms))
            {
                this.logger.LogWarning("Proposed sms number is not valid {SmsNumber}", sanitizedSms);
                throw new ProblemDetailsException(
                    ExceptionUtility.CreateProblemDetails(
                        $"Proposed sms number is not valid {sanitizedSms}",
                        HttpStatusCode.BadRequest,
                        nameof(UserSmsService)));
            }

            UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
            userProfile.SmsNumber = null;
            this.profileDelegate.Update(userProfile);

            bool isDeleted = string.IsNullOrEmpty(sanitizedSms);
            MessagingVerification? lastSmsVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Sms);
            if (lastSmsVerification != null)
            {
                this.logger.LogInformation("Expiring old sms validation for user {Hdid}", hdid);
                this.messageVerificationDelegate.Expire(lastSmsVerification, isDeleted);
            }

            NotificationSettingsRequest notificationRequest = new(userProfile, userProfile.Email, sanitizedSms);
            if (!isDeleted)
            {
                this.logger.LogInformation("Adding new sms verification for user {Hdid}", hdid);
                MessagingVerification messagingVerification = this.AddVerificationSms(hdid, sanitizedSms);
                notificationRequest.SmsVerificationCode = messagingVerification.SmsValidationCode;
            }

            // Update the notification settings
            this.notificationSettingsService.QueueNotificationSettings(notificationRequest);

            this.logger.LogDebug("Finished updating user sms");
            return true;
        }

        /// <summary>
        /// Creates a new 6 digit verification code.
        /// </summary>
        /// <returns>The verification code.</returns>
        private static string CreateVerificationCode()
        {
            using RandomNumberGenerator generator = RandomNumberGenerator.Create();
            byte[] data = new byte[4];
            generator.GetBytes(data);
            return
                BitConverter
                    .ToUInt32(data)
                    .ToString("D6", CultureInfo.InvariantCulture)
                    .Substring(0, 6);
        }

        private string SanitizeSms(string smsNumber)
        {
            return this.validSmsRegex.Replace(smsNumber, string.Empty);
        }

        private MessagingVerification AddVerificationSms(string hdid, string sms)
        {
            this.logger.LogInformation("Sending new sms verification for user {Hdid}", hdid);
            MessagingVerification messagingVerification = new()
            {
                UserProfileId = hdid,
                SmsNumber = sms,
                SmsValidationCode = CreateVerificationCode(),
                VerificationType = MessagingVerificationType.Sms,
                ExpireDate = DateTime.UtcNow.AddDays(VerificationExpiryDays),
            };

            this.messageVerificationDelegate.Insert(messagingVerification);
            return messagingVerification;
        }
    }
}
