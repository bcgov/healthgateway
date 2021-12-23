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
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class UserProfileService : IUserProfileService
    {
        private readonly string webClientConfigSection = "WebClient";
        private readonly string userProfileHistoryRecordLimitKey = "UserProfileHistoryRecordLimit";
        private readonly int userProfileHistoryRecordLimit;
        private readonly ILogger logger;
        private readonly IPatientService patientService;
        private readonly IUserEmailService userEmailService;
        private readonly IUserSMSService userSMSService;
        private readonly IConfigurationService configurationService;
        private readonly IEmailQueueService emailQueueService;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IUserPreferenceDelegate userPreferenceDelegate;
        private readonly ILegalAgreementDelegate legalAgreementDelegate;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly ICryptoDelegate cryptoDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="patientService">The patient service.</param>
        /// <param name="userEmailService">The User Email service.</param>
        /// <param name="userSMSService">The User SMS service.</param>
        /// <param name="configurationService">The configuration service.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="notificationSettingsService">The Notifications Settings service.</param>
        /// <param name="userProfileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="userPreferenceDelegate">The preference delegate to interact with the DB.</param>
        /// <param name="legalAgreementDelegate">The terms of service delegate.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="configuration">Configuration settings.</param>
        public UserProfileService(
            ILogger<UserProfileService> logger,
            IPatientService patientService,
            IUserEmailService userEmailService,
            IUserSMSService userSMSService,
            IConfigurationService configurationService,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService,
            IUserProfileDelegate userProfileDelegate,
            IUserPreferenceDelegate userPreferenceDelegate,
            ILegalAgreementDelegate legalAgreementDelegate,
            IMessagingVerificationDelegate messageVerificationDelegate,
            ICryptoDelegate cryptoDelegate,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.userEmailService = userEmailService;
            this.userSMSService = userSMSService;
            this.configurationService = configurationService;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
            this.userProfileDelegate = userProfileDelegate;
            this.userPreferenceDelegate = userPreferenceDelegate;
            this.legalAgreementDelegate = legalAgreementDelegate;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.cryptoDelegate = cryptoDelegate;
            this.httpContextAccessor = httpContextAccessor;
            this.userProfileHistoryRecordLimit = configuration.GetSection(this.webClientConfigSection).GetValue<int>(this.userProfileHistoryRecordLimitKey, 4);
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
            DBResult<IEnumerable<UserProfileHistory>> userProfileHistoryDbResult = this.userProfileDelegate.GetUserProfileHistories(hdid, this.userProfileHistoryRecordLimit);

            // Populate most recent login date time
            userProfile.LastLoginDateTimes.Add(retVal.Payload.LastLoginDateTime);
            foreach (UserProfileHistory userProfileHistory in userProfileHistoryDbResult.Payload)
            {
                userProfile.LastLoginDateTimes.Add(userProfileHistory.LastLoginDateTime);
            }

            userProfile.HasTermsOfServiceUpdated = termsOfServiceResult.ResourcePayload?.EffectiveDate > previousLastLogin;

            if (!userProfile.IsEmailVerified)
            {
                this.logger.LogTrace($"Retrieving last email invite... {hdid}");
                MessagingVerification? emailInvite = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.Email);
                this.logger.LogDebug($"Finished retrieving email: {JsonSerializer.Serialize(emailInvite)}");
                userProfile.Email = emailInvite?.Email?.To;
            }

            if (!userProfile.IsSMSNumberVerified)
            {
                this.logger.LogTrace($"Retrieving last email invite... {hdid}");
                MessagingVerification? smsInvite = this.messageVerificationDelegate.GetLastForUser(hdid, MessagingVerificationType.SMS);
                this.logger.LogDebug($"Finished retrieving email: {JsonSerializer.Serialize(smsInvite)}");
                userProfile.SMSNumber = smsInvite?.SMSNumber;
            }

            return new RequestResult<UserProfileModel>()
            {
                ResultStatus = retVal.Status != DBStatusCode.Error ? ResultType.Success : ResultType.Error,
                ResultError = retVal.Status != DBStatusCode.Error ? null : new RequestResultError() { ResultMessage = retVal.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
                ResourcePayload = userProfile,
            };
        }

        /// <inheritdoc />
        public async Task<RequestResult<UserProfileModel>> CreateUserProfile(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string jwtEmailAddress)
        {
            this.logger.LogTrace($"Creating user profile... {JsonSerializer.Serialize(createProfileRequest)}");

            string registrationStatus = this.configurationService.GetConfiguration().WebClient.RegistrationStatus;

            if (registrationStatus == RegistrationStatus.Closed)
            {
                this.logger.LogWarning($"Registration is closed. {JsonSerializer.Serialize(createProfileRequest)}");
                return new RequestResult<UserProfileModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Registration is closed",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }

            // Validate registration age
            string hdid = createProfileRequest.Profile.HdId;
            PrimitiveRequestResult<bool> isMimimunAgeResult = await this.ValidateMinimumAge(hdid).ConfigureAwait(true);
            if (isMimimunAgeResult.ResultStatus != ResultType.Success)
            {
                return new RequestResult<UserProfileModel>()
                {
                    ResultStatus = isMimimunAgeResult.ResultStatus,
                    ResultError = isMimimunAgeResult.ResultError,
                };
            }
            else if (!isMimimunAgeResult.ResourcePayload)
            {
                this.logger.LogWarning($"Patient under minimum age. {JsonSerializer.Serialize(createProfileRequest)}");
                return new RequestResult<UserProfileModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Patient under minimum age",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }

            // Create profile
            UserProfile newProfile = new()
            {
                HdId = hdid,
                IdentityManagementId = createProfileRequest.Profile.IdentityManagementId,
                AcceptedTermsOfService = createProfileRequest.Profile.AcceptedTermsOfService,
                Email = string.Empty,
                SMSNumber = null,
                CreatedBy = hdid,
                UpdatedBy = hdid,
                LastLoginDateTime = jwtAuthTime,
                EncryptionKey = this.cryptoDelegate.GenerateKey(),
            };
            DBResult<UserProfile> insertResult = this.userProfileDelegate.InsertUserProfile(newProfile);

            if (insertResult.Status == DBStatusCode.Created)
            {
                UserProfile createdProfile = insertResult.Payload;
                string? requestedSMSNumber = createProfileRequest.Profile.SMSNumber;
                string? requestedEmail = createProfileRequest.Profile.Email;

                NotificationSettingsRequest notificationRequest = new(createdProfile, requestedEmail, requestedSMSNumber);

                // Add email verification
                if (!string.IsNullOrWhiteSpace(requestedEmail))
                {
                    this.userEmailService.CreateUserEmail(hdid, requestedEmail, requestedEmail.Equals(jwtEmailAddress, StringComparison.OrdinalIgnoreCase));
                }

                // Add SMS verification
                if (!string.IsNullOrWhiteSpace(requestedSMSNumber))
                {
                    MessagingVerification smsVerification = this.userSMSService.CreateUserSMS(hdid, requestedSMSNumber);
                    notificationRequest.SMSVerificationCode = smsVerification.SMSValidationCode;
                }

                this.notificationSettingsService.QueueNotificationSettings(notificationRequest);

                this.logger.LogDebug($"Finished creating user profile. {JsonSerializer.Serialize(insertResult)}");
                return new RequestResult<UserProfileModel>()
                {
                    ResourcePayload = UserProfileModel.CreateFromDbModel(insertResult.Payload),
                    ResultStatus = ResultType.Success,
                };
            }
            else
            {
                this.logger.LogError($"Error creating user profile. {JsonSerializer.Serialize(insertResult)}");
                return new RequestResult<UserProfileModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = insertResult.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    },
                };
            }
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team decision")]
        public RequestResult<UserProfileModel> CloseUserProfile(string hdid, Guid userId)
        {
            this.logger.LogTrace($"Closing user profile... {hdid}");

            DBResult<UserProfile> retrieveResult = this.userProfileDelegate.GetUserProfile(hdid);

            if (retrieveResult.Status == DBStatusCode.Read)
            {
                UserProfile profile = retrieveResult.Payload;
                if (profile.ClosedDateTime != null)
                {
                    this.logger.LogTrace("Finished. Profile already Closed");
                    return new RequestResult<UserProfileModel>()
                    {
                        ResourcePayload = UserProfileModel.CreateFromDbModel(profile),
                        ResultStatus = ResultType.Success,
                    };
                }

                profile.ClosedDateTime = DateTime.UtcNow;
                profile.IdentityManagementId = userId;
                DBResult<UserProfile> updateResult = this.userProfileDelegate.Update(profile);
                if (!string.IsNullOrWhiteSpace(profile.Email))
                {
                    this.QueueEmail(profile.Email, EmailTemplateName.AccountClosedTemplate);
                }

                this.logger.LogDebug($"Finished closing user profile. {JsonSerializer.Serialize(updateResult)}");
                return new RequestResult<UserProfileModel>()
                {
                    ResourcePayload = UserProfileModel.CreateFromDbModel(updateResult.Payload),
                    ResultStatus = ResultType.Success,
                };
            }
            else
            {
                return new RequestResult<UserProfileModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = retrieveResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
                };
            }
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team decision")]
        public RequestResult<UserProfileModel> RecoverUserProfile(string hdid)
        {
            this.logger.LogTrace($"Recovering user profile... {hdid}");

            DBResult<UserProfile> retrieveResult = this.userProfileDelegate.GetUserProfile(hdid);

            if (retrieveResult.Status == DBStatusCode.Read)
            {
                UserProfile profile = retrieveResult.Payload;
                if (profile.ClosedDateTime == null)
                {
                    this.logger.LogTrace("Finished. Profile already is active, recover not needed.");
                    return new RequestResult<UserProfileModel>()
                    {
                        ResourcePayload = UserProfileModel.CreateFromDbModel(profile),
                        ResultStatus = ResultType.Success,
                    };
                }

                // Remove values set for deletion
                profile.ClosedDateTime = null;
                profile.IdentityManagementId = null;
                DBResult<UserProfile> updateResult = this.userProfileDelegate.Update(profile);
                if (!string.IsNullOrWhiteSpace(profile.Email))
                {
                    this.QueueEmail(profile.Email, EmailTemplateName.AccountRecoveredTemplate);
                }

                this.logger.LogDebug($"Finished recovering user profile. {JsonSerializer.Serialize(updateResult)}");
                return new RequestResult<UserProfileModel>()
                {
                    ResourcePayload = UserProfileModel.CreateFromDbModel(updateResult.Payload),
                    ResultStatus = ResultType.Success,
                };
            }
            else
            {
                return new RequestResult<UserProfileModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = retrieveResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
                };
            }
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
                DateTime birthDate = patientResult.ResourcePayload?.Birthdate ?? default(DateTime);
                return new PrimitiveRequestResult<bool>()
                {
                    ResultStatus = patientResult.ResultStatus,
                    ResourcePayload = birthDate.AddYears(minAge.Value) < DateTime.Now,
                };
            }
        }

        private void QueueEmail(string toEmail, string templateName)
        {
            string activationHost = this.httpContextAccessor.HttpContext!.Request
                                             .GetTypedHeaders()
                                             .Referer!
                                             .GetLeftPart(UriPartial.Authority);
            string hostUrl = activationHost.ToString();

            Dictionary<string, string> keyValues = new() { [EmailTemplateVariable.Host] = hostUrl };
            this.emailQueueService.QueueNewEmail(toEmail, templateName, keyValues);
        }
    }
}
