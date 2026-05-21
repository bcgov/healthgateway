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
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using Hangfire;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Delegates;
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

    /// <inheritdoc/>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="authenticationDelegate">The injected authentication delegate.</param>
    /// <param name="cryptoDelegate">The injected crypto delegate.</param>
    /// <param name="messagingVerificationService">The injected message verification service.</param>
    /// <param name="outboxStoreService">The injected job service.</param>
    /// <param name="patientDetailsService">The injected patient details service.</param>
    /// <param name="emailQueueService">The injected email queue service.</param>
    /// <param name="userProfileDelegate">The injected user profile database delegate.</param>
    /// <param name="userProfileModelService">The injected user profile model service.</param>
    /// <param name="notificationSettingsService">Notification settings service.</param>
    /// <param name="backgroundJobClient">Hangfire background job client.</param>
    /// <param name="transactionProvider">
    /// Provides database transaction and persistence operations for the current request
    /// scope.
    /// </param>
#pragma warning disable S107 // The number of DI parameters should be ignored
    public class RegistrationService(
        IConfiguration configuration,
        IAuthenticationDelegate authenticationDelegate,
        ICryptoDelegate cryptoDelegate,
        IMessagingVerificationService messagingVerificationService,
        IOutboxStoreService outboxStoreService,
        IPatientDetailsService patientDetailsService,
        IUserProfileDelegate userProfileDelegate,
        IUserProfileModelService userProfileModelService,
        IEmailQueueService emailQueueService,
        INotificationSettingsService notificationSettingsService,
        IBackgroundJobClient backgroundJobClient,
        IGatewayDbContextTransactionProvider transactionProvider) : IRegistrationService
    {
        private const string MinPatientAgeKey = "MinPatientAge";
        private const string UserProfileHistoryRecordLimitKey = "UserProfileHistoryRecordLimit";
        private const string WebClientConfigSection = "WebClient";
        private const int DefaultPatientAge = 12;
        private const int DefaultUserProfileHistoryRecordLimit = 4;
        private readonly int minPatientAge = configuration.GetSection(WebClientConfigSection).GetValue(MinPatientAgeKey, DefaultPatientAge);
        private readonly int userProfileHistoryRecordLimit = configuration.GetSection(WebClientConfigSection).GetValue(UserProfileHistoryRecordLimitKey, DefaultUserProfileHistoryRecordLimit);
        private readonly bool accountsChangeFeedEnabled = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>()?.Accounts.Enabled ?? false;
        private readonly bool notificationsChangeFeedEnabled = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>()?.Notifications.Enabled ?? false;

        /// <inheritdoc/>
        [SuppressMessage(
            "Maintainability",
            "CA1506:Avoid excessive class coupling",
            Justification = "Service orchestration method requiring multiple domain dependencies.")]
        public async Task<UserProfileModel> CreateUserProfileAsync(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string? jwtEmailAddress, CancellationToken ct = default)
        {
            string hdid = createProfileRequest.Profile.HdId;
            string? requestedEmail = createProfileRequest.Profile.Email;
            string? requestedSmsNumber = createProfileRequest.Profile.SmsNumber;
            bool isEmailVerified = !string.IsNullOrWhiteSpace(requestedEmail) && string.Equals(requestedEmail, jwtEmailAddress, StringComparison.OrdinalIgnoreCase);

            // validate provided SMS number and patient age
            await new SmsNumberValidator().ValidateAndThrowAsync(requestedSmsNumber, ct);
            PatientDetails patient = await patientDetailsService.GetPatientAsync(hdid, ct: ct);
            if (this.minPatientAge > 0)
            {
                await new AgeRangeValidator(this.minPatientAge).ValidateAndThrowAsync(patient.Birthdate.ToDateTime(TimeOnly.MinValue), ct);
            }

            // Begin transaction for all database updates
            await using IDbContextTransaction transaction =
                await transactionProvider.BeginTransactionAsync(ct);

            // Generate and add SMS messaging verification to DB without committing changes
            MessagingVerification? smsVerification = !string.IsNullOrWhiteSpace(requestedSmsNumber)
                ? await messagingVerificationService.AddSmsVerificationAsync(hdid, requestedSmsNumber, false, ct)
                : null;

            MessagingVerification? emailVerification = !string.IsNullOrWhiteSpace(requestedEmail)
                ? await messagingVerificationService.AddEmailVerificationAsync(hdid, requestedEmail, isEmailVerified, false, ct)
                : null;

            // initialize user profile
            UserProfile profile = new()
            {
                HdId = hdid,
                IdentityManagementId = null,
                TermsOfServiceId = createProfileRequest.Profile.TermsOfServiceId,
                Email = isEmailVerified ? requestedEmail : string.Empty,
                SmsNumber = null,
                CreatedBy = hdid,
                UpdatedBy = hdid,
                LastLoginDateTime = jwtAuthTime,
                EncryptionKey = cryptoDelegate.GenerateKey(),
                YearOfBirth = patient.Birthdate.Year,
                LastLoginClientCode = authenticationDelegate.FetchAuthenticatedUserClientType(),
            };

            // add user profile to DB
            await userProfileDelegate.InsertUserProfileAsync(profile, false, ct);

            if (emailVerification != null && !isEmailVerified)
            {
                // Persist the email for background processing.
                await emailQueueService.QueueNewEmailAsync(emailVerification.Email, false, ct);
            }

            if (this.accountsChangeFeedEnabled)
            {
                // Store an event indicating the account was created.
                await outboxStoreService.QueueAccountCreatedEventAsync(profile.HdId, false, ct);
            }

            if (isEmailVerified && this.notificationsChangeFeedEnabled)
            {
                // Store an event indicating the email was verified.
                await outboxStoreService.QueueEmailVerificationEventAsync(profile.HdId, requestedEmail, false, ct);
            }

            // Persist changes within transaction
            await transactionProvider.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // Dispatch outbox events after commit
            backgroundJobClient.Enqueue<DbOutboxStore>(store =>
                store.DispatchOutboxItemsAsync(ct));

            if (emailVerification != null && !isEmailVerified)
            {
                // Queue a background job to send the email.
                backgroundJobClient.Enqueue<IEmailJob>(j => j.SendEmailAsync(emailVerification.Email!.Id, ct));
            }

            // Intentionally use profile.SmsNumber here.
            // The requested SMS number is not persisted to UserProfile.SmsNumber until SMS verification succeeds.
            // UserSmsServiceV2.VerifySmsNumberAsync updates UserProfile.SmsNumber after successful verification.
            NotificationSettingsRequest notificationSettingsRequest = new(profile, profile.Email, profile.SmsNumber)
            {
                SmsVerificationCode = smsVerification?.SmsValidationCode,
            };

            // Queue background jobs to push notification settings updates for the primary user and corresponding dependent(s).
            await notificationSettingsService.QueueNotificationSettingsAsync(notificationSettingsRequest, ct);

            // build and return model
            return await userProfileModelService.BuildUserProfileModelAsync(profile, this.userProfileHistoryRecordLimit, ct);
        }
    }
}
