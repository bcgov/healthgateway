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
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation.Results;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
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
    using PatientModel = HealthGateway.Common.Models.PatientModel;

    /// <inheritdoc/>
    public class UserProfileService : IUserProfileService
    {
        private const string WebClientConfigSection = "WebClient";
        private const string UserProfileHistoryRecordLimitKey = "UserProfileHistoryRecordLimit";
        private const string MinPatientAgeKey = "MinPatientAge";
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IApplicationSettingsDelegate applicationSettingsDelegate;
        private readonly ICacheProvider cacheProvider;
        private readonly IGatewayApiMappingService mappingService;
        private readonly ICryptoDelegate cryptoDelegate;
        private readonly IEmailQueueService emailQueueService;
        private readonly ILegalAgreementDelegate legalAgreementDelegate;
        private readonly ILogger logger;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly int minPatientAge;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IPatientService patientService;
        private readonly IUserEmailService userEmailService;
        private readonly IUserPreferenceDelegate userPreferenceDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly int userProfileHistoryRecordLimit;
        private readonly IUserSmsService userSmsService;
        private readonly IPatientRepository patientRepository;
        private readonly bool accountsChangeFeedEnabled;
        private readonly bool notificationsChangeFeedEnabled;
        private readonly IMessageSender messageSender;
        private readonly EmailTemplateConfig emailTemplateConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="patientService">The patient service.</param>
        /// <param name="userEmailService">The User Email service.</param>
        /// <param name="userSmsService">The User SMS service.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="notificationSettingsService">The Notifications Settings service.</param>
        /// <param name="userProfileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="userPreferenceDelegate">The preference delegate to interact with the DB.</param>
        /// <param name="legalAgreementDelegate">The terms of service delegate.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="mappingService">The inject automapper provider.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        /// <param name="applicationSettingsDelegate">The injected Application Settings delegate.</param>
        /// <param name="cacheProvider">The injected cache provider.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="messageSender">The message sender.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public UserProfileService(
            ILogger<UserProfileService> logger,
            IPatientService patientService,
            IUserEmailService userEmailService,
            IUserSmsService userSmsService,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService,
            IUserProfileDelegate userProfileDelegate,
            IUserPreferenceDelegate userPreferenceDelegate,
            ILegalAgreementDelegate legalAgreementDelegate,
            IMessagingVerificationDelegate messageVerificationDelegate,
            ICryptoDelegate cryptoDelegate,
            IConfiguration configuration,
            IGatewayApiMappingService mappingService,
            IAuthenticationDelegate authenticationDelegate,
            IApplicationSettingsDelegate applicationSettingsDelegate,
            ICacheProvider cacheProvider,
            IPatientRepository patientRepository,
            IMessageSender messageSender)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.userEmailService = userEmailService;
            this.userSmsService = userSmsService;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
            this.userProfileDelegate = userProfileDelegate;
            this.userPreferenceDelegate = userPreferenceDelegate;
            this.legalAgreementDelegate = legalAgreementDelegate;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.cryptoDelegate = cryptoDelegate;
            this.userProfileHistoryRecordLimit = configuration.GetSection(WebClientConfigSection)
                .GetValue(UserProfileHistoryRecordLimitKey, 4);
            this.minPatientAge = configuration.GetSection(WebClientConfigSection).GetValue(MinPatientAgeKey, 12);
            this.mappingService = mappingService;
            this.authenticationDelegate = authenticationDelegate;
            this.applicationSettingsDelegate = applicationSettingsDelegate;
            this.cacheProvider = cacheProvider;
            this.patientRepository = patientRepository;
            this.messageSender = messageSender;
            ChangeFeedOptions? changeFeedConfiguration = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>();
            this.accountsChangeFeedEnabled = changeFeedConfiguration?.Accounts.Enabled ?? false;
            this.notificationsChangeFeedEnabled = changeFeedConfiguration?.Notifications.Enabled ?? false;
            this.emailTemplateConfig = configuration.GetSection(EmailTemplateConfig.ConfigurationSectionKey).Get<EmailTemplateConfig>() ?? new();
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> GetUserProfileAsync(string hdid, DateTime jwtAuthTime, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting user profile... {Hdid}", hdid);
            UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct);
            this.logger.LogDebug("Finished getting user profile...{Hdid}", hdid);

            if (userProfile == null)
            {
                return new RequestResult<UserProfileModel>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new UserProfileModel(),
                };
            }

            DateTime previousLastLogin = userProfile.LastLoginDateTime;
            if (DateTime.Compare(previousLastLogin, jwtAuthTime) != 0)
            {
                this.logger.LogTrace("Updating user last login and year of birth... {Hdid}", hdid);
                userProfile.LastLoginDateTime = jwtAuthTime;
                userProfile.LastLoginClientCode = this.authenticationDelegate.FetchAuthenticatedUserClientType();

                // Update user year of birth.
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(hdid, ct: ct);
                DateTime? birthDate = patientResult.ResourcePayload?.Birthdate;
                userProfile.YearOfBirth = birthDate?.Year;

                // Try to update user profile with last login time; ignore any failures
                await this.userProfileDelegate.UpdateAsync(userProfile, ct: ct);

                this.logger.LogDebug("Finished updating user last login and year of birth... {Hdid}", hdid);
            }

            IList<UserProfileHistory> userProfileHistoryList =
                await this.userProfileDelegate.GetUserProfileHistoryListAsync(hdid, this.userProfileHistoryRecordLimit, ct);
            UserProfileModel userProfileModel = await this.BuildUserProfileModelAsync(userProfile, [.. userProfileHistoryList], ct);

            // Populate most recent login date time
            userProfileModel.LastLoginDateTimes.Add(userProfile.LastLoginDateTime);
            foreach (UserProfileHistory userProfileHistory in userProfileHistoryList)
            {
                userProfileModel.LastLoginDateTimes.Add(userProfileHistory.LastLoginDateTime);
            }

            if (!userProfileModel.IsEmailVerified)
            {
                this.logger.LogTrace("Retrieving last email invite... {Hdid}", hdid);
                MessagingVerification? emailInvite =
                    await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
                this.logger.LogDebug("Finished retrieving email invite... {Hdid}", hdid);
                userProfileModel.Email = emailInvite?.Email?.To;
            }

            if (!userProfileModel.IsSmsNumberVerified)
            {
                this.logger.LogTrace("Retrieving last sms invite... {Hdid}", hdid);
                MessagingVerification? smsInvite =
                    await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);
                this.logger.LogDebug("Finished retrieving sms invite... {Hdid}", hdid);
                userProfileModel.SmsNumber = smsInvite?.SmsNumber;
            }

            return new RequestResult<UserProfileModel>
            {
                ResultStatus = ResultType.Success,
                ResultError = null,
                ResourcePayload = userProfileModel,
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> CreateUserProfileAsync(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string? jwtEmailAddress, CancellationToken ct = default)
        {
            this.logger.LogTrace("Creating user profile... {Hdid}", createProfileRequest.Profile.HdId);

            RequestResult<UserProfileModel>? validationResult = await this.ValidateUserProfileAsync(createProfileRequest, ct);
            if (validationResult != null)
            {
                return validationResult;
            }

            string hdid = createProfileRequest.Profile.HdId;

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(hdid, ct: ct);
            DateTime? birthDate = patientResult.ResourcePayload?.Birthdate;

            // Create profile
            UserProfile newProfile = new()
            {
                HdId = hdid,
                IdentityManagementId = createProfileRequest.Profile.IdentityManagementId,
                TermsOfServiceId = createProfileRequest.Profile.TermsOfServiceId,
                Email = string.Empty,
                SmsNumber = null,
                CreatedBy = hdid,
                UpdatedBy = hdid,
                LastLoginDateTime = jwtAuthTime,
                EncryptionKey = this.cryptoDelegate.GenerateKey(),
                YearOfBirth = birthDate?.Year,
                LastLoginClientCode = this.authenticationDelegate.FetchAuthenticatedUserClientType(),
            };
            DbResult<UserProfile> insertResult = await this.userProfileDelegate.InsertUserProfileAsync(newProfile, !this.accountsChangeFeedEnabled, ct);

            if (this.accountsChangeFeedEnabled)
            {
                await this.messageSender.SendAsync(
                    new[]
                    {
                        new MessageEnvelope(new AccountCreatedEvent(hdid, DateTime.UtcNow), hdid),
                    },
                    ct);
            }

            if (insertResult.Status != DbStatusCode.Created)
            {
                this.logger.LogError("Error creating user profile... {Hdid}", insertResult.Payload.HdId);
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, insertResult.Message);
            }

            UserProfile dbModel = insertResult.Payload;
            string? requestedSmsNumber = createProfileRequest.Profile.SmsNumber;
            string? requestedEmail = createProfileRequest.Profile.Email;

            UserProfileModel userProfileModel = await this.BuildUserProfileModelAsync(dbModel, ct: ct);

            NotificationSettingsRequest notificationRequest = new(dbModel, requestedEmail, requestedSmsNumber);

            // Add email verification
            if (!string.IsNullOrWhiteSpace(requestedEmail))
            {
                bool isEmailVerified = requestedEmail.Equals(jwtEmailAddress, StringComparison.OrdinalIgnoreCase);
                await this.userEmailService.CreateUserEmailAsync(hdid, requestedEmail, isEmailVerified, !this.notificationsChangeFeedEnabled, ct);
                userProfileModel.Email = requestedEmail;
                userProfileModel.IsEmailVerified = isEmailVerified;
                if (isEmailVerified && this.notificationsChangeFeedEnabled)
                {
                    MessageEnvelope[] events =
                    {
                        new(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Email, requestedEmail), hdid),
                    };
                    await this.messageSender.SendAsync(events, ct);
                }
            }

            // Add SMS verification
            if (!string.IsNullOrWhiteSpace(requestedSmsNumber))
            {
                MessagingVerification smsVerification = await this.userSmsService.CreateUserSmsAsync(hdid, requestedSmsNumber, ct);
                notificationRequest.SmsVerificationCode = smsVerification.SmsValidationCode;
                userProfileModel.SmsNumber = requestedSmsNumber;
            }

            await this.notificationSettingsService.QueueNotificationSettingsAsync(notificationRequest, ct);

            this.logger.LogDebug("Finished creating user profile... {Hdid}", insertResult.Payload.HdId);

            return RequestResultFactory.Success(userProfileModel);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> CloseUserProfileAsync(string hdid, Guid userId, CancellationToken ct = default)
        {
            this.logger.LogTrace("Closing user profile... {Hdid}", hdid);

            UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct);

            if (userProfile == null)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, ErrorMessages.UserProfileNotFound);
            }

            if (userProfile.ClosedDateTime != null)
            {
                this.logger.LogTrace("Finished. Profile already Closed");
                return RequestResultFactory.Success(await this.BuildUserProfileModelAsync(userProfile, ct: ct));
            }

            userProfile.ClosedDateTime = DateTime.UtcNow;
            userProfile.IdentityManagementId = userId;
            DbResult<UserProfile> updateResult = await this.userProfileDelegate.UpdateAsync(userProfile, ct: ct);
            return await this.HandleUpdateUserProfileResultAsync(updateResult, EmailTemplateName.AccountClosedTemplate, ct);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> RecoverUserProfileAsync(string hdid, CancellationToken ct = default)
        {
            this.logger.LogTrace("Recovering user profile... {Hdid}", hdid);

            UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct);

            if (userProfile == null)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, ErrorMessages.UserProfileNotFound);
            }

            if (userProfile.ClosedDateTime == null)
            {
                this.logger.LogTrace("Finished. Profile already is active, recover not needed");
                return RequestResultFactory.Success(await this.BuildUserProfileModelAsync(userProfile, ct: ct));
            }

            // Remove values set for deletion
            userProfile.ClosedDateTime = null;
            userProfile.IdentityManagementId = null;
            DbResult<UserProfile> updateResult = await this.userProfileDelegate.UpdateAsync(userProfile, true, ct);
            await this.HandleUpdateUserProfileResultAsync(updateResult, EmailTemplateName.AccountRecoveredTemplate, ct);
            return RequestResultFactory.Success(await this.BuildUserProfileModelAsync(updateResult.Payload, ct: ct));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<TermsOfServiceModel>> GetActiveTermsOfServiceAsync(CancellationToken ct = default)
        {
            LegalAgreement? legalAgreement = await this.legalAgreementDelegate.GetActiveByAgreementTypeAsync(LegalAgreementType.TermsOfService, ct);
            return legalAgreement == null
                ? RequestResultFactory.ServiceError<TermsOfServiceModel>(ErrorType.CommunicationInternal, ServiceType.Database, ErrorMessages.LegalAgreementNotFound)
                : RequestResultFactory.Success(this.mappingService.MapToTermsOfServiceModel(legalAgreement));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserPreferenceModel>> UpdateUserPreferenceAsync(UserPreferenceModel userPreferenceModel, CancellationToken ct = default)
        {
            this.logger.LogTrace("Updating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);

            UserPreference userPreference = this.mappingService.MapToUserPreference(userPreferenceModel);

            DbResult<UserPreference> dbResult = await this.userPreferenceDelegate.UpdateUserPreferenceAsync(userPreference, ct: ct);

            if (dbResult.Status != DbStatusCode.Updated)
            {
                return RequestResultFactory.ServiceError<UserPreferenceModel>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.mappingService.MapToUserPreferenceModel(dbResult.Payload));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserPreferenceModel>> CreateUserPreferenceAsync(UserPreferenceModel userPreferenceModel, CancellationToken ct = default)
        {
            this.logger.LogTrace("Creating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);
            UserPreference userPreference = this.mappingService.MapToUserPreference(userPreferenceModel);
            DbResult<UserPreference> dbResult = await this.userPreferenceDelegate.CreateUserPreferenceAsync(userPreference, ct: ct);

            if (dbResult.Status != DbStatusCode.Created)
            {
                return RequestResultFactory.ServiceError<UserPreferenceModel>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.mappingService.MapToUserPreferenceModel(dbResult.Payload));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<bool>> ValidateMinimumAgeAsync(string hdid, CancellationToken ct = default)
        {
            if (this.minPatientAge == 0)
            {
                return RequestResultFactory.Success(true);
            }

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(hdid, ct: ct);

            if (patientResult.ResultStatus != ResultType.Success || patientResult.ResourcePayload == null)
            {
                this.logger.LogWarning("Error retrieving patient age... {Hdid}", hdid);
                return RequestResultFactory.Error(false, patientResult.ResultError);
            }

            bool isValid = AgeRangeValidator.IsValid(patientResult.ResourcePayload.Birthdate, this.minPatientAge);

            return RequestResultFactory.Success(isValid);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> UpdateAcceptedTermsAsync(string hdid, Guid termsOfServiceId, CancellationToken ct = default)
        {
            UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct);

            if (userProfile == null)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, "Unable to retrieve user profile");
            }

            userProfile.TermsOfServiceId = termsOfServiceId;
            DbResult<UserProfile> result = await this.userProfileDelegate.UpdateAsync(userProfile, ct: ct);
            if (result.Status != DbStatusCode.Updated)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, "Unable to update the terms of service: DB Error");
            }

            UserProfileModel userProfileModel = await this.BuildUserProfileModelAsync(result.Payload, ct: ct);

            return RequestResultFactory.Success(userProfileModel);
        }

        /// <inheritdoc/>
        public async Task<bool> IsPhoneNumberValidAsync(string phoneNumber, CancellationToken ct = default)
        {
            return await UserProfileValidator.ValidateUserProfileSmsNumberAsync(phoneNumber, ct);
        }

        private async Task QueueEmailAsync(string toEmail, string templateName, CancellationToken ct)
        {
            Dictionary<string, string> keyValues = new() { [EmailTemplateVariable.Host] = this.emailTemplateConfig.Host };
            await this.emailQueueService.QueueNewEmailAsync(toEmail, templateName, keyValues, ct: ct);
        }

        private async Task<RequestResult<UserProfileModel>?> ValidateUserProfileAsync(CreateUserRequest createProfileRequest, CancellationToken ct)
        {
            // Validate registration age
            string hdid = createProfileRequest.Profile.HdId;
            RequestResult<bool> isMinimumAgeResult = await this.ValidateMinimumAgeAsync(hdid, ct);
            if (isMinimumAgeResult.ResultStatus != ResultType.Success)
            {
                return RequestResultFactory.Error<UserProfileModel>(isMinimumAgeResult.ResultError);
            }

            if (!isMinimumAgeResult.ResourcePayload)
            {
                this.logger.LogWarning("Patient under minimum age... {Hdid}", createProfileRequest.Profile.HdId);
                return RequestResultFactory.Error<UserProfileModel>(ErrorType.InvalidState, "Patient under minimum age");
            }

            // Validate UserProfile inputs
            ValidationResult profileValidationResult = await new UserProfileValidator().ValidateAsync(createProfileRequest.Profile, ct);
            if (!profileValidationResult.IsValid)
            {
                this.logger.LogWarning("Profile inputs have failed validation for {Hdid}", createProfileRequest.Profile.HdId);
                return RequestResultFactory.Error<UserProfileModel>(ErrorType.SmsInvalid, "Profile values entered are invalid");
            }

            return null;
        }

        private async Task<UserProfileModel> BuildUserProfileModelAsync(UserProfile userProfile, UserProfileHistory[]? profileHistoryCollection = null, CancellationToken ct = default)
        {
            Guid? termsOfServiceId = (await this.legalAgreementDelegate.GetActiveByAgreementTypeAsync(LegalAgreementType.TermsOfService, ct))?.Id;
            UserProfileModel userProfileModel = this.mappingService.MapToUserProfileModel(userProfile, termsOfServiceId);

            DateTime? latestTourChangeDateTime = await this.GetLatestTourChangeDateTimeAsync(ct);
            userProfileModel.HasTourUpdated = profileHistoryCollection != null &&
                                              profileHistoryCollection.Length != 0 &&
                                              latestTourChangeDateTime != null &&
                                              profileHistoryCollection.Max(x => x.LastLoginDateTime) < latestTourChangeDateTime;

            userProfileModel.BlockedDataSources = await this.patientRepository.GetDataSourcesAsync(userProfile.HdId, ct);
            userProfileModel.Preferences = await this.GetUserPreferencesAsync(userProfileModel.HdId, ct);

            return userProfileModel;
        }

        private async Task<DateTime?> GetLatestTourChangeDateTimeAsync(CancellationToken ct)
        {
            return await this.cacheProvider.GetOrSetAsync(
                $"{TourApplicationSettings.Application}:{TourApplicationSettings.Component}:{TourApplicationSettings.LatestChangeDateTime}",
                async () =>
                {
                    ApplicationSetting? applicationSetting = await this.applicationSettingsDelegate.GetApplicationSettingAsync(
                        TourApplicationSettings.Application,
                        TourApplicationSettings.Component,
                        TourApplicationSettings.LatestChangeDateTime,
                        ct);

                    return applicationSetting?.Value != null
                        ? DateTime.Parse(applicationSetting.Value, CultureInfo.InvariantCulture).ToUniversalTime()
                        : (DateTime?)null;
                },
                TimeSpan.FromMinutes(30),
                ct);
        }

        private async Task<Dictionary<string, UserPreferenceModel>> GetUserPreferencesAsync(string hdid, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting user preference... {Hdid}", hdid);
            IEnumerable<UserPreference> userPreferences = await this.userPreferenceDelegate.GetUserPreferencesAsync(hdid, ct);
            return userPreferences.Select(this.mappingService.MapToUserPreferenceModel).ToDictionary(x => x.Preference, x => x);
        }

        private async Task<RequestResult<UserProfileModel>> HandleUpdateUserProfileResultAsync(DbResult<UserProfile> result, string emailTemplateName, CancellationToken ct)
        {
            if (result.Status == DbStatusCode.Updated)
            {
                if (!string.IsNullOrWhiteSpace(result.Payload.Email))
                {
                    await this.QueueEmailAsync(result.Payload.Email, emailTemplateName, ct);
                }

                return RequestResultFactory.Success(await this.BuildUserProfileModelAsync(result.Payload, ct: ct));
            }

            return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, result.Message);
        }
    }
}
