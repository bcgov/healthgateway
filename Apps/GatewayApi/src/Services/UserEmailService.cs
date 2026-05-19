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
    using Hangfire;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Providers;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly IUserProfileNotificationSettingService profileNotificationSettingService;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly ChangeFeedOptions changeFeedConfiguration;
        private readonly IOutboxStoreService outboxStoreService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IGatewayDbContextTransactionProvider transactionProvider;
        private readonly EmailTemplateConfig emailTemplateConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="notificationSettingsService">Notification settings service.</param>
        /// <param name="profileNotificationSettingService">The injected user profile notification setting service.</param>
        /// <param name="outboxStoreService">The outbox store service.</param>
        /// <param name="backgroundJobClient">Hangfire background job client.</param>
        /// <param name="transactionProvider">
        /// Provides database transaction and persistence operations for the current request
        /// scope.
        /// </param>
        /// <param name="configuration">Configuration settings.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public UserEmailService(
            ILogger<UserEmailService> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IUserProfileDelegate profileDelegate,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService,
            IUserProfileNotificationSettingService profileNotificationSettingService,
            IOutboxStoreService outboxStoreService,
            IBackgroundJobClient backgroundJobClient,
            IGatewayDbContextTransactionProvider transactionProvider,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
            this.profileNotificationSettingService = profileNotificationSettingService;
            this.emailVerificationExpirySeconds = configuration.GetSection(WebClientConfigSection).GetValue(EmailConfigExpirySecondsKey, 5);

            this.outboxStoreService = outboxStoreService;
            this.backgroundJobClient = backgroundJobClient;
            this.transactionProvider = transactionProvider;
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

            // Begin transaction for all database updates
            await using IDbContextTransaction transaction =
                await this.transactionProvider.BeginTransactionAsync(ct);

            matchingVerification.Validated = true;
            userProfile.Email = matchingVerification.Email!.To; // Gets the user email from the email sent.

            if (this.changeFeedConfiguration.Notifications.Enabled)
            {
                // Store an event indicating email was verified.
                await this.outboxStoreService.QueueEmailVerificationEventAsync(hdid, matchingVerification.Email!.To, false, ct);
            }

            // Enable default user profile notification settings after successful verification
            UserProfileNotificationSettingModel[] notificationSettingModels =
            [
                new()
                {
                    Type = ProfileNotificationType.BcCancerScreening,
                    EmailEnabled = true,
                    SmsEnabled = null,
                },
            ];

            // Store an event indicating user profile notification settings have been defaulted.
            await this.profileNotificationSettingService.UpdateAsync(hdid, notificationSettingModels, false, ct);

            // Persist changes within transaction
            await this.transactionProvider.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // Dispatch outbox events after commit
            this.logger.LogDebug("Dispatching events after commit");
            this.backgroundJobClient.Enqueue<DbOutboxStore>(store =>
                store.DispatchOutboxItemsAsync(ct));

            // Queue background job to push notification settings through the job scheduler for user and dependents.
            NotificationSettingsRequest notificationSettingsRequest = new(userProfile, userProfile.Email, userProfile.SmsNumber);
            await this.notificationSettingsService.QueueNotificationSettingsAsync(notificationSettingsRequest, ct);

            // Verification verified
            return new RequestResult<bool>
            {
                ResourcePayload = true,
                ResultStatus = ResultType.Success,
            };
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

            // Begin transaction for all database updates
            await using IDbContextTransaction transaction =
                await this.transactionProvider.BeginTransactionAsync(ct);

            this.logger.LogDebug("Update user profile - clearing user's email address and beta feature codes");
            userProfile.Email = null;
            userProfile.BetaFeatureCodes = [];

            MessagingVerification? lastEmailVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);

            MessagingVerification? emailVerification = null;

            if (lastEmailVerification != null)
            {
                this.logger.LogDebug("Expiring old email verification");

                bool isDeleted = string.IsNullOrEmpty(emailAddress);
                lastEmailVerification.ExpireDate = DateTime.UtcNow;
                lastEmailVerification.Deleted = isDeleted;

                if (!isDeleted)
                {
                    this.logger.LogDebug("Sending new email verification");
                    Guid inviteKey = Guid.NewGuid();

                    if (lastEmailVerification.Email?.To is { } lastEmail
                        && emailAddress.Equals(lastEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        inviteKey = lastEmailVerification.InviteKey!.Value;
                    }

                    emailVerification = await this.GenerateEmailVerificationAsync(hdid, emailAddress, inviteKey, ct);
                }
            }
            else
            {
                emailVerification = await this.GenerateEmailVerificationAsync(hdid, emailAddress, Guid.NewGuid(), ct);
            }

            if (emailVerification != null)
            {
                await this.messageVerificationDelegate.InsertAsync(emailVerification, false, ct);
                await this.emailQueueService.QueueNewEmailAsync(emailVerification.Email, false, ct);
            }

            // Persist changes within transaction
            await this.transactionProvider.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            if (emailVerification != null)
            {
                // Queue a background job to send the email.
                this.backgroundJobClient.Enqueue<IEmailJob>(j => j.SendEmailAsync(emailVerification.Email!.Id, ct));
            }

            // Queue background job to push notification settings through the job scheduler for user and dependents.
            NotificationSettingsRequest notificationSettingsRequest = new(userProfile, userProfile.Email, userProfile.SmsNumber);
            await this.notificationSettingsService.QueueNotificationSettingsAsync(notificationSettingsRequest, ct);

            return true;
        }

        [ExcludeFromCodeCoverage]
        private async Task<MessagingVerification> GenerateEmailVerificationAsync(string hdid, string toEmail, Guid inviteKey, CancellationToken ct = default)
        {
            this.logger.LogDebug("Generate email verification");
            float verificationExpiryHours = (float)this.emailVerificationExpirySeconds / 3600;

            Dictionary<string, string> keyValues = new()
            {
                [EmailTemplateVariable.InviteKey] = inviteKey.ToString(),
                [EmailTemplateVariable.ActivationHost] = this.emailTemplateConfig.Host,
                [EmailTemplateVariable.ExpiryHours] = verificationExpiryHours.ToString("0", CultureInfo.CurrentCulture),
            };

            EmailTemplate emailTemplate = await this.emailQueueService.GetEmailTemplateAsync(EmailTemplateName.RegistrationTemplate, ct) ??
                                          throw new DatabaseException(ErrorMessages.EmailTemplateNotFound);

            return new()
            {
                InviteKey = inviteKey,
                UserProfileId = hdid,
                ExpireDate = DateTime.UtcNow.AddSeconds(this.emailVerificationExpirySeconds),
                Email = this.emailQueueService.ProcessTemplate(toEmail, emailTemplate, keyValues),
                EmailAddress = toEmail,
            };
        }
    }
}
