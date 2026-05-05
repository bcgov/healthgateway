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
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling.Exceptions;
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
    public partial class UserSmsService : IUserSmsService
    {
        /// <summary>
        /// The maximum verification attempts.
        /// </summary>
        public const int MaxVerificationAttempts = 5;
        private const int VerificationExpiryDays = 5;
        private readonly ILogger<UserSmsService> logger;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IUserProfileNotificationSettingService profileNotificationSettingService;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IJobService jobService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IGatewayDbContextTransactionProvider transactionProvider;
        private readonly ChangeFeedOptions changeFeedConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSmsService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        /// <param name="profileNotificationSettingService">The injected user profile notification setting service.</param>
        /// <param name="jobService">The job service.</param>
        /// <param name="backgroundJobClient">Hangfire background job client.</param>
        /// <param name="transactionProvider">
        /// Provides database transaction and persistence operations for the current request
        /// scope.
        /// </param>
        /// <param name="configuration">The application's configuration.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public UserSmsService(
            ILogger<UserSmsService> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IUserProfileDelegate profileDelegate,
            INotificationSettingsService notificationSettingsService,
            IUserProfileNotificationSettingService profileNotificationSettingService,
            IJobService jobService,
            IBackgroundJobClient backgroundJobClient,
            IGatewayDbContextTransactionProvider transactionProvider,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.notificationSettingsService = notificationSettingsService;
            this.profileNotificationSettingService = profileNotificationSettingService;
            this.jobService = jobService;
            this.backgroundJobClient = backgroundJobClient;
            this.transactionProvider = transactionProvider;
            this.changeFeedConfiguration = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>() ?? new();
        }

        /// <inheritdoc/>
        public async Task<RequestResult<bool>> ValidateSmsAsync(string hdid, string validationCode, CancellationToken ct = default)
        {
            RequestResult<bool> retVal = new() { ResourcePayload = false, ResultStatus = ResultType.Success };
            MessagingVerification? smsVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);

            UserProfile? userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct);
            if (userProfile != null &&
                smsVerification is { Validated: false, Deleted: false, VerificationAttempts: < MaxVerificationAttempts } &&
                smsVerification.UserProfileId == hdid &&
                smsVerification.SmsValidationCode == validationCode &&
                smsVerification.ExpireDate >= DateTime.UtcNow)
            {
                // Begin transaction for all database updates
                await using IDbContextTransaction transaction =
                    await this.transactionProvider.BeginTransactionAsync(ct);

                smsVerification.Validated = true;
                await this.messageVerificationDelegate.UpdateAsync(smsVerification, false, ct);

                userProfile.SmsNumber = smsVerification.SmsNumber; // Gets the user sms number from the message sent.

                if (this.changeFeedConfiguration.Notifications.Enabled)
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
                await this.notificationSettingsService.QueueNotificationSettingsAsync(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SmsNumber), ct);

                retVal.ResourcePayload = true;
            }
            else
            {
                smsVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);
                if (smsVerification is { Validated: false })
                {
                    smsVerification.VerificationAttempts++;
                    await this.messageVerificationDelegate.UpdateAsync(smsVerification, ct: ct);
                }
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<MessagingVerification> CreateUserSmsAsync(string hdid, string sms, CancellationToken ct = default)
        {
            string sanitizedSms = SanitizeSms(sms);
            return await this.AddVerificationSmsAsync(hdid, sanitizedSms, ct);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUserSmsAsync(string hdid, string sms, CancellationToken ct = default)
        {
            string sanitizedSms = SanitizeSms(sms);
            await UserProfileValidator.ValidateSmsNumberAndThrowAsync(sanitizedSms, ct);

            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct) ??
                                      throw new NotFoundException($"User profile not found for hdid {hdid}");

            userProfile.SmsNumber = null;
            this.logger.LogDebug("Clearing user's SMS number");
            await this.profileDelegate.UpdateAsync(userProfile, ct: ct);

            bool isDeleted = string.IsNullOrEmpty(sanitizedSms);
            MessagingVerification? lastSmsVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);
            if (lastSmsVerification != null)
            {
                this.logger.LogDebug("Expiring old SMS messaging verification");
                await this.messageVerificationDelegate.ExpireAsync(lastSmsVerification, isDeleted, ct: ct);
            }

            NotificationSettingsRequest notificationRequest = new(userProfile, userProfile.Email, sanitizedSms);
            if (!isDeleted)
            {
                MessagingVerification messagingVerification = await this.AddVerificationSmsAsync(hdid, sanitizedSms, ct);
                notificationRequest.SmsVerificationCode = messagingVerification.SmsValidationCode;
            }

            // Update the notification settings
            await this.notificationSettingsService.QueueNotificationSettingsAsync(notificationRequest, ct);
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
                    .ToString("D6", CultureInfo.InvariantCulture)[..6];
        }

        private static string SanitizeSms(string smsNumber)
        {
            return NonDigitRegex().Replace(smsNumber, string.Empty);
        }

        [GeneratedRegex("[^0-9]")]
        private static partial Regex NonDigitRegex();

        private async Task<MessagingVerification> AddVerificationSmsAsync(string hdid, string sms, CancellationToken ct = default)
        {
            MessagingVerification messagingVerification = new()
            {
                UserProfileId = hdid,
                SmsNumber = sms,
                SmsValidationCode = CreateVerificationCode(),
                VerificationType = MessagingVerificationType.Sms,
                ExpireDate = DateTime.UtcNow.AddDays(VerificationExpiryDays),
            };

            this.logger.LogDebug("Adding SMS messaging verification");
            await this.messageVerificationDelegate.InsertAsync(messagingVerification, ct: ct);
            return messagingVerification;
        }
    }
}
