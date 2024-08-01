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
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling.Exceptions;
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
    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationService"/> class.
    /// </summary>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="logger">The injected logger.</param>
    /// <param name="authenticationDelegate">The injected authentication delegate.</param>
    /// <param name="cryptoDelegate">The injected crypto delegate.</param>
    /// <param name="emailQueueService">The injected service to queue emails.</param>
    /// <param name="messageSender">The injected message sender.</param>
    /// <param name="messageVerificationDelegate">The injected message verification delegate.</param>
    /// <param name="notificationSettingsService">The injected notifications settings service.</param>
    /// <param name="patientDetailsService">The injected patient details service.</param>
    /// <param name="userEmailService">The injected user email service.</param>
    /// <param name="userSmsService">The injected user SMS service.</param>
    /// <param name="userProfileDelegate">The injected user profile database delegate.</param>
    /// <param name="userProfileService">The injected user profile service.</param>
    public class RegistrationService(
        IConfiguration configuration,
        ILogger<RegistrationService> logger,
        IAuthenticationDelegate authenticationDelegate,
        ICryptoDelegate cryptoDelegate,
        IEmailQueueService emailQueueService,
        IMessageSender messageSender,
        IMessagingVerificationDelegate messageVerificationDelegate,
        INotificationSettingsService notificationSettingsService,
        IPatientDetailsService patientDetailsService,
        IUserEmailServiceV2 userEmailService,
        IUserSmsServiceV2 userSmsService,
        IUserProfileDelegate userProfileDelegate,
        IUserProfileServiceV2 userProfileService) : IRegistrationService
    {
        private const string MinPatientAgeKey = "MinPatientAge";
        private const string WebClientConfigSection = "WebClient";
        private const int DefaultPatientAge = 12;
        private readonly int minPatientAge = configuration.GetSection(WebClientConfigSection).GetValue(MinPatientAgeKey, DefaultPatientAge);
        private readonly bool accountsChangeFeedEnabled = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>()?.Accounts.Enabled ?? false;
        private readonly bool notificationsChangeFeedEnabled = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>()?.Notifications.Enabled ?? false;

        /// <inheritdoc/>
        public async Task<UserProfileModel> CreateUserProfileAsync(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string? jwtEmailAddress, CancellationToken ct = default)
        {
            logger.LogTrace("Creating user profile... {Hdid}", createProfileRequest.Profile.HdId);
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

            // add SMS and email messaging verifications to DB without committing changes
            MessagingVerification? smsVerification = await this.AddSmsVerification(hdid, requestedSmsNumber, ct);
            MessagingVerification? emailVerification = await this.AddEmailVerification(hdid, requestedEmail, isEmailVerified, ct);

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

            // add user profile to DB and commit changes
            DbResult<UserProfile> dbResult = await userProfileDelegate.InsertUserProfileAsync(profile, true, ct);
            if (dbResult.Status != DbStatusCode.Created)
            {
                logger.LogError("Error creating user profile... {Hdid}", hdid);
                throw new DatabaseException(dbResult.Message);
            }

            // queue verification email
            if (emailVerification != null && !isEmailVerified)
            {
                await emailQueueService.QueueNewEmailAsync(emailVerification.Email, true, ct);
            }

            // notify partners about new account
            await this.NotifyAccountCreated(profile, requestedEmail, requestedSmsNumber, isEmailVerified, smsVerification?.SmsValidationCode, ct);

            // build and return model
            UserProfileModel userProfileModel = await userProfileService.GetUserProfileAsync(hdid, jwtAuthTime, ct);
            logger.LogTrace("Finished creating user profile... {Hdid}", dbResult.Payload.HdId);
            return userProfileModel;
        }

        private async Task<MessagingVerification?> AddSmsVerification(string hdid, string? requestedSmsNumber, CancellationToken ct)
        {
            MessagingVerification? smsVerification = null;
            if (!string.IsNullOrWhiteSpace(requestedSmsNumber))
            {
                smsVerification = userSmsService.GenerateMessagingVerification(hdid, requestedSmsNumber);
                await messageVerificationDelegate.InsertAsync(smsVerification, false, ct);
            }

            return smsVerification;
        }

        private async Task<MessagingVerification?> AddEmailVerification(string hdid, string? requestedEmail, bool isEmailVerified, CancellationToken ct)
        {
            MessagingVerification? emailVerification = null;
            if (!string.IsNullOrWhiteSpace(requestedEmail))
            {
                emailVerification = await userEmailService.GenerateMessagingVerificationAsync(hdid, requestedEmail, Guid.NewGuid(), isEmailVerified, ct);
                await messageVerificationDelegate.InsertAsync(emailVerification, false, ct);
            }

            return emailVerification;
        }

        private async Task NotifyAccountCreated(UserProfile profile, string requestedEmail, string requestedSmsNumber, bool isEmailVerified, string smsVerificationCode, CancellationToken ct)
        {
            // notify account was created
            if (this.accountsChangeFeedEnabled)
            {
                await messageSender.SendAsync([new MessageEnvelope(new AccountCreatedEvent(profile.HdId, DateTime.UtcNow), profile.HdId)], ct);
            }

            // notify email verification was successful
            if (isEmailVerified && this.notificationsChangeFeedEnabled)
            {
                await messageSender.SendAsync([new(new NotificationChannelVerifiedEvent(profile.HdId, NotificationChannel.Email, requestedEmail), profile.HdId)], ct);
            }

            // queue notification settings job
            NotificationSettingsRequest notificationSettingsRequest = new(profile, profile.Email, requestedSmsNumber) { SmsVerificationCode = smsVerificationCode };
            await notificationSettingsService.QueueNotificationSettingsAsync(notificationSettingsRequest, ct);
        }
    }
}
