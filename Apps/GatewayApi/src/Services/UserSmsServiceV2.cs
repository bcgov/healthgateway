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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public partial class UserSmsServiceV2 : IUserSmsServiceV2
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
        private readonly IMessageSender messageSender;
        private readonly bool notificationsChangeFeedEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSmsServiceV2"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        /// <param name="messageSender">The change feed message sender.</param>
        /// <param name="configuration">The application's configuration.</param>
        public UserSmsServiceV2(
            ILogger<UserSmsServiceV2> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IUserProfileDelegate profileDelegate,
            INotificationSettingsService notificationSettingsService,
            IMessageSender messageSender,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.profileDelegate = profileDelegate;
            this.notificationSettingsService = notificationSettingsService;
            this.messageSender = messageSender;
            ChangeFeedOptions? changeFeedConfiguration = configuration.GetSection(ChangeFeedOptions.ChangeFeed)
                .Get<ChangeFeedOptions>();
            this.notificationsChangeFeedEnabled = changeFeedConfiguration?.Notifications.Enabled ?? false;
        }

        /// <inheritdoc/>
        public async Task<bool> VerifySmsNumberAsync(string hdid, string verificationCode, CancellationToken ct = default)
        {
            this.logger.LogTrace("Verifying sms... {ValidationCode}", verificationCode);

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

            smsVerification.Validated = true;
            await this.messageVerificationDelegate.UpdateAsync(smsVerification, false, ct);

            userProfile.SmsNumber = smsVerification.SmsNumber; // Gets the user sms number from the message sent.
            DbResult<UserProfile> dbResult = await this.profileDelegate.UpdateAsync(userProfile, true, ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }

            if (this.notificationsChangeFeedEnabled)
            {
                MessageEnvelope[] events = [new(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Sms, smsVerification.SmsNumber), hdid)];
                await this.messageSender.SendAsync(events, ct);
            }

            // Update the notification settings
            await this.notificationSettingsService.QueueNotificationSettingsAsync(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SmsNumber), ct);

            this.logger.LogDebug("Finished verifying sms");
            return true;
        }

        /// <inheritdoc/>
        public MessagingVerification GenerateMessagingVerification(string hdid, string sms, bool sanitize = true)
        {
            this.logger.LogInformation("Generating new sms verification for user {Hdid}", hdid);
            if (sanitize)
            {
                sms = SanitizeSms(sms);
            }

            MessagingVerification messagingVerification = new()
            {
                UserProfileId = hdid,
                SmsNumber = sms,
                SmsValidationCode = CreateVerificationCode(),
                VerificationType = MessagingVerificationType.Sms,
                ExpireDate = DateTime.UtcNow.AddDays(VerificationExpiryDays),
            };

            return messagingVerification;
        }

        /// <inheritdoc/>
        public async Task UpdateSmsNumberAsync(string hdid, string sms, CancellationToken ct = default)
        {
            this.logger.LogTrace("Removing user sms number {Hdid}", hdid);
            string sanitizedSms = SanitizeSms(sms);
            await UserProfileValidator.ValidateSmsNumberAndThrowAsync(sanitizedSms, ct);

            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            userProfile.SmsNumber = null;

            DbResult<UserProfile> dbResult = await this.profileDelegate.UpdateAsync(userProfile, ct: ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }

            bool isDeleted = string.IsNullOrEmpty(sanitizedSms);
            MessagingVerification? lastSmsVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);
            if (lastSmsVerification != null)
            {
                this.logger.LogInformation("Expiring old sms validation for user {Hdid}", hdid);
                await this.messageVerificationDelegate.ExpireAsync(lastSmsVerification, isDeleted, ct: ct);
            }

            NotificationSettingsRequest notificationRequest = new(userProfile, userProfile.Email, sanitizedSms);
            if (!isDeleted)
            {
                this.logger.LogInformation("Adding new sms verification for user {Hdid}", hdid);
                MessagingVerification messagingVerification = this.GenerateMessagingVerification(hdid, sanitizedSms, false);
                await this.messageVerificationDelegate.InsertAsync(messagingVerification, true, ct);
                notificationRequest.SmsVerificationCode = messagingVerification.SmsValidationCode;
            }

            // Update the notification settings
            await this.notificationSettingsService.QueueNotificationSettingsAsync(notificationRequest, ct);

            this.logger.LogDebug("Finished updating user sms");
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

        private static string SanitizeSms(string smsNumber)
        {
            return NonDigitRegex().Replace(smsNumber, string.Empty);
        }

        [GeneratedRegex("[^0-9]")]
        private static partial Regex NonDigitRegex();
    }
}
