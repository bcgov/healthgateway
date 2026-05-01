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
    using HealthGateway.Common.Messaging;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Providers;
    using HealthGateway.Database.Wrapper;
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
        private readonly IUserProfileNotificationSettingService profileNotificationSettingService;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IJobService jobService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IGatewayDbContextTransactionProvider transactionProvider;
        private readonly bool notificationsChangeFeedEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailServiceV2"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="messagingVerificationService">The messaging verification service.</param>
        /// <param name="profileNotificationSettingService">The injected user profile notification setting service.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="jobService">The injected job service.</param>
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
            IUserProfileNotificationSettingService profileNotificationSettingService,
            IUserProfileDelegate profileDelegate,
            IJobService jobService,
            IBackgroundJobClient backgroundJobClient,
            IGatewayDbContextTransactionProvider transactionProvider,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.messagingVerificationService = messagingVerificationService;
            this.profileNotificationSettingService = profileNotificationSettingService;
            this.profileDelegate = profileDelegate;
            this.jobService = jobService;
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
                await this.IncrementEmailVerificationAttempts(hdid, ct);
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

            // Update verification state and send confirmation (DB + related logic)
            await this.SetAddressValidated(userProfile, matchingVerification, ct);
            await this.NotifyVerificationSuccessful(hdid, matchingVerification.Email!.To, false, ct);

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

            await this.profileNotificationSettingService.UpdateAsync(hdid, notificationSettingModels, false, ct);

            // Persist changes within transaction
            await this.transactionProvider.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // Dispatch outbox events after commit
            this.logger.LogDebug("Dispatching events after commit");
            this.backgroundJobClient.Enqueue<DbOutboxStore>(store =>
                store.DispatchOutboxItemsAsync(ct));

            // Update notification settings after commit
            await this.QueueNotificationSettingsRequest(userProfile, ct);

            return true;
        }

        /// <inheritdoc/>
        public async Task UpdateEmailAddressAsync(string hdid, string emailAddress, CancellationToken ct = default)
        {
            bool isEmpty = string.IsNullOrEmpty(emailAddress);
            Guid inviteKey = Guid.NewGuid();

            await new OptionalEmailAddressValidator().ValidateAndThrowAsync(emailAddress, ct);
            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, true, ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            // expire previous email verification in DB without committing changes
            MessagingVerification? lastEmailVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
            if (lastEmailVerification != null)
            {
                this.logger.LogDebug("Expiring old email verification");
                await this.messageVerificationDelegate.ExpireAsync(lastEmailVerification, isEmpty, false, ct);
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
                messagingVerification = await this.messagingVerificationService.GenerateMessagingVerificationAsync(hdid, emailAddress, inviteKey, false, ct);
                await this.messageVerificationDelegate.InsertAsync(messagingVerification, false, ct);
            }

            this.logger.LogDebug("Clearing user's email address");
            userProfile.Email = null;
            userProfile.BetaFeatureCodes = [];

            // update user profile in DB and commit changes
            DbResult<UserProfile> dbResult = await this.profileDelegate.UpdateAsync(userProfile, true, ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }

            if (messagingVerification != null)
            {
                this.logger.LogDebug("Sending new email verification");
                await this.jobService.SendEmailAsync(messagingVerification.Email, true, ct);
            }

            await this.QueueNotificationSettingsRequest(userProfile, ct);
        }

        private async Task IncrementEmailVerificationAttempts(string hdid, CancellationToken ct)
        {
            MessagingVerification? latestEmailVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
            if (latestEmailVerification is { Validated: false })
            {
                latestEmailVerification.VerificationAttempts++;
                await this.messageVerificationDelegate.UpdateAsync(latestEmailVerification, true, ct);
            }
        }

        private async Task SetAddressValidated(UserProfile userProfile, MessagingVerification matchingVerification, CancellationToken ct = default)
        {
            matchingVerification.Validated = true;
            await this.messageVerificationDelegate.UpdateAsync(matchingVerification, false, ct);

            userProfile.Email = matchingVerification.Email!.To;
            await this.profileDelegate.UpdateAsync(userProfile, false, ct);
        }

        private async Task NotifyVerificationSuccessful(string hdid, string emailAddress, bool shouldCommit, CancellationToken ct)
        {
            if (this.notificationsChangeFeedEnabled)
            {
                await this.jobService.NotifyEmailVerificationAsync(hdid, emailAddress, shouldCommit, ct);
            }
        }

        private async Task QueueNotificationSettingsRequest(UserProfile userProfile, CancellationToken ct)
        {
            await this.jobService.QueueNotificationSettingsRequestAsync(userProfile, userProfile.Email, userProfile.SmsNumber, ct: ct);
        }
    }
}
