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
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using Hangfire;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
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
    public class UserEmailServiceV2 : IUserEmailServiceV2
    {
        private const int MaxVerificationAttempts = 5;

        private readonly ILogger logger;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly IMessagingVerificationService messagingVerificationService;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IUserProfileNotificationSettingService profileNotificationSettingService;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IEmailQueueService emailQueueService;
        private readonly IOutboxStoreService outboxStoreService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IGatewayDbContextTransactionProvider transactionProvider;
        private readonly bool notificationsChangeFeedEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailServiceV2"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="messagingVerificationService">The messaging verification service.</param>
        /// <param name="notificationSettingsService">Notification settings service.</param>
        /// <param name="profileNotificationSettingService">The injected user profile notification setting service.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="outboxStoreService">The injected outbox store service.</param>
        /// <param name="backgroundJobClient">Hangfire background job client.</param>
        /// <param name="transactionProvider">
        /// Provides database transaction and persistence operations for the current request
        /// scope.
        /// </param>
        /// <param name="configuration">Configuration settings.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public UserEmailServiceV2(
            ILogger<UserEmailServiceV2> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IMessagingVerificationService messagingVerificationService,
            INotificationSettingsService notificationSettingsService,
            IUserProfileNotificationSettingService profileNotificationSettingService,
            IUserProfileDelegate profileDelegate,
            IEmailQueueService emailQueueService,
            IOutboxStoreService outboxStoreService,
            IBackgroundJobClient backgroundJobClient,
            IGatewayDbContextTransactionProvider transactionProvider,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.messagingVerificationService = messagingVerificationService;
            this.notificationSettingsService = notificationSettingsService;
            this.profileNotificationSettingService = profileNotificationSettingService;
            this.profileDelegate = profileDelegate;
            this.emailQueueService = emailQueueService;
            this.outboxStoreService = outboxStoreService;
            this.backgroundJobClient = backgroundJobClient;
            this.transactionProvider = transactionProvider;
            this.notificationsChangeFeedEnabled = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>()?.Notifications.Enabled ?? false;
        }

        /// <inheritdoc/>
        public async Task<bool> VerifyEmailAddressAsync(string hdid, Guid inviteKey, CancellationToken ct = default)
        {
            MessagingVerification? matchingVerification = await this.messageVerificationDelegate.GetLastByInviteKeyAsync(inviteKey, ct);
            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            if (matchingVerification == null || matchingVerification.UserProfileId != hdid || matchingVerification.Deleted)
            {
                this.logger.LogDebug("Invalid email verification");
                await this.IncrementEmailVerificationAttempts(hdid, true, ct);
                return false;
            }

            if (matchingVerification.VerificationAttempts >= MaxVerificationAttempts || matchingVerification.ExpireDate < DateTime.UtcNow)
            {
                this.logger.LogDebug("Email verification expired");
                return false;
            }

            if (matchingVerification.Validated)
            {
                throw new AlreadyExistsException("Email already verified");
            }

            // Begin transaction for all database updates
            await using IDbContextTransaction transaction =
                await this.transactionProvider.BeginTransactionAsync(ct);

            // Email address validated
            matchingVerification.Validated = true;
            userProfile.Email = matchingVerification.Email!.To;

            if (this.notificationsChangeFeedEnabled)
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

            // Store an event indicating user profile notification setting haa been defaulted.
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

            return true;
        }

        /// <inheritdoc/>
        public async Task UpdateEmailAddressAsync(string hdid, string emailAddress, CancellationToken ct = default)
        {
            bool isEmpty = string.IsNullOrEmpty(emailAddress);
            Guid inviteKey = Guid.NewGuid();

            await new OptionalEmailAddressValidator().ValidateAndThrowAsync(emailAddress, ct);
            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, true, ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            // Begin transaction for all database updates
            await using IDbContextTransaction transaction =
                await this.transactionProvider.BeginTransactionAsync(ct);

            // expire previous email verification in DB without committing changes
            MessagingVerification? lastEmailVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
            if (lastEmailVerification != null)
            {
                this.logger.LogDebug("Expiring old email verification");
                lastEmailVerification.ExpireDate = DateTime.UtcNow;
                lastEmailVerification.Deleted = isEmpty;

                if (string.Equals(emailAddress, lastEmailVerification.Email?.To, StringComparison.OrdinalIgnoreCase))
                {
                    // reuse same invite key if the last verification was for the same email address
                    inviteKey = lastEmailVerification.InviteKey!.Value;
                }
            }

            // add new messaging verification to DB without committing changes
            MessagingVerification? messagingVerification = null;
            if (!isEmpty)
            {
                messagingVerification = await this.messagingVerificationService.AddEmailVerificationAsync(hdid, emailAddress, false, inviteKey, false, ct);
            }

            this.logger.LogDebug("Update user profile - clearing user's email address and beta feature codes");
            userProfile.Email = null;
            userProfile.BetaFeatureCodes = [];

            if (messagingVerification != null)
            {
                this.logger.LogDebug("Sending new email verification");
                await this.emailQueueService.QueueNewEmailAsync(messagingVerification.Email, false, ct);
            }

            // Persist changes within transaction
            await this.transactionProvider.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // Dispatch events after commit
            this.logger.LogDebug("Dispatching events after commit");
            if (messagingVerification is { Email: not null })
            {
                this.backgroundJobClient.Enqueue<IEmailJob>(j => j.SendEmailAsync(messagingVerification.Email.Id, ct));
            }

            // Queue background job to push notification settings through the job scheduler for user and dependents.
            NotificationSettingsRequest notificationSettingsRequest = new(userProfile, userProfile.Email, userProfile.SmsNumber);
            await this.notificationSettingsService.QueueNotificationSettingsAsync(notificationSettingsRequest, ct);
        }

        private async Task IncrementEmailVerificationAttempts(string hdid, bool shouldCommit, CancellationToken ct)
        {
            MessagingVerification? latestEmailVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
            if (latestEmailVerification is { Validated: false })
            {
                latestEmailVerification.VerificationAttempts++;
                await this.messageVerificationDelegate.UpdateAsync(latestEmailVerification, shouldCommit, ct);
            }
        }
    }
}
