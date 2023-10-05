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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class UserEmailService : IUserEmailService
    {
        private readonly string emailConfigExpirySecondsKey = "EmailVerificationExpirySeconds";
        private readonly IEmailQueueService emailQueueService;
        private readonly int emailVerificationExpirySeconds;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;
        private readonly int maxVerificationAttempts = 5;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly string webClientConfigSection = "WebClient";
        private readonly bool changeFeedEnabled;
        private readonly IMessageSender messageSender;

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
        /// <param name="messageSender">The message sender.</param>
        public UserEmailService(
            ILogger<UserEmailService> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IUserProfileDelegate profileDelegate,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMessageSender messageSender)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
            this.emailVerificationExpirySeconds = configuration.GetSection(this.webClientConfigSection).GetValue(this.emailConfigExpirySecondsKey, 5);

            this.httpContextAccessor = httpContextAccessor;
            this.messageSender = messageSender;
            this.changeFeedEnabled = configuration.GetSection(ChangeFeedConfiguration.ConfigurationSectionKey)
                .GetValue($"{ChangeFeedConfiguration.NotificationChannelVerifiedKey}:Enabled", false);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<bool>> ValidateEmailAsync(string hdid, Guid inviteKey, CancellationToken ct = default)
        {
            this.logger.LogTrace("Validating email... {InviteKey}", inviteKey);
            MessagingVerification? matchingVerification = this.messageVerificationDelegate.GetLastByInviteKey(inviteKey);
            UserProfile? userProfile = await this.profileDelegate.GetUserProfileAsync(hdid);
            if (userProfile == null ||
                matchingVerification == null ||
                matchingVerification.UserProfileId != hdid ||
                matchingVerification.Deleted)
            {
                // Invalid Verification Attempt
                MessagingVerification? lastEmailVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Email);
                if (lastEmailVerification is { Validated: false })
                {
                    lastEmailVerification.VerificationAttempts++;
                    await this.messageVerificationDelegate.UpdateAsync(lastEmailVerification, ct: ct);
                }

                this.logger.LogDebug("Invalid email verification");

                return new RequestResult<bool>
                {
                    ResourcePayload = false,
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Invalid Email Verification",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }

            if (matchingVerification.VerificationAttempts >= this.maxVerificationAttempts ||
                matchingVerification.ExpireDate < DateTime.UtcNow)
            {
                this.logger.LogDebug("Email verification expired");

                // Verification Expired
                return new RequestResult<bool>
                {
                    ResourcePayload = false,
                    ResultStatus = ResultType.ActionRequired,
                    ResultError = ErrorTranslator.ActionRequired("Email Verification Expired", ActionType.Expired),
                };
            }

            if (matchingVerification.Validated)
            {
                this.logger.LogDebug("Email already validated");

                // Verification already verified
                return new RequestResult<bool>
                {
                    ResourcePayload = true,
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Email Verification Already verified",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }

            matchingVerification.Validated = true;
            await this.messageVerificationDelegate.UpdateAsync(matchingVerification, !this.changeFeedEnabled, ct);
            userProfile.Email = matchingVerification.Email!.To; // Gets the user email from the email sent.
            this.profileDelegate.Update(userProfile, !this.changeFeedEnabled);

            if (this.changeFeedEnabled)
            {
                await this.messageSender.SendAsync(new[] { new MessageEnvelope(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Email, matchingVerification.Email!.To)) }, ct);
            }

            // UpdateAsync the notification settings
            this.notificationSettingsService.QueueNotificationSettings(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SmsNumber));

            this.logger.LogDebug("Email validated");

            // Verification verified
            return new RequestResult<bool>
            {
                ResourcePayload = true,
                ResultStatus = ResultType.Success,
            };
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public async Task<bool> CreateUserEmailAsync(string hdid, string emailAddress, bool isVerified, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Creating user email...");
            await this.AddVerificationEmail(hdid, emailAddress, Guid.NewGuid(), isVerified, commit);
            this.logger.LogDebug("Finished creating user email");
            return true;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public async Task<bool> UpdateUserEmailAsync(string hdid, string emailAddress, CancellationToken ct = default)
        {
            this.logger.LogTrace("Updating user email...");

            UserProfile? userProfile = await this.profileDelegate.GetUserProfileAsync(hdid);
            if (userProfile == null)
            {
                throw new ProblemDetailsException(
                    ExceptionUtility.CreateProblemDetails(
                        $"User profile not found for hdid {hdid}",
                        HttpStatusCode.BadRequest,
                        nameof(UserSmsService)));
            }

            this.logger.LogInformation("Removing email from user {Hdid}", hdid);
            this.profileDelegate.Update(userProfile);
            userProfile.Email = null;

            // UpdateAsync the notification settings
            this.notificationSettingsService.QueueNotificationSettings(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SmsNumber));

            MessagingVerification? lastEmailVerification = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Email);
            if (lastEmailVerification != null)
            {
                this.logger.LogInformation("Expiring old email validation for user {Hdid}", hdid);
                bool isDeleted = string.IsNullOrEmpty(emailAddress);
                await this.messageVerificationDelegate.ExpireAsync(lastEmailVerification, isDeleted, ct);
                if (!isDeleted)
                {
                    this.logger.LogInformation("Sending new email verification for user {Hdid}", hdid);

                    if (lastEmailVerification.Email != null
                        && !string.IsNullOrEmpty(lastEmailVerification.Email.To)
                        && emailAddress.Equals(lastEmailVerification.Email.To, StringComparison.OrdinalIgnoreCase))
                    {
                        await this.AddVerificationEmail(hdid, emailAddress, lastEmailVerification.InviteKey!.Value);
                    }
                    else
                    {
                        await this.AddVerificationEmail(hdid, emailAddress, Guid.NewGuid());
                    }
                }
            }
            else
            {
                await this.AddVerificationEmail(hdid, emailAddress, Guid.NewGuid());
            }

            this.logger.LogDebug("Finished updating user email");
            return true;
        }

        [ExcludeFromCodeCoverage]
        private async Task AddVerificationEmail(string hdid, string toEmail, Guid inviteKey, bool isVerified = false, bool commit = true)
        {
            float verificationExpiryHours = (float)this.emailVerificationExpirySeconds / 3600;

            string hostUrl = this.httpContextAccessor.HttpContext!.Request
                .GetTypedHeaders()
                .Referer!
                .GetLeftPart(UriPartial.Authority);

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
                EmailAddress = toEmail,
            };

            if (isVerified)
            {
                messageVerification.Email.EmailStatusCode = EmailStatus.Processed;
                await this.messageVerificationDelegate.InsertAsync(messageVerification, commit);
                await this.ValidateEmailAsync(hdid, inviteKey);
            }
            else
            {
                await this.messageVerificationDelegate.InsertAsync(messageVerification);
                this.emailQueueService.QueueNewEmail(messageVerification.Email);
            }
        }
    }
}
