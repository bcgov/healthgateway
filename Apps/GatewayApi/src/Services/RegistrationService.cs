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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="logger">The injected logger.</param>
    /// <param name="authenticationDelegate">The injected authentication delegate.</param>
    /// <param name="cryptoDelegate">The injected crypto delegate.</param>
    /// <param name="messagingVerificationService">The injected message verification service.</param>
    /// <param name="jobService">The injected job service.</param>
    /// <param name="patientDetailsService">The injected patient details service.</param>
    /// <param name="userProfileDelegate">The injected user profile database delegate.</param>
    /// <param name="userProfileModelService">The injected user profile model service.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
    public class RegistrationService(
        IConfiguration configuration,
        ILogger<RegistrationService> logger,
        IAuthenticationDelegate authenticationDelegate,
        ICryptoDelegate cryptoDelegate,
        IMessagingVerificationService messagingVerificationService,
        IJobService jobService,
        IPatientDetailsService patientDetailsService,
        IUserProfileDelegate userProfileDelegate,
        IUserProfileModelService userProfileModelService) : IRegistrationService
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
                await jobService.SendEmailAsync(emailVerification.Email, true, ct);
            }

            // notify partners about new account
            await this.NotifyAccountCreated(profile, requestedEmail, requestedSmsNumber, isEmailVerified, smsVerification?.SmsValidationCode, ct);

            // build and return model
            UserProfileModel userProfileModel = await userProfileModelService.BuildUserProfileModelAsync(profile, this.userProfileHistoryRecordLimit, ct);
            logger.LogTrace("Finished creating user profile... {Hdid}", dbResult.Payload.HdId);
            return userProfileModel;
        }

        private async Task<MessagingVerification?> AddSmsVerification(string hdid, string? requestedSmsNumber, CancellationToken ct)
        {
            MessagingVerification? smsVerification = null;
            if (!string.IsNullOrWhiteSpace(requestedSmsNumber))
            {
                // Generate amd add SMS messaging verification to DB without committing changes
                smsVerification = await messagingVerificationService.AddSmsVerificationAsync(hdid, requestedSmsNumber, false, ct);
            }

            return smsVerification;
        }

        private async Task<MessagingVerification?> AddEmailVerification(string hdid, string? requestedEmail, bool isEmailVerified, CancellationToken ct)
        {
            MessagingVerification? emailVerification = null;
            if (!string.IsNullOrWhiteSpace(requestedEmail))
            {
                // Generate amd add email messaging verification to DB without committing changes
                emailVerification = await messagingVerificationService.AddEmailVerificationAsync(hdid, requestedEmail, isEmailVerified, false, ct);
            }

            return emailVerification;
        }

        private async Task NotifyAccountCreated(UserProfile profile, string requestedEmail, string requestedSmsNumber, bool isEmailVerified, string smsVerificationCode, CancellationToken ct)
        {
            // notify account was created
            if (this.accountsChangeFeedEnabled)
            {
                await jobService.NotifyAccountCreationAsync(profile.HdId, ct);
            }

            // notify email verification was successful
            if (isEmailVerified && this.notificationsChangeFeedEnabled)
            {
                await jobService.NotifyEmailVerificationAsync(profile.HdId, requestedEmail, ct);
            }

            // queue notification settings job
            await jobService.PushNotificationSettingsToPhsaAsync(profile, profile.Email, requestedSmsNumber, smsVerificationCode, ct);
        }
    }
}
