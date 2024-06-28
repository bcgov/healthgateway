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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
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
    public class UserProfileServiceV2 : IUserProfileServiceV2
    {
        private const string MinPatientAgeKey = "MinPatientAge";
        private const string UserProfileHistoryRecordLimitKey = "UserProfileHistoryRecordLimit";
        private const string WebClientConfigSection = "WebClient";
        private readonly EmailTemplateConfig emailTemplateConfig;

        private readonly bool accountsChangeFeedEnabled;
        private readonly bool notificationsChangeFeedEnabled;
        private readonly int minPatientAge;
        private readonly int userProfileHistoryRecordLimit;

        private readonly ILogger<UserProfileServiceV2> logger;
        private readonly IPatientDetailsService patientDetailsService;
        private readonly IUserEmailServiceV2 userEmailService;
        private readonly IUserSmsServiceV2 userSmsService;
        private readonly IEmailQueueService emailQueueService;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IUserPreferenceServiceV2 userPreferenceService;
        private readonly ILegalAgreementServiceV2 legalAgreementService;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly ICryptoDelegate cryptoDelegate;
        private readonly IGatewayApiMappingService mappingService;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IApplicationSettingsService applicationSettingsService;
        private readonly IPatientRepository patientRepository;
        private readonly IMessageSender messageSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileServiceV2"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="patientDetailsService">The injected patient details service.</param>
        /// <param name="userEmailService">The injected user email service.</param>
        /// <param name="userSmsService">The injected user SMS service.</param>
        /// <param name="emailQueueService">The injected service to queue emails.</param>
        /// <param name="notificationSettingsService">The injected notifications settings service.</param>
        /// <param name="userProfileDelegate">The injected user profile database delegate.</param>
        /// <param name="userPreferenceService">The injected user preference service.</param>
        /// <param name="legalAgreementService">The injected legal agreement service.</param>
        /// <param name="messageVerificationDelegate">The injected message verification delegate.</param>
        /// <param name="cryptoDelegate">The injected crypto delegate.</param>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        /// <param name="applicationSettingsService">The injected application settings service.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="messageSender">The injected message sender.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public UserProfileServiceV2(
            ILogger<UserProfileServiceV2> logger,
            IPatientDetailsService patientDetailsService,
            IUserEmailServiceV2 userEmailService,
            IUserSmsServiceV2 userSmsService,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService,
            IUserProfileDelegate userProfileDelegate,
            IUserPreferenceServiceV2 userPreferenceService,
            ILegalAgreementServiceV2 legalAgreementService,
            IMessagingVerificationDelegate messageVerificationDelegate,
            ICryptoDelegate cryptoDelegate,
            IConfiguration configuration,
            IGatewayApiMappingService mappingService,
            IAuthenticationDelegate authenticationDelegate,
            IApplicationSettingsService applicationSettingsService,
            IPatientRepository patientRepository,
            IMessageSender messageSender)
        {
            this.logger = logger;
            this.patientDetailsService = patientDetailsService;
            this.userEmailService = userEmailService;
            this.userSmsService = userSmsService;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
            this.userProfileDelegate = userProfileDelegate;
            this.userPreferenceService = userPreferenceService;
            this.legalAgreementService = legalAgreementService;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.cryptoDelegate = cryptoDelegate;
            this.mappingService = mappingService;
            this.authenticationDelegate = authenticationDelegate;
            this.applicationSettingsService = applicationSettingsService;
            this.patientRepository = patientRepository;
            this.messageSender = messageSender;
            this.emailTemplateConfig = configuration.GetSection(EmailTemplateConfig.ConfigurationSectionKey).Get<EmailTemplateConfig>() ?? new();
            this.accountsChangeFeedEnabled = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>()?.Accounts.Enabled ?? false;
            this.notificationsChangeFeedEnabled = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>()?.Notifications.Enabled ?? false;
            this.minPatientAge = configuration.GetSection(WebClientConfigSection).GetValue(MinPatientAgeKey, 12);
            this.userProfileHistoryRecordLimit = configuration.GetSection(WebClientConfigSection).GetValue(UserProfileHistoryRecordLimitKey, 4);
        }

        /// <inheritdoc/>
        public async Task<UserProfileModel> GetUserProfileAsync(string hdid, DateTime jwtAuthTime, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting user profile... {Hdid}", hdid);
            UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, true, ct);
            this.logger.LogDebug("Finished getting user profile...{Hdid}", hdid);

            if (userProfile == null)
            {
                return new UserProfileModel();
            }

            DateTime previousLastLogin = userProfile.LastLoginDateTime;
            if (DateTime.Compare(previousLastLogin, jwtAuthTime) != 0)
            {
                this.logger.LogTrace("Updating user last login and year of birth... {Hdid}", hdid);
                userProfile.LastLoginDateTime = jwtAuthTime;
                userProfile.LastLoginClientCode = this.authenticationDelegate.FetchAuthenticatedUserClientType();

                // Update user year of birth.
                PatientDetails patient = await this.patientDetailsService.GetPatientAsync(hdid, ct: ct);
                userProfile.YearOfBirth = patient.Birthdate.Year;

                // Try to update user profile with last login time; ignore any failures
                await this.userProfileDelegate.UpdateAsync(userProfile, ct: ct);

                this.logger.LogDebug("Finished updating user last login and year of birth... {Hdid}", hdid);
            }

            IList<UserProfileHistory> userProfileHistoryList = await this.userProfileDelegate.GetUserProfileHistoryListAsync(hdid, this.userProfileHistoryRecordLimit, ct);
            UserProfileModel userProfileModel = await this.BuildUserProfileModelAsync(userProfile, userProfileHistoryList, ct);

            if (!userProfileModel.IsEmailVerified)
            {
                this.logger.LogTrace("Retrieving last email invite... {Hdid}", hdid);
                MessagingVerification? emailInvite = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
                this.logger.LogDebug("Finished retrieving email invite... {Hdid}", hdid);
                userProfileModel.Email = emailInvite?.Email?.To;
            }

            if (!userProfileModel.IsSmsNumberVerified)
            {
                this.logger.LogTrace("Retrieving last sms invite... {Hdid}", hdid);
                MessagingVerification? smsInvite = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);
                this.logger.LogDebug("Finished retrieving sms invite... {Hdid}", hdid);
                userProfileModel.SmsNumber = smsInvite?.SmsNumber;
            }

            return userProfileModel;
        }

        /// <inheritdoc/>
        public async Task<UserProfileModel> CreateUserProfileAsync(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string? jwtEmailAddress, CancellationToken ct = default)
        {
            string hdid = createProfileRequest.Profile.HdId;
            this.logger.LogTrace("Creating user profile... {Hdid}", hdid);

            await new SmsNumberValidator().ValidateAndThrowAsync(createProfileRequest.Profile.SmsNumber, ct);

            PatientDetails patient = await this.patientDetailsService.GetPatientAsync(hdid, ct: ct);
            if (this.minPatientAge > 0)
            {
                await new AgeRangeValidator(this.minPatientAge).ValidateAndThrowAsync(patient.Birthdate.ToDateTime(TimeOnly.MinValue), ct);
            }

            UserProfile profile = new()
            {
                HdId = hdid,
                IdentityManagementId = null,
                TermsOfServiceId = createProfileRequest.Profile.TermsOfServiceId,
                Email = string.Empty,
                SmsNumber = null,
                CreatedBy = hdid,
                UpdatedBy = hdid,
                LastLoginDateTime = jwtAuthTime,
                EncryptionKey = this.cryptoDelegate.GenerateKey(),
                YearOfBirth = patient.Birthdate.Year,
                LastLoginClientCode = this.authenticationDelegate.FetchAuthenticatedUserClientType(),
            };

            DbResult<UserProfile> dbResult = await this.userProfileDelegate.InsertUserProfileAsync(profile, ct: ct);
            if (dbResult.Status != DbStatusCode.Created)
            {
                this.logger.LogError("Error creating user profile... {Hdid}", hdid);
                throw new DatabaseException(dbResult.Message);
            }

            UserProfileModel userProfileModel = await this.BuildUserProfileModelAsync(dbResult.Payload, [], ct);

            string? requestedEmail = createProfileRequest.Profile.Email;
            bool isEmailVerified = !string.IsNullOrWhiteSpace(requestedEmail) && requestedEmail.Equals(jwtEmailAddress, StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(requestedEmail))
            {
                userProfileModel.Email = requestedEmail;
                userProfileModel.IsEmailVerified = isEmailVerified;

                // Add email verification
                await this.userEmailService.CreateUserEmailAsync(hdid, requestedEmail, isEmailVerified, !this.notificationsChangeFeedEnabled, ct);
            }

            string? requestedSmsNumber = createProfileRequest.Profile.SmsNumber;
            NotificationSettingsRequest notificationRequest = new(dbResult.Payload, requestedEmail, requestedSmsNumber);
            if (!string.IsNullOrWhiteSpace(requestedSmsNumber))
            {
                userProfileModel.SmsNumber = requestedSmsNumber;

                // Add SMS verification
                MessagingVerification smsVerification = await this.userSmsService.CreateUserSmsAsync(hdid, requestedSmsNumber, ct);
                notificationRequest.SmsVerificationCode = smsVerification.SmsValidationCode;
            }

            await this.NotifyAccountCreated(hdid, notificationRequest, isEmailVerified, ct);

            this.logger.LogDebug("Finished creating user profile... {Hdid}", dbResult.Payload.HdId);

            return userProfileModel;
        }

        /// <inheritdoc/>
        public async Task CloseUserProfileAsync(string hdid, CancellationToken ct = default)
        {
            this.logger.LogTrace("Closing user profile... {Hdid}", hdid);

            UserProfile userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            if (userProfile.ClosedDateTime != null)
            {
                this.logger.LogTrace("Profile already closed");
                return;
            }

            // Mark profile for deletion
            userProfile.ClosedDateTime = DateTime.UtcNow;
            userProfile.IdentityManagementId = new(this.authenticationDelegate.FetchAuthenticatedUserId());
            DbResult<UserProfile> dbResult = await this.userProfileDelegate.UpdateAsync(userProfile, ct: ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }

            await this.SendEmailAsync(dbResult.Payload.Email, EmailTemplateName.AccountClosedTemplate, ct);
        }

        /// <inheritdoc/>
        public async Task RecoverUserProfileAsync(string hdid, CancellationToken ct = default)
        {
            this.logger.LogTrace("Recovering user profile... {Hdid}", hdid);
            UserProfile userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            if (userProfile.ClosedDateTime == null)
            {
                this.logger.LogTrace("Profile already is active, recover not needed");
                return;
            }

            // Unmark profile for deletion
            userProfile.ClosedDateTime = null;
            userProfile.IdentityManagementId = null;
            DbResult<UserProfile> dbResult = await this.userProfileDelegate.UpdateAsync(userProfile, true, ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }

            await this.SendEmailAsync(dbResult.Payload.Email, EmailTemplateName.AccountRecoveredTemplate, ct);
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateEligibilityAsync(string hdid, CancellationToken ct = default)
        {
            if (this.minPatientAge == 0)
            {
                return true;
            }

            PatientDetails patient = await this.patientDetailsService.GetPatientAsync(hdid, ct: ct);
            return (await new AgeRangeValidator(this.minPatientAge).ValidateAsync(patient.Birthdate.ToDateTime(TimeOnly.MinValue), ct)).IsValid;
        }

        /// <inheritdoc/>
        public async Task UpdateAcceptedTermsAsync(string hdid, Guid termsOfServiceId, CancellationToken ct = default)
        {
            UserProfile userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);
            userProfile.TermsOfServiceId = termsOfServiceId;

            DbResult<UserProfile> result = await this.userProfileDelegate.UpdateAsync(userProfile, ct: ct);
            if (result.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(result.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsPhoneNumberValidAsync(string phoneNumber, CancellationToken ct = default)
        {
            return (await new SmsNumberValidator().ValidateAsync(phoneNumber, ct)).IsValid;
        }

        private async Task<UserProfileModel> BuildUserProfileModelAsync(UserProfile userProfile, ICollection<UserProfileHistory> historyCollection, CancellationToken ct = default)
        {
            Guid? termsOfServiceId = await this.legalAgreementService.GetActiveLegalAgreementId(LegalAgreementType.TermsOfService, ct);
            UserProfileModel userProfileModel = this.mappingService.MapToUserProfileModel(userProfile, termsOfServiceId);

            DateTime? latestTourChangeDateTime = await this.applicationSettingsService.GetLatestTourChangeDateTimeAsync(ct);
            userProfileModel.HasTourUpdated = historyCollection.Count != 0 &&
                                              latestTourChangeDateTime != null &&
                                              historyCollection.Max(x => x.LastLoginDateTime) < latestTourChangeDateTime;

            userProfileModel.BlockedDataSources = await this.patientRepository.GetDataSourcesAsync(userProfile.HdId, ct);
            userProfileModel.Preferences = await this.userPreferenceService.GetUserPreferencesAsync(userProfileModel.HdId, ct);
            userProfileModel.LastLoginDateTimes = [userProfile.LastLoginDateTime, ..historyCollection.Select(h => h.LastLoginDateTime)];

            return userProfileModel;
        }

        private async Task NotifyAccountCreated(string hdid, NotificationSettingsRequest notificationRequest, bool isEmailVerified, CancellationToken ct)
        {
            if (this.accountsChangeFeedEnabled)
            {
                await this.messageSender.SendAsync([new MessageEnvelope(new AccountCreatedEvent(hdid, DateTime.UtcNow), hdid)], ct);
            }

            if (isEmailVerified && this.notificationsChangeFeedEnabled)
            {
                await this.messageSender.SendAsync([new(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Email, notificationRequest.EmailAddress), hdid)], ct);
            }

            await this.notificationSettingsService.QueueNotificationSettingsAsync(notificationRequest, ct);
        }

        private async Task SendEmailAsync(string? emailAddress, string emailTemplateName, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                Dictionary<string, string> keyValues = new() { [EmailTemplateVariable.Host] = this.emailTemplateConfig.Host };
                await this.emailQueueService.QueueNewEmailAsync(emailAddress, emailTemplateName, keyValues, ct: ct);
            }
        }
    }
}
