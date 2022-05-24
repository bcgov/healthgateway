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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

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
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        /// <param name="configuration">Configuration settings.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        public UserEmailService(
            ILogger<UserEmailService> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IUserProfileDelegate profileDelegate,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
            this.emailVerificationExpirySeconds = configuration.GetSection(this.webClientConfigSection).GetValue<int>(this.emailConfigExpirySecondsKey, 5);

            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public PrimitiveRequestResult<bool> ValidateEmail(string hdid, Guid inviteKey)
        {
            this.logger.LogTrace($"Validating email... {inviteKey}");
            MessagingVerification? matchingVerification = this.messageVerificationDelegate.GetLastByInviteKey(inviteKey);
            if (matchingVerification == null ||
                matchingVerification.UserProfileId != hdid ||
                matchingVerification.Deleted)
            {
                // Invalid Verification Attempt
                MessagingVerification? lastEmailVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Email);
                if (lastEmailVerification != null &&
                    !lastEmailVerification.Validated)
                {
                    lastEmailVerification.VerificationAttempts++;
                    this.messageVerificationDelegate.Update(lastEmailVerification);
                }

                this.logger.LogDebug($"Invalid email verification");

                return new PrimitiveRequestResult<bool>()
                {
                    ResourcePayload = false,
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Invalid Email Verification",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }
            else if (matchingVerification.VerificationAttempts >= this.maxVerificationAttempts ||
                     matchingVerification.ExpireDate < DateTime.UtcNow)
            {
                this.logger.LogDebug($"Email verification expired");

                // Verification Expired
                return new PrimitiveRequestResult<bool>()
                {
                    ResourcePayload = false,
                    ResultStatus = ResultType.ActionRequired,
                    ResultError = ErrorTranslator.ActionRequired("Email Verification Expired", ActionType.Expired),
                };
            }
            else if (matchingVerification.Validated)
            {
                this.logger.LogDebug($"Email already validated");

                // Verification already verified
                return new PrimitiveRequestResult<bool>()
                {
                    ResourcePayload = true,
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Email Verification Already verified",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }
            else
            {
                matchingVerification.Validated = true;
                this.messageVerificationDelegate.Update(matchingVerification);
                UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
                userProfile.Email = matchingVerification.Email!.To; // Gets the user email from the email sent.
                this.profileDelegate.Update(userProfile);

                // Update the notification settings
                this.notificationSettingsService.QueueNotificationSettings(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SMSNumber));

                this.logger.LogDebug($"Email validated");

                // Verification already verified
                return new PrimitiveRequestResult<bool>()
                {
                    ResourcePayload = true,
                    ResultStatus = ResultType.Success,
                };
            }
        }

        /// <inheritdoc />
        public bool CreateUserEmail(string hdid, string emailAddress, bool isVerified)
        {
            this.logger.LogTrace($"Creating user email...");
            this.AddVerificationEmail(hdid, emailAddress, Guid.NewGuid(), isVerified);
            this.logger.LogDebug($"Finished creating user email");
            return true;
        }

        /// <inheritdoc />
        public bool UpdateUserEmail(string hdid, string emailAddress)
        {
            this.logger.LogTrace($"Updating user email...");

            UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
            this.logger.LogInformation($"Removing email from user ${hdid}");
            this.profileDelegate.Update(userProfile);
            userProfile.Email = null;

            // Update the notification settings
            this.notificationSettingsService.QueueNotificationSettings(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SMSNumber));

            MessagingVerification? lastEmailVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Email);
            if (lastEmailVerification != null)
            {
                this.logger.LogInformation($"Expiring old email validation for user ${hdid}");
                bool isDeleted = string.IsNullOrEmpty(emailAddress);
                this.messageVerificationDelegate.Expire(lastEmailVerification, isDeleted);
                if (!isDeleted)
                {
                    this.logger.LogInformation($"Sending new email verification for user ${hdid}");

                    if (lastEmailVerification.Email != null
                        && !string.IsNullOrEmpty(lastEmailVerification.Email.To)
                        && emailAddress.Equals(lastEmailVerification.Email.To, StringComparison.OrdinalIgnoreCase))
                    {
                        this.AddVerificationEmail(hdid, emailAddress, lastEmailVerification.InviteKey);
                    }
                    else
                    {
                        this.AddVerificationEmail(hdid, emailAddress, Guid.NewGuid());
                    }
                }
            }
            else
            {
                this.AddVerificationEmail(hdid, emailAddress, Guid.NewGuid());
            }

            this.logger.LogDebug($"Finished updating user email");
            return true;
        }

        private void AddVerificationEmail(string hdid, string toEmail, Guid inviteKey, bool isVerified = false)
        {
            float verificationExpiryHours = (float)this.emailVerificationExpirySeconds / 3600;

            string activationHost = this.httpContextAccessor.HttpContext!.Request
                                             .GetTypedHeaders()
                                             .Referer!
                                             .GetLeftPart(UriPartial.Authority);
            string hostUrl = activationHost.ToString();

            Dictionary<string, string> keyValues = new()
            {
                [EmailTemplateVariable.InviteKey] = inviteKey.ToString(),
                [EmailTemplateVariable.ActivationHost] = hostUrl,
                [EmailTemplateVariable.ExpiryHours] = verificationExpiryHours.ToString("0", CultureInfo.CurrentCulture),
            };

            MessagingVerification messageVerification = new()
            {
                InviteKey = inviteKey,
                UserProfileId = hdid,
                ExpireDate = DateTime.UtcNow.AddSeconds(this.emailVerificationExpirySeconds),
                Email = this.emailQueueService.ProcessTemplate(toEmail, EmailTemplateName.RegistrationTemplate, keyValues),
            };

            if (isVerified)
            {
                messageVerification.Email.EmailStatusCode = EmailStatus.Processed;
                this.messageVerificationDelegate.Insert(messageVerification);
                this.ValidateEmail(hdid, inviteKey);
            }
            else
            {
                this.messageVerificationDelegate.Insert(messageVerification);
                this.emailQueueService.QueueNewEmail(messageVerification.Email);
            }
        }
    }
}
