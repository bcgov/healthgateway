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
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class UserEmailService : IUserEmailService
    {
        private const int MaxVerificationAttempts = 5;
        private const string EmailConfigExpirySecondsKey = "EmailVerificationExpirySeconds";
        private const string WebClientConfigSection = "WebClient";

        private readonly IEmailQueueService emailQueueService;
        private readonly int emailVerificationExpirySeconds;
        private readonly ILogger<UserEmailService> logger;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly ChangeFeedOptions changeFeedConfiguration;
        private readonly IMessageSender messageSender;
        private readonly EmailTemplateConfig emailTemplateConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        /// <param name="configuration">Configuration settings.</param>
        /// <param name="messageSender">The message sender.</param>
        public UserEmailService(
            ILogger<UserEmailService> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IUserProfileDelegate profileDelegate,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService,
            IConfiguration configuration,
            IMessageSender messageSender)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
            this.emailVerificationExpirySeconds = configuration.GetSection(WebClientConfigSection).GetValue(EmailConfigExpirySecondsKey, 5);

            this.messageSender = messageSender;
            this.changeFeedConfiguration = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>() ?? new();
            this.emailTemplateConfig = configuration.GetSection(EmailTemplateConfig.ConfigurationSectionKey).Get<EmailTemplateConfig>() ?? new();
        }

        /// <inheritdoc/>
        public async Task<RequestResult<bool>> ValidateEmailAsync(string hdid, Guid inviteKey, CancellationToken ct = default)
        {
            MessagingVerification? matchingVerification = await this.messageVerificationDelegate.GetLastByInviteKeyAsync(inviteKey, ct);
            UserProfile? userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct);
            if (userProfile == null ||
                matchingVerification == null ||
                matchingVerification.UserProfileId != hdid ||
                matchingVerification.Deleted)
            {
                // Invalid Verification Attempt
                MessagingVerification? lastEmailVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
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

            if (matchingVerification.VerificationAttempts >= MaxVerificationAttempts ||
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
                this.logger.LogDebug("Email already verified");

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
            await this.messageVerificationDelegate.UpdateAsync(matchingVerification, false, ct);

            userProfile.Email = matchingVerification.Email!.To; // Gets the user email from the email sent.
            DbResult<UserProfile> dbResult = await this.profileDelegate.UpdateAsync(userProfile, true, ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                return RequestResultFactory.ServiceError<bool>(ErrorType.CommunicationInternal, ServiceType.Database, ErrorMessages.CannotPerformAction);
            }

            if (this.changeFeedConfiguration.Notifications.Enabled)
            {
                MessageEnvelope[] events = [new(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Email, matchingVerification.Email!.To), hdid)];
                await this.messageSender.SendAsync(events, ct);
            }

            // Update the notification settings
            await this.notificationSettingsService.QueueNotificationSettingsAsync(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SmsNumber), ct);

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
            await this.AddVerificationEmailAsync(hdid, emailAddress, Guid.NewGuid(), isVerified, ct);
            return true;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("ReSharper", "CognitiveComplexity", Justification = "Team decision")]
        public async Task<bool> UpdateUserEmailAsync(string hdid, string emailAddress, CancellationToken ct = default)
        {
            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, true, ct) ??
                                      throw new NotFoundException($"User profile not found for hdid {hdid}");

            bool result = string.IsNullOrWhiteSpace(emailAddress)
                          || (await new OptionalEmailAddressValidator().ValidateAsync(emailAddress, ct))?.IsValid == true;
            if (!result)
            {
                throw new ValidationException($"Invalid email address: {emailAddress}");
            }

            this.logger.LogDebug("Clearing user's email address");
            userProfile.Email = null;
            userProfile.BetaFeatureCodes = [];
            await this.profileDelegate.UpdateAsync(userProfile, ct: ct);

            // Update the notification settings
            await this.notificationSettingsService.QueueNotificationSettingsAsync(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SmsNumber), ct);

            MessagingVerification? lastEmailVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
            if (lastEmailVerification != null)
            {
                this.logger.LogDebug("Expiring old email verification");
                bool isDeleted = string.IsNullOrEmpty(emailAddress);
                await this.messageVerificationDelegate.ExpireAsync(lastEmailVerification, isDeleted, ct: ct);
                if (!isDeleted)
                {
                    this.logger.LogDebug("Sending new email verification");

                    if (lastEmailVerification.Email != null
                        && !string.IsNullOrEmpty(lastEmailVerification.Email.To)
                        && emailAddress.Equals(lastEmailVerification.Email.To, StringComparison.OrdinalIgnoreCase))
                    {
                        await this.AddVerificationEmailAsync(hdid, emailAddress, lastEmailVerification.InviteKey!.Value, ct: ct);
                    }
                    else
                    {
                        await this.AddVerificationEmailAsync(hdid, emailAddress, Guid.NewGuid(), ct: ct);
                    }
                }
            }
            else
            {
                await this.AddVerificationEmailAsync(hdid, emailAddress, Guid.NewGuid(), ct: ct);
            }

            return true;
        }

        [ExcludeFromCodeCoverage]
        private async Task AddVerificationEmailAsync(string hdid, string toEmail, Guid inviteKey, bool isVerified = false, CancellationToken ct = default)
        {
            this.logger.LogDebug("Adding verification email");
            float verificationExpiryHours = (float)this.emailVerificationExpirySeconds / 3600;

            Dictionary<string, string> keyValues = new()
            {
                [EmailTemplateVariable.InviteKey] = inviteKey.ToString(),
                [EmailTemplateVariable.ActivationHost] = this.emailTemplateConfig.Host,
                [EmailTemplateVariable.ExpiryHours] = verificationExpiryHours.ToString("0", CultureInfo.CurrentCulture),
            };

            EmailTemplate emailTemplate = await this.emailQueueService.GetEmailTemplateAsync(EmailTemplateName.RegistrationTemplate, ct) ??
                                          throw new DatabaseException(ErrorMessages.EmailTemplateNotFound);

            MessagingVerification messageVerification = new()
            {
                InviteKey = inviteKey,
                UserProfileId = hdid,
                ExpireDate = DateTime.UtcNow.AddSeconds(this.emailVerificationExpirySeconds),
                Email = this.emailQueueService.ProcessTemplate(toEmail, emailTemplate, keyValues),
                EmailAddress = toEmail,
            };

            if (isVerified)
            {
                messageVerification.Email.EmailStatusCode = EmailStatus.Processed;
                await this.messageVerificationDelegate.InsertAsync(messageVerification, ct: ct);
                await this.ValidateEmailAsync(hdid, inviteKey, ct);
            }
            else
            {
                await this.messageVerificationDelegate.InsertAsync(messageVerification, ct: ct);
                await this.emailQueueService.QueueNewEmailAsync(messageVerification.Email, ct: ct);
            }
        }
    }
}
