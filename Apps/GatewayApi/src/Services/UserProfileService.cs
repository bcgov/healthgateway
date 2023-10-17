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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentValidation.Results;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Data.ViewModels;
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
    using HealthGateway.GatewayApi.MapUtils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.AspNetCore.Http;
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
        private readonly IMapper autoMapper;
        private readonly ICryptoDelegate cryptoDelegate;
        private readonly IEmailQueueService emailQueueService;
        private readonly IHttpContextAccessor httpContextAccessor;
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
        private readonly bool changeFeedEnabled;
        private readonly IMessageSender messageSender;

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
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="autoMapper">The inject automapper provider.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        /// <param name="applicationSettingsDelegate">The injected Application Settings delegate.</param>
        /// <param name="cacheProvider">The injected cache provider.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="messageSender">The message sender.</param>
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
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IMapper autoMapper,
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
            this.httpContextAccessor = httpContextAccessor;
            this.userProfileHistoryRecordLimit = configuration.GetSection(WebClientConfigSection)
                .GetValue(UserProfileHistoryRecordLimitKey, 4);
            this.minPatientAge = configuration.GetSection(WebClientConfigSection).GetValue(MinPatientAgeKey, 12);
            this.autoMapper = autoMapper;
            this.authenticationDelegate = authenticationDelegate;
            this.applicationSettingsDelegate = applicationSettingsDelegate;
            this.cacheProvider = cacheProvider;
            this.patientRepository = patientRepository;
            this.messageSender = messageSender;
            this.changeFeedEnabled = configuration.GetSection(ChangeFeedConfiguration.ConfigurationSectionKey)
                .GetValue($"{ChangeFeedConfiguration.AccountsKey}:Enabled", false);
        }

        /// <inheritdoc/>
        [SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "Requires refactoring")]
        public async Task<RequestResult<UserProfileModel>> GetUserProfile(string hdid, DateTime jwtAuthTime)
        {
            this.logger.LogTrace("Getting user profile... {Hdid}", hdid);
            DbResult<UserProfile> userProfileDbResult = this.userProfileDelegate.GetUserProfile(hdid);
            this.logger.LogDebug("Finished getting user profile...{Hdid}", hdid);

            if (userProfileDbResult.Status == DbStatusCode.NotFound)
            {
                return new RequestResult<UserProfileModel>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new UserProfileModel(),
                };
            }

            DateTime previousLastLogin = userProfileDbResult.Payload.LastLoginDateTime;
            if (DateTime.Compare(previousLastLogin, jwtAuthTime) != 0)
            {
                this.logger.LogTrace("Updating user last login and year of birth... {Hdid}", hdid);
                userProfileDbResult.Payload.LastLoginDateTime = jwtAuthTime;
                userProfileDbResult.Payload.LastLoginClientCode = this.authenticationDelegate.FetchAuthenticatedUserClientType();

                // Update user year of birth.
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);
                DateTime? birthDate = patientResult.ResourcePayload?.Birthdate;
                userProfileDbResult.Payload.YearOfBirth = birthDate?.Year.ToString(CultureInfo.InvariantCulture);

                this.userProfileDelegate.Update(userProfileDbResult.Payload);
                this.logger.LogDebug("Finished updating user last login and year of birth... {Hdid}", hdid);
            }

            DbResult<IEnumerable<UserProfileHistory>> userProfileHistoryDbResult =
                this.userProfileDelegate.GetUserProfileHistories(hdid, this.userProfileHistoryRecordLimit);
            UserProfileModel userProfile = this.BuildUserProfileModel(userProfileDbResult.Payload, userProfileHistoryDbResult.Payload.ToArray());

            // Populate most recent login date time
            userProfile.LastLoginDateTimes.Add(userProfileDbResult.Payload.LastLoginDateTime);
            foreach (UserProfileHistory userProfileHistory in userProfileHistoryDbResult.Payload)
            {
                userProfile.LastLoginDateTimes.Add(userProfileHistory.LastLoginDateTime);
            }

            if (!userProfile.IsEmailVerified)
            {
                this.logger.LogTrace("Retrieving last email invite... {Hdid}", hdid);
                MessagingVerification? emailInvite =
                    this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Email);
                this.logger.LogDebug("Finished retrieving email invite... {Hdid}", hdid);
                userProfile.Email = emailInvite?.Email?.To;
            }

            if (!userProfile.IsSmsNumberVerified)
            {
                this.logger.LogTrace("Retrieving last sms invite... {Hdid}", hdid);
                MessagingVerification? smsInvite =
                    this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Sms);
                this.logger.LogDebug("Finished retrieving sms invite... {Hdid}", hdid);
                userProfile.SmsNumber = smsInvite?.SmsNumber;
            }

            return new RequestResult<UserProfileModel>
            {
                ResultStatus = userProfileDbResult.Status != DbStatusCode.Error ? ResultType.Success : ResultType.Error,
                ResultError = userProfileDbResult.Status != DbStatusCode.Error
                    ? null
                    : new RequestResultError
                    {
                        ResultMessage = userProfileDbResult.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    },
                ResourcePayload = userProfile,
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> CreateUserProfile(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string? jwtEmailAddress, CancellationToken ct = default)
        {
            this.logger.LogTrace("Creating user profile... {Hdid}", createProfileRequest.Profile.HdId);

            RequestResult<UserProfileModel>? validationResult = await this.ValidateUserProfile(createProfileRequest).ConfigureAwait(true);
            if (validationResult != null)
            {
                return validationResult;
            }

            string hdid = createProfileRequest.Profile.HdId;

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);
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
                YearOfBirth = birthDate?.Year.ToString(CultureInfo.InvariantCulture),
                LastLoginClientCode = this.authenticationDelegate.FetchAuthenticatedUserClientType(),
            };
            DbResult<UserProfile> insertResult = await this.userProfileDelegate.InsertUserProfileAsync(newProfile, !this.changeFeedEnabled, ct);

            if (this.changeFeedEnabled)
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

            UserProfileModel userProfileModel = this.BuildUserProfileModel(dbModel);

            NotificationSettingsRequest notificationRequest = new(dbModel, requestedEmail, requestedSmsNumber);

            // Add email verification
            if (!string.IsNullOrWhiteSpace(requestedEmail))
            {
                bool isEmailVerified = requestedEmail.Equals(jwtEmailAddress, StringComparison.OrdinalIgnoreCase);
                await this.userEmailService.CreateUserEmailAsync(hdid, requestedEmail, isEmailVerified, !this.changeFeedEnabled, ct);
                userProfileModel.Email = requestedEmail;
                userProfileModel.IsEmailVerified = isEmailVerified;
                if (isEmailVerified)
                {
                    await this.messageSender.SendAsync(new[] { new MessageEnvelope(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Email, requestedEmail)) }, ct);
                }
            }

            // Add SMS verification
            if (!string.IsNullOrWhiteSpace(requestedSmsNumber))
            {
                MessagingVerification smsVerification = await this.userSmsService.CreateUserSmsAsync(hdid, requestedSmsNumber, ct);
                notificationRequest.SmsVerificationCode = smsVerification.SmsValidationCode;
                userProfileModel.SmsNumber = requestedSmsNumber;
            }

            this.notificationSettingsService.QueueNotificationSettings(notificationRequest);

            this.logger.LogDebug("Finished creating user profile... {Hdid}", insertResult.Payload.HdId);

            return RequestResultFactory.Success(userProfileModel);
        }

        /// <inheritdoc/>
        public RequestResult<UserProfileModel> CloseUserProfile(string hdid, Guid userId)
        {
            this.logger.LogTrace("Closing user profile... {Hdid}", hdid);

            DbResult<UserProfile> retrieveResult = this.userProfileDelegate.GetUserProfile(hdid);

            if (retrieveResult.Status != DbStatusCode.Read)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, retrieveResult.Message);
            }

            UserProfile profile = retrieveResult.Payload;
            if (profile.ClosedDateTime != null)
            {
                this.logger.LogTrace("Finished. Profile already Closed");
                return RequestResultFactory.Success(this.BuildUserProfileModel(profile));
            }

            profile.ClosedDateTime = DateTime.UtcNow;
            profile.IdentityManagementId = userId;
            DbResult<UserProfile> updateResult = this.userProfileDelegate.Update(profile);
            if (!string.IsNullOrWhiteSpace(profile.Email))
            {
                this.QueueEmail(profile.Email, EmailTemplateName.AccountClosedTemplate);
            }

            return RequestResultFactory.Success(this.BuildUserProfileModel(updateResult.Payload));
        }

        /// <inheritdoc/>
        public RequestResult<UserProfileModel> RecoverUserProfile(string hdid)
        {
            this.logger.LogTrace("Recovering user profile... {Hdid}", hdid);

            DbResult<UserProfile> retrieveResult = this.userProfileDelegate.GetUserProfile(hdid);

            if (retrieveResult.Status != DbStatusCode.Read)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, retrieveResult.Message);
            }

            UserProfile profile = retrieveResult.Payload;
            if (profile.ClosedDateTime == null)
            {
                this.logger.LogTrace("Finished. Profile already is active, recover not needed");
                return RequestResultFactory.Success(this.BuildUserProfileModel(profile));
            }

            // Remove values set for deletion
            profile.ClosedDateTime = null;
            profile.IdentityManagementId = null;
            DbResult<UserProfile> updateResult = this.userProfileDelegate.Update(profile);
            if (!string.IsNullOrWhiteSpace(profile.Email))
            {
                this.QueueEmail(profile.Email, EmailTemplateName.AccountRecoveredTemplate);
            }

            return RequestResultFactory.Success(this.BuildUserProfileModel(updateResult.Payload));
        }

        /// <inheritdoc/>
        public RequestResult<TermsOfServiceModel> GetActiveTermsOfService()
        {
            DbResult<LegalAgreement> retVal = this.legalAgreementDelegate.GetActiveByAgreementType(LegalAgreementType.TermsOfService);

            if (retVal.Status == DbStatusCode.Error)
            {
                return RequestResultFactory.ServiceError<TermsOfServiceModel>(ErrorType.CommunicationInternal, ServiceType.Database, retVal.Message);
            }

            return RequestResultFactory.Success(this.autoMapper.Map<TermsOfServiceModel>(retVal.Payload));
        }

        /// <inheritdoc/>
        public RequestResult<UserPreferenceModel> UpdateUserPreference(UserPreferenceModel userPreferenceModel)
        {
            this.logger.LogTrace("Updating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);

            UserPreference userPreference = this.autoMapper.Map<UserPreference>(userPreferenceModel);

            DbResult<UserPreference> dbResult = this.userPreferenceDelegate.UpdateUserPreference(userPreference);

            if (dbResult.Status != DbStatusCode.Updated)
            {
                return RequestResultFactory.ServiceError<UserPreferenceModel>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.autoMapper.Map<UserPreferenceModel>(dbResult.Payload));
        }

        /// <inheritdoc/>
        public RequestResult<UserPreferenceModel> CreateUserPreference(UserPreferenceModel userPreferenceModel)
        {
            this.logger.LogTrace("Creating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);
            UserPreference userPreference = this.autoMapper.Map<UserPreference>(userPreferenceModel);
            DbResult<UserPreference> dbResult = this.userPreferenceDelegate.CreateUserPreference(userPreference);

            if (dbResult.Status != DbStatusCode.Created)
            {
                return RequestResultFactory.ServiceError<UserPreferenceModel>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.autoMapper.Map<UserPreferenceModel>(dbResult.Payload));
        }

        /// <inheritdoc/>
        public RequestResult<Dictionary<string, UserPreferenceModel>> GetUserPreferences(string hdid)
        {
            this.logger.LogTrace("Getting user preference... {Hdid}", hdid);
            DbResult<IEnumerable<UserPreference>> dbResult = this.userPreferenceDelegate.GetUserPreferences(hdid);

            if (dbResult.Status != DbStatusCode.Read)
            {
                return RequestResultFactory.ServiceError<Dictionary<string, UserPreferenceModel>>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.autoMapper.Map<IEnumerable<UserPreferenceModel>>(dbResult.Payload).ToDictionary(x => x.Preference, x => x));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<bool>> ValidateMinimumAge(string hdid)
        {
            if (this.minPatientAge == 0)
            {
                return RequestResultFactory.Success(true);
            }

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success || patientResult.ResourcePayload == null)
            {
                this.logger.LogWarning("Error retrieving patient age... {Hdid}", hdid);
                return RequestResultFactory.Error(false, patientResult.ResultError);
            }

            bool isValid = AgeRangeValidator.IsValid(patientResult.ResourcePayload.Birthdate, this.minPatientAge);

            return RequestResultFactory.Success(isValid);
        }

        /// <inheritdoc/>
        public RequestResult<UserProfileModel> UpdateAcceptedTerms(string hdid, Guid termsOfServiceId)
        {
            DbResult<UserProfile> profileResult = this.userProfileDelegate.GetUserProfile(hdid);

            if (profileResult.Status != DbStatusCode.Read)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, "Unable to retrieve user profile");
            }

            profileResult.Payload.TermsOfServiceId = termsOfServiceId;
            profileResult = this.userProfileDelegate.Update(profileResult.Payload);
            if (profileResult.Status != DbStatusCode.Updated)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, "Unable to update the terms of service: DB Error");
            }

            return RequestResultFactory.Success(this.BuildUserProfileModel(profileResult.Payload));
        }

        /// <inheritdoc/>
        public bool IsPhoneNumberValid(string phoneNumber)
        {
            return UserProfileValidator.ValidateUserProfileSmsNumber(phoneNumber);
        }

        private void QueueEmail(string toEmail, string templateName)
        {
            string activationHost = this.httpContextAccessor.HttpContext!.Request
                .GetTypedHeaders()
                .Referer!
                .GetLeftPart(UriPartial.Authority);
            string hostUrl = activationHost;

            Dictionary<string, string> keyValues = new() { [EmailTemplateVariable.Host] = hostUrl };
            this.emailQueueService.QueueNewEmail(toEmail, templateName, keyValues);
        }

        private async Task<RequestResult<UserProfileModel>?> ValidateUserProfile(CreateUserRequest createProfileRequest)
        {
            // Validate registration age
            string hdid = createProfileRequest.Profile.HdId;
            RequestResult<bool> isMinimumAgeResult = await this.ValidateMinimumAge(hdid).ConfigureAwait(true);
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
            ValidationResult profileValidationResult = new UserProfileValidator().Validate(createProfileRequest.Profile);
            if (!profileValidationResult.IsValid)
            {
                this.logger.LogWarning("Profile inputs have failed validation for {Hdid}", createProfileRequest.Profile.HdId);
                return RequestResultFactory.Error<UserProfileModel>(ErrorType.SmsInvalid, "Profile values entered are invalid");
            }

            return null;
        }

        private UserProfileModel BuildUserProfileModel(UserProfile userProfile, UserProfileHistory[]? profileHistoryCollection = null)
        {
            Guid? termsOfServiceId = this.GetActiveTermsOfService().ResourcePayload?.Id;
            DateTime? latestTourChangeDateTime = this.GetLatestTourChangeDateTime();
            UserProfileModel userProfileModel = UserProfileMapUtils.CreateFromDbModel(userProfile, termsOfServiceId, this.autoMapper);
            userProfileModel.HasTourUpdated = profileHistoryCollection != null &&
                                              profileHistoryCollection.Any() &&
                                              latestTourChangeDateTime != null &&
                                              profileHistoryCollection.Max(x => x.LastLoginDateTime) < latestTourChangeDateTime;
            userProfileModel.BlockedDataSources = this.patientRepository.GetDataSources(userProfile.HdId).Result;
            return userProfileModel;
        }

        private DateTime? GetLatestTourChangeDateTime()
        {
            return this.cacheProvider.GetOrSet<DateTime?>(
                $"{TourApplicationSettings.Application}:{TourApplicationSettings.Component}:{TourApplicationSettings.LatestChangeDateTime}",
                () =>
                {
                    ApplicationSetting? applicationSetting = this.applicationSettingsDelegate.GetApplicationSetting(
                        TourApplicationSettings.Application,
                        TourApplicationSettings.Component,
                        TourApplicationSettings.LatestChangeDateTime);
                    return applicationSetting?.Value != null ? DateTime.Parse(applicationSetting.Value, CultureInfo.InvariantCulture).ToUniversalTime() : null;
                },
                TimeSpan.FromMinutes(30));
        }
    }
}
