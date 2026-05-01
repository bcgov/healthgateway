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
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
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
    public partial class UserSmsServiceV2 : IUserSmsServiceV2
    {
        /// <summary>
        /// The maximum verification attempts.
        /// </summary>
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
        /// Initializes a new instance of the <see cref="UserSmsServiceV2"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="messagingVerificationService">The messaging verification service.</param>
        /// <param name="profileNotificationSettingService">The injected user profile notification setting service.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="jobService">The job service.</param>
        /// <param name="backgroundJobClient">Hangfire background job client.</param>
        /// <param name="transactionProvider">
        /// Provides database transaction and persistence operations for the current request
        /// scope.
        /// </param>
        /// <param name="configuration">The application's configuration.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public UserSmsServiceV2(
            ILogger<UserSmsServiceV2> logger,
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
            ChangeFeedOptions? changeFeedConfiguration = configuration.GetSection(ChangeFeedOptions.ChangeFeed)
                .Get<ChangeFeedOptions>();
            this.notificationsChangeFeedEnabled = changeFeedConfiguration?.Notifications.Enabled ?? false;
        }

        /// <inheritdoc/>
        public async Task<bool> VerifySmsNumberAsync(string hdid, string verificationCode, CancellationToken ct = default)
        {
            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);
            MessagingVerification? smsVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);

            if (smsVerification is not { Validated: false, Deleted: false, VerificationAttempts: < MaxVerificationAttempts } ||
                smsVerification.SmsValidationCode != verificationCode ||
                smsVerification.ExpireDate < DateTime.UtcNow)
            {
                if (smsVerification is { Validated: false })
                {
                    smsVerification.VerificationAttempts++;
                    await this.messageVerificationDelegate.UpdateAsync(smsVerification, ct: ct);
                }

                this.logger.LogDebug("Finished verifying sms");
                return false;
            }

            // Begin transaction for all database updates
            await using IDbContextTransaction transaction =
                await this.transactionProvider.BeginTransactionAsync(ct);

            smsVerification.Validated = true;
            await this.messageVerificationDelegate.UpdateAsync(smsVerification, false, ct);

            userProfile.SmsNumber = smsVerification.SmsNumber; // Gets the user sms number from the message sent.
            DbResult<UserProfile> dbResult = await this.profileDelegate.UpdateAsync(userProfile, false, ct);
            if (dbResult.Status == DbStatusCode.NotFound)
            {
                throw new NotFoundException(ErrorMessages.UserProfileNotFound);
            }

            if (this.notificationsChangeFeedEnabled)
            {
                await this.jobService.NotifySmsVerificationAsync(hdid, smsVerification.SmsNumber, false, ct);
            }

            // Enable default user profile notification settings after successful verification
            UserProfileNotificationSettingModel[] notificationSettingModels =
            [
                new()
                {
                    Type = ProfileNotificationType.BcCancerScreening,
                    EmailEnabled = null,
                    SmsEnabled = true,
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
            await this.jobService.QueueNotificationSettingsRequestAsync(userProfile, userProfile.Email, userProfile.SmsNumber, ct: ct);

            this.logger.LogDebug("Finished verifying sms");
            return true;
        }

        /// <inheritdoc/>
        public async Task UpdateSmsNumberAsync(string hdid, string sms, CancellationToken ct = default)
        {
            string sanitizedSms = SanitizeSms(sms);
            await UserProfileValidator.ValidateSmsNumberAndThrowAsync(sanitizedSms, ct);

            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            userProfile.SmsNumber = null;

            this.logger.LogDebug("Clearing user's SMS number");
            DbResult<UserProfile> dbResult = await this.profileDelegate.UpdateAsync(userProfile, ct: ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }

            bool isDeleted = string.IsNullOrEmpty(sanitizedSms);
            MessagingVerification? lastSmsVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);
            if (lastSmsVerification != null)
            {
                this.logger.LogDebug("Expiring old SMS messaging verification");
                await this.messageVerificationDelegate.ExpireAsync(lastSmsVerification, isDeleted, ct: ct);
            }

            if (!isDeleted)
            {
                MessagingVerification messagingVerification = this.messagingVerificationService.GenerateMessagingVerification(hdid, sanitizedSms, false);
                this.logger.LogDebug("Adding SMS messaging verification");
                await this.messageVerificationDelegate.InsertAsync(messagingVerification, true, ct);

                // Update the notification settings
                await this.jobService.QueueNotificationSettingsRequestAsync(userProfile, userProfile.Email, sanitizedSms, messagingVerification.SmsValidationCode, ct);
            }
            else
            {
                // Update the notification settings
                await this.jobService.QueueNotificationSettingsRequestAsync(userProfile, userProfile.Email, sanitizedSms, ct: ct);
            }
        }

        private static string SanitizeSms(string smsNumber)
        {
            return NonDigitRegex().Replace(smsNumber, string.Empty);
        }

        [GeneratedRegex("[^0-9]")]
        private static partial Regex NonDigitRegex();
    }
}
