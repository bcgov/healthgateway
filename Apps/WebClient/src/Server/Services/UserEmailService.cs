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
    using System.Collections.Generic;
    using System.Globalization;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class UserEmailService : IUserEmailService
    {
        private readonly string webClientConfigSection = "WebClient";
        private readonly string emailConfigExpirySecondsKey = "EmailVerificationExpirySeconds";
        private readonly int emailVerificationExpirySeconds;
        private readonly int maxVerificationAttempts = 5;
        private readonly ILogger logger;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IEmailQueueService emailQueueService;
        private readonly INotificationSettingsService notificationSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        /// <param name="configuration">Configuration settings.</param>
        public UserEmailService(
            ILogger<UserEmailService> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IUserProfileDelegate profileDelegate,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
            this.emailVerificationExpirySeconds = configuration.GetSection(this.webClientConfigSection).GetValue<int>(this.emailConfigExpirySecondsKey, 5);
        }

        /// <inheritdoc />
        public PrimitiveRequestResult<bool> ValidateEmail(string hdid, Guid inviteKey)
        {
            this.logger.LogTrace($"Validating email... {inviteKey}");
            PrimitiveRequestResult<bool> retVal = new();

            MessagingVerification? emailInvite = this.messageVerificationDelegate.GetLastByInviteKey(inviteKey);
            if (emailInvite == null ||
                emailInvite.HdId != hdid ||
                emailInvite.Validated == true ||
                emailInvite.Deleted == true)
            {
                // Invalid Verification Attempt
                emailInvite = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Email);
                if (emailInvite != null &&
                    !emailInvite.Validated)
                {
                    emailInvite.VerificationAttempts++;
                    this.messageVerificationDelegate.Update(emailInvite);
                }

                return new PrimitiveRequestResult<bool>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Invalid Email Invite",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }
            else if (emailInvite.VerificationAttempts >= this.maxVerificationAttempts ||
                     emailInvite.ExpireDate < DateTime.UtcNow)
            {
                // Verification Expired
                retVal.ResultStatus = ResultType.ActionRequired;
                retVal.ResultError = ErrorTranslator.ActionRequired("Email Invite Expired", ActionType.Expired);
            }
            else
            {
                emailInvite.Validated = true;
                this.messageVerificationDelegate.Update(emailInvite);
                UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
                userProfile.Email = emailInvite.Email!.To; // Gets the user email from the email sent.
                this.profileDelegate.Update(userProfile);

                // Update the notification settings
                this.UpdateNotificationSettings(userProfile);

                retVal.ResultStatus = ResultType.Success;
            }

            this.logger.LogDebug($"Finished validating email: {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public bool UpdateUserEmail(string hdid, string emailAddress, Uri hostUri)
        {
            this.logger.LogTrace($"Updating user email...");
            UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
            this.logger.LogInformation($"Removing email from user ${hdid}");
            userProfile.Email = null;
            this.profileDelegate.Update(userProfile);

            MessagingVerification? lastEmailVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Email);
            if (lastEmailVerification != null)
            {
                this.logger.LogInformation($"Expiring old email validation for user ${hdid}");
                bool isEmailRemoved = string.IsNullOrEmpty(emailAddress);
                this.messageVerificationDelegate.Expire(lastEmailVerification, isEmailRemoved);
                if (!isEmailRemoved)
                {
                    this.logger.LogInformation($"Sending new email invite for user ${hdid}");

                    if (lastEmailVerification.Email != null
                        && !string.IsNullOrEmpty(lastEmailVerification.Email.To)
                        && emailAddress.Equals(lastEmailVerification.Email.To, StringComparison.OrdinalIgnoreCase))
                    {
                        this.QueueVerificationEmail(hdid, emailAddress, hostUri, lastEmailVerification.InviteKey);
                    }
                    else
                    {
                        this.QueueVerificationEmail(hdid, emailAddress, hostUri, Guid.NewGuid());
                    }
                }
            }
            else
            {
                this.QueueVerificationEmail(hdid, emailAddress, hostUri, Guid.NewGuid());
            }

            // Update the notification settings
            this.UpdateNotificationSettings(userProfile);

            this.logger.LogDebug($"Finished updating user email");
            return true;
        }

        private void UpdateNotificationSettings(UserProfile userProfile)
        {
            // Update the notification settings
            NotificationSettingsRequest request = new(userProfile, userProfile.Email, userProfile.SMSNumber);
            this.notificationSettingsService.QueueNotificationSettings(request);
        }

        private void QueueVerificationEmail(string hdid, string toEmail, Uri activationHost, Guid inviteKey)
        {
            float verificationExpiryHours = this.emailVerificationExpirySeconds / 3600;
            string hostUrl = activationHost.ToString();
            hostUrl = hostUrl.Remove(hostUrl.Length - 1, 1); // Strips last slash

            Dictionary<string, string> keyValues = new()
            {
                [EmailTemplateVariable.INVITE_KEY_VARIABLE] = inviteKey.ToString(),
                [EmailTemplateVariable.ACTIVATION_HOST_VARIABLE] = hostUrl,
                [EmailTemplateVariable.EMAIL_TEMPLATE_EXPIRY_HOURS] = verificationExpiryHours.ToString("0", CultureInfo.CurrentCulture),
            };

            MessagingVerification messageVerification = new()
            {
                InviteKey = inviteKey,
                HdId = hdid,
                ExpireDate = DateTime.UtcNow.AddSeconds(this.emailVerificationExpirySeconds),
                Email = this.emailQueueService.ProcessTemplate(toEmail, EmailTemplateName.RegistrationTemplate, keyValues),
            };

            this.messageVerificationDelegate.Insert(messageVerification);

            this.emailQueueService.QueueNewEmail(messageVerification.Email);
        }
    }
}
