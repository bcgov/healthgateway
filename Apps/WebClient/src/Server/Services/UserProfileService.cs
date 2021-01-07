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
namespace HealthGateway.WebClient.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Constants;
    using HealthGateway.WebClient.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class UserProfileService : IUserProfileService
    {
        private const string HostTemplateVariable = "host";

        private readonly ILogger logger;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IUserPreferenceDelegate userPreferenceDelegate;
        private readonly IEmailDelegate emailDelegate;
        private readonly IMessagingVerificationDelegate emailInviteDelegate;
        private readonly IConfigurationService configurationService;
        private readonly IEmailQueueService emailQueueService;
        private readonly ILegalAgreementDelegate legalAgreementDelegate;
        private readonly ICryptoDelegate cryptoDelegate;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly IPatientService patientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="userProfileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="userPreferenceDelegate">The preference delegate to interact with the DB.</param>
        /// <param name="emailDelegate">The email delegate to interact with the DB.</param>
        /// <param name="emailInviteDelegate">The email invite delegate to interact with the DB.</param>
        /// <param name="configuration">The configuration service.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="legalAgreementDelegate">The terms of service delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="patientService">The patient service.</param>
        public UserProfileService(
            ILogger<UserProfileService> logger,
            IUserProfileDelegate userProfileDelegate,
            IUserPreferenceDelegate userPreferenceDelegate,
            IEmailDelegate emailDelegate,
            IMessagingVerificationDelegate emailInviteDelegate,
            IConfigurationService configuration,
            IEmailQueueService emailQueueService,
            ILegalAgreementDelegate legalAgreementDelegate,
            ICryptoDelegate cryptoDelegate,
            INotificationSettingsService notificationSettingsService,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IPatientService patientService)
        {
            this.logger = logger;
            this.userProfileDelegate = userProfileDelegate;
            this.userPreferenceDelegate = userPreferenceDelegate;
            this.emailDelegate = emailDelegate;
            this.emailInviteDelegate = emailInviteDelegate;
            this.configurationService = configuration;
            this.emailQueueService = emailQueueService;
            this.legalAgreementDelegate = legalAgreementDelegate;
            this.cryptoDelegate = cryptoDelegate;
            this.notificationSettingsService = notificationSettingsService;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.patientService = patientService;
        }

        /// <inheritdoc />
        public RequestResult<UserProfileModel> GetUserProfile(string hdid, DateTime jwtAuthTime)
        {
            this.logger.LogTrace($"Getting user profile... {hdid}");
            DBResult<UserProfile> retVal = this.userProfileDelegate.GetUserProfile(hdid);
            this.logger.LogDebug($"Finished getting user profile. {JsonSerializer.Serialize(retVal)}");

            if (retVal.Status == DBStatusCode.NotFound)
            {
                return new RequestResult<UserProfileModel>()
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new UserProfileModel(),
                };
            }

            DateTime previousLastLogin = retVal.Payload.LastLoginDateTime;
            if (DateTime.Compare(previousLastLogin, jwtAuthTime) != 0)
            {
                this.logger.LogTrace($"Updating user last login... {hdid}");
                retVal.Payload.LastLoginDateTime = jwtAuthTime;
                DBResult<UserProfile> updateResult = this.userProfileDelegate.Update(retVal.Payload);
                this.logger.LogDebug($"Finished updating user last login. {JsonSerializer.Serialize(updateResult)}");
            }

            RequestResult<TermsOfServiceModel> termsOfServiceResult = this.GetActiveTermsOfService();

            UserProfileModel userProfile = UserProfileModel.CreateFromDbModel(retVal.Payload);
            userProfile.HasTermsOfServiceUpdated = termsOfServiceResult.ResourcePayload?.EffectiveDate > previousLastLogin;

            return new RequestResult<UserProfileModel>()
            {
                ResultStatus = retVal.Status != DBStatusCode.Error ? ResultType.Success : ResultType.Error,
                ResultError = retVal.Status != DBStatusCode.Error ? null : new RequestResultError() { ResultMessage = retVal.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
                ResourcePayload = userProfile,
            };
        }

        /// <inheritdoc />
        public async Task<RequestResult<UserProfileModel>> CreateUserProfile(CreateUserRequest createProfileRequest, Uri hostUri, string bearerToken, DateTime lastLogin)
        {
            this.logger.LogTrace($"Creating user profile... {JsonSerializer.Serialize(createProfileRequest)}");

            string registrationStatus = this.configurationService.GetConfiguration().WebClient.RegistrationStatus;

            RequestResult<UserProfileModel> requestResult = new RequestResult<UserProfileModel>();

            if (registrationStatus == RegistrationStatus.Closed)
            {
                requestResult.ResultStatus = ResultType.Error;
                requestResult.ResultError = new RequestResultError() { ResultMessage = "Registration is closed", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) };
                this.logger.LogWarning($"Registration is closed. {JsonSerializer.Serialize(createProfileRequest)}");
                return requestResult;
            }

            string hdid = createProfileRequest.Profile.HdId;
            PrimitiveRequestResult<bool> isMimimunAgeResult = await this.ValidateMinimumAge(hdid).ConfigureAwait(true);

            if (isMimimunAgeResult.ResultStatus != ResultType.Success)
            {
                requestResult.ResultStatus = isMimimunAgeResult.ResultStatus;
                requestResult.ResultError = isMimimunAgeResult.ResultError;
                return requestResult;
            }
            else if (!isMimimunAgeResult.ResourcePayload)
            {
                requestResult.ResultStatus = ResultType.Error;
                requestResult.ResultError = new RequestResultError() { ResultMessage = "Patient under minimum age", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) };
                this.logger.LogWarning($"Patient under minimum age. {JsonSerializer.Serialize(createProfileRequest)}");
                return requestResult;
            }

            string? requestedSMSNumber = createProfileRequest.Profile.SMSNumber;
            string? requestedEmail = createProfileRequest.Profile.Email;
            UserProfile newProfile = createProfileRequest.Profile;
            newProfile.Email = string.Empty;
            newProfile.SMSNumber = null;
            newProfile.CreatedBy = hdid;
            newProfile.UpdatedBy = hdid;
            newProfile.LastLoginDateTime = lastLogin;
            newProfile.EncryptionKey = this.cryptoDelegate.GenerateKey();

            DBResult<UserProfile> insertResult = this.userProfileDelegate.InsertUserProfile(newProfile);

            if (insertResult.Status == DBStatusCode.Created)
            {
                // Update the notification settings
                NotificationSettingsRequest notificationRequest = this.UpdateNotificationSettings(newProfile, requestedSMSNumber);

                if (!string.IsNullOrWhiteSpace(requestedEmail))
                {
                    this.emailQueueService.QueueNewInviteEmail(hdid, requestedEmail, hostUri);
                }

                if (!string.IsNullOrWhiteSpace(requestedSMSNumber))
                {
                    this.logger.LogInformation($"Sending sms invite for user ${hdid}");
                    MessagingVerification messagingVerification = new MessagingVerification();
                    messagingVerification.HdId = hdid;
                    messagingVerification.SMSNumber = requestedSMSNumber;
                    messagingVerification.SMSValidationCode = notificationRequest.SMSVerificationCode;
                    messagingVerification.VerificationType = MessagingVerificationType.SMS;
                    messagingVerification.ExpireDate = DateTime.MaxValue;
                    this.messageVerificationDelegate.Insert(messagingVerification);
                }

                requestResult.ResourcePayload = UserProfileModel.CreateFromDbModel(insertResult.Payload);
                requestResult.ResultStatus = ResultType.Success;
            }

            this.logger.LogDebug($"Finished creating user profile. {JsonSerializer.Serialize(insertResult)}");
            return requestResult;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team decision")]
        public RequestResult<UserProfileModel> CloseUserProfile(string hdid, Guid userId, string hostUrl)
        {
            this.logger.LogTrace($"Closing user profile... {hdid}");

            string registrationStatus = this.configurationService.GetConfiguration().WebClient.RegistrationStatus;

            RequestResult<UserProfileModel> requestResult = new RequestResult<UserProfileModel>();

            DBResult<UserProfile> retrieveResult = this.userProfileDelegate.GetUserProfile(hdid);

            if (retrieveResult.Status == DBStatusCode.Read)
            {
                UserProfile profile = retrieveResult.Payload;
                if (profile.ClosedDateTime != null)
                {
                    this.logger.LogTrace("Finished. Profile already Closed");
                    requestResult.ResourcePayload = UserProfileModel.CreateFromDbModel(profile);
                    requestResult.ResultStatus = ResultType.Success;
                    return requestResult;
                }

                profile.ClosedDateTime = DateTime.Now;
                profile.IdentityManagementId = userId;
                DBResult<UserProfile> updateResult = this.userProfileDelegate.Update(profile);
                if (!string.IsNullOrWhiteSpace(profile.Email))
                {
                    Dictionary<string, string> keyValues = new Dictionary<string, string>();
                    keyValues.Add(HostTemplateVariable, hostUrl);
                    this.emailQueueService.QueueNewEmail(profile.Email, EmailTemplateName.AccountClosedTemplate, keyValues);
                }

                requestResult.ResourcePayload = UserProfileModel.CreateFromDbModel(updateResult.Payload);
                requestResult.ResultStatus = ResultType.Success;
                this.logger.LogDebug($"Finished closing user profile. {JsonSerializer.Serialize(updateResult)}");
            }

            return requestResult;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team decision")]
        public RequestResult<UserProfileModel> RecoverUserProfile(string hdid, string hostUrl)
        {
            this.logger.LogTrace($"Recovering user profile... {hdid}");

            string registrationStatus = this.configurationService.GetConfiguration().WebClient.RegistrationStatus;

            RequestResult<UserProfileModel> requestResult = new RequestResult<UserProfileModel>();

            DBResult<UserProfile> retrieveResult = this.userProfileDelegate.GetUserProfile(hdid);

            if (retrieveResult.Status == DBStatusCode.Read)
            {
                UserProfile profile = retrieveResult.Payload;
                if (profile.ClosedDateTime == null)
                {
                    this.logger.LogTrace("Finished. Profile already is active, recover not needed.");
                    requestResult.ResourcePayload = UserProfileModel.CreateFromDbModel(profile);
                    requestResult.ResultStatus = ResultType.Success;
                    return requestResult;
                }

                // Remove values set for deletion
                profile.ClosedDateTime = null;
                profile.IdentityManagementId = null;
                DBResult<UserProfile> updateResult = this.userProfileDelegate.Update(profile);
                if (!string.IsNullOrWhiteSpace(profile.Email))
                {
                    Dictionary<string, string> keyValues = new Dictionary<string, string>();
                    keyValues.Add(HostTemplateVariable, hostUrl);
                    this.emailQueueService.QueueNewEmail(profile.Email, EmailTemplateName.AccountRecoveredTemplate, keyValues);
                }

                requestResult.ResourcePayload = UserProfileModel.CreateFromDbModel(updateResult.Payload);
                requestResult.ResultStatus = ResultType.Success;
                this.logger.LogDebug($"Finished recovering user profile. {JsonSerializer.Serialize(updateResult)}");
            }

            return requestResult;
        }

        /// <inheritdoc />
        public RequestResult<TermsOfServiceModel> GetActiveTermsOfService()
        {
            this.logger.LogDebug($"Getting active terms of service...");
            DBResult<LegalAgreement> retVal = this.legalAgreementDelegate.GetActiveByAgreementType(LegalAgreementType.TermsofService);

            return new RequestResult<TermsOfServiceModel>()
            {
                ResultStatus = retVal.Status != DBStatusCode.Error ? ResultType.Success : ResultType.Error,
                ResourcePayload = TermsOfServiceModel.CreateFromDbModel(retVal.Payload),
                ResultError = retVal.Status != DBStatusCode.Error ? null : new RequestResultError() { ResultMessage = retVal.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
        }

        /// <inheritdoc />
        public RequestResult<UserPreferenceModel> UpdateUserPreference(UserPreferenceModel userPreferenceModel)
        {
            this.logger.LogTrace($"Updating user preference... {userPreferenceModel.Preference}");

            UserPreference userPreference = userPreferenceModel.ToDbModel();

            DBResult<UserPreference> dbResult = this.userPreferenceDelegate.UpdateUserPreference(userPreference);
            this.logger.LogDebug($"Finished updating user preference. {JsonSerializer.Serialize(dbResult)}");

            RequestResult<UserPreferenceModel> requestResult = new RequestResult<UserPreferenceModel>()
            {
                ResourcePayload = UserPreferenceModel.CreateFromDbModel(dbResult.Payload),
                ResultStatus = dbResult.Status == DBStatusCode.Updated ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DBStatusCode.Updated ? null : new RequestResultError()
                {
                    ResultMessage = dbResult.Message,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                },
            };
            return requestResult;
        }

        /// <inheritdoc />
        public RequestResult<UserPreferenceModel> CreateUserPreference(UserPreferenceModel userPreferenceModel)
        {
            this.logger.LogTrace($"Creating user preference... {userPreferenceModel.Preference}");

            UserPreference userPreference = userPreferenceModel.ToDbModel();

            DBResult<UserPreference> dbResult = this.userPreferenceDelegate.CreateUserPreference(userPreference);
            this.logger.LogDebug($"Finished creating user preference. {JsonSerializer.Serialize(dbResult)}");

            RequestResult<UserPreferenceModel> requestResult = new RequestResult<UserPreferenceModel>()
            {
                ResourcePayload = UserPreferenceModel.CreateFromDbModel(dbResult.Payload),
                ResultStatus = dbResult.Status == DBStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DBStatusCode.Created ? null : new RequestResultError()
                {
                    ResultMessage = dbResult.Message,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                },
            };
            return requestResult;
        }

        /// <inheritdoc />
        public RequestResult<Dictionary<string, UserPreferenceModel>> GetUserPreferences(string hdid)
        {
            this.logger.LogTrace($"Getting user preference... {hdid}");
            DBResult<IEnumerable<UserPreference>> dbResult = this.userPreferenceDelegate.GetUserPreferences(hdid);
            RequestResult<Dictionary<string, UserPreferenceModel>> requestResult = new RequestResult<Dictionary<string, UserPreferenceModel>>()
            {
                ResourcePayload = UserPreferenceModel.CreateListFromDbModel(dbResult.Payload).ToDictionary(x => x.Preference, x => x),
                ResultStatus = dbResult.Status == DBStatusCode.Read ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DBStatusCode.Read ? null : new RequestResultError()
                {
                    ResultMessage = dbResult.Message,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                },
            };

            this.logger.LogTrace($"Finished getting user preference. {JsonSerializer.Serialize(dbResult)}");
            return requestResult;
        }

        /// <inheritdoc />
        public async Task<PrimitiveRequestResult<bool>> ValidateMinimumAge(string hdid)
        {
            int? minAge = this.configurationService.GetConfiguration().WebClient.MinPatientAge;

            if (!minAge.HasValue || minAge.Value == 0)
            {
                return new PrimitiveRequestResult<bool>() { ResourcePayload = true, ResultStatus = ResultType.Success };
            }

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogWarning($"Error retrieving patient age. {JsonSerializer.Serialize(patientResult)}");
                return new PrimitiveRequestResult<bool>()
                {
                    ResultStatus = patientResult.ResultStatus,
                    ResultError = patientResult.ResultError,
                    ResourcePayload = false,
                };
            }
            else
            {
                DateTime birthDate = patientResult.ResourcePayload?.Birthdate ?? new DateTime();
                return new PrimitiveRequestResult<bool>()
                {
                    ResultStatus = patientResult.ResultStatus,
                    ResourcePayload = birthDate.AddYears(minAge.Value) < DateTime.Now,
                };
            }
        }

        private NotificationSettingsRequest UpdateNotificationSettings(UserProfile userProfile, string? smsNumber)
        {
            // Update the notification settings
            NotificationSettingsRequest request = new NotificationSettingsRequest(userProfile, userProfile.Email, smsNumber);
            this.notificationSettingsService.QueueNotificationSettings(request);
            return request;
        }
    }
}
