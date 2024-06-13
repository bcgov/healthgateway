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
    using HealthGateway.GatewayApi.Validations;
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
        private readonly ILogger logger;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IMessageSender messageSender;
        private readonly bool notificationsChangeFeedEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSmsService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        /// <param name="messageSender">The change feed message sender.</param>
        /// <param name="configuration">The application's configuration.</param>
        public UserSmsService(
            ILogger<UserSmsService> logger,
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
        public async Task<RequestResult<bool>> ValidateSmsAsync(string hdid, string validationCode, CancellationToken ct = default)
        {
            this.logger.LogTrace("Validating sms... {ValidationCode}", validationCode);

            RequestResult<bool> retVal = new() { ResourcePayload = false, ResultStatus = ResultType.Success };
            MessagingVerification? smsVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);

            UserProfile? userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct);
            if (userProfile != null &&
                smsVerification is { Validated: false, Deleted: false, VerificationAttempts: < MaxVerificationAttempts } &&
                smsVerification.UserProfileId == hdid &&
                smsVerification.SmsValidationCode == validationCode &&
                smsVerification.ExpireDate >= DateTime.UtcNow)
            {
                smsVerification.Validated = true;
                await this.messageVerificationDelegate.UpdateAsync(smsVerification, false, ct);

                userProfile.SmsNumber = smsVerification.SmsNumber; // Gets the user sms number from the message sent.
                DbResult<UserProfile> dbResult = await this.profileDelegate.UpdateAsync(userProfile, true, ct);
                if (dbResult.Status != DbStatusCode.Updated)
                {
                    return RequestResultFactory.ServiceError<bool>(ErrorType.CommunicationInternal, ServiceType.Database, ErrorMessages.CannotPerformAction);
                }

                if (this.notificationsChangeFeedEnabled)
                {
                    MessageEnvelope[] events = [new(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Sms, smsVerification.SmsNumber), hdid)];
                    await this.messageSender.SendAsync(events, ct);
                }

                retVal.ResourcePayload = true;

                // Update the notification settings
                await this.notificationSettingsService.QueueNotificationSettingsAsync(new NotificationSettingsRequest(userProfile, userProfile.Email, userProfile.SmsNumber), ct);
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

            this.logger.LogDebug("Finished validating sms");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<MessagingVerification> CreateUserSmsAsync(string hdid, string sms, CancellationToken ct = default)
        {
            this.logger.LogInformation("Adding new sms verification for user {Hdid}", hdid);
            string sanitizedSms = SanitizeSms(sms);
            MessagingVerification messagingVerification = await this.AddVerificationSmsAsync(hdid, sanitizedSms, ct);
            this.logger.LogDebug("Finished updating user sms");
            return messagingVerification;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUserSmsAsync(string hdid, string sms, CancellationToken ct = default)
        {
            this.logger.LogTrace("Removing user sms number {Hdid}", hdid);
            string sanitizedSms = SanitizeSms(sms);
            await UserProfileValidator.ValidateSmsNumberAndThrowAsync(sanitizedSms, ct);

            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct) ??
                                      throw new NotFoundException($"User profile not found for hdid {hdid}");

            userProfile.SmsNumber = null;
            await this.profileDelegate.UpdateAsync(userProfile, ct: ct);

            bool isDeleted = string.IsNullOrEmpty(sanitizedSms);
            MessagingVerification? lastSmsVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);
            if (lastSmsVerification != null)
            {
                this.logger.LogInformation("Expiring old sms validation for user {Hdid}", hdid);
                await this.messageVerificationDelegate.ExpireAsync(lastSmsVerification, isDeleted, ct);
            }

            NotificationSettingsRequest notificationRequest = new(userProfile, userProfile.Email, sanitizedSms);
            if (!isDeleted)
            {
                this.logger.LogInformation("Adding new sms verification for user {Hdid}", hdid);
                MessagingVerification messagingVerification = await this.AddVerificationSmsAsync(hdid, sanitizedSms, ct);
                notificationRequest.SmsVerificationCode = messagingVerification.SmsValidationCode;
            }

            // Update the notification settings
            await this.notificationSettingsService.QueueNotificationSettingsAsync(notificationRequest, ct);

            this.logger.LogDebug("Finished updating user sms");
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
                    .ToString("D6", CultureInfo.InvariantCulture)
                    .Substring(0, 6);
        }

        private static string SanitizeSms(string smsNumber)
        {
            return NonDigitRegex().Replace(smsNumber, string.Empty);
        }

        [GeneratedRegex("[^0-9]")]
        private static partial Regex NonDigitRegex();

        private async Task<MessagingVerification> AddVerificationSmsAsync(string hdid, string sms, CancellationToken ct = default)
        {
            this.logger.LogInformation("Sending new sms verification for user {Hdid}", hdid);
            MessagingVerification messagingVerification = new()
            {
                UserProfileId = hdid,
                SmsNumber = sms,
                SmsValidationCode = CreateVerificationCode(),
                VerificationType = MessagingVerificationType.Sms,
                ExpireDate = DateTime.UtcNow.AddDays(VerificationExpiryDays),
            };

            await this.messageVerificationDelegate.InsertAsync(messagingVerification, ct: ct);
            return messagingVerification;
        }
    }
}
