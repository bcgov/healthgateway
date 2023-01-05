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
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Constants;
    using HealthGateway.GatewayApi.MapUtils;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class UserProfileService : IUserProfileService
    {
        private const string WebClientConfigSection = "WebClient";
        private const string UserProfileHistoryRecordLimitKey = "UserProfileHistoryRecordLimit";
        private const string RegistrationStatusKey = "RegistrationStatus";
        private const string MinPatientAgeKey = "MinPatientAge";
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
        private readonly string registrationStatus;
        private readonly IUserEmailService userEmailService;
        private readonly IUserPreferenceDelegate userPreferenceDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly int userProfileHistoryRecordLimit;
        private readonly IUserSmsService userSmsService;
        private readonly IAuthenticationDelegate authenticationDelegate;

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
            IAuthenticationDelegate authenticationDelegate)
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
            this.registrationStatus = configuration.GetSection(WebClientConfigSection)
                .GetValue<string>(RegistrationStatusKey) ?? RegistrationStatus.Open;
            this.minPatientAge = configuration.GetSection(WebClientConfigSection).GetValue(MinPatientAgeKey, 12);
            this.autoMapper = autoMapper;
            this.authenticationDelegate = authenticationDelegate;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> GetUserProfile(string hdid, DateTime jwtAuthTime)
        {
            this.logger.LogTrace("Getting user profile... {Hdid}", hdid);
            DbResult<UserProfile> retVal = this.userProfileDelegate.GetUserProfile(hdid);
            this.logger.LogDebug("Finished getting user profile...{Hdid}", hdid);

            if (retVal.Status == DbStatusCode.NotFound)
            {
                return new RequestResult<UserProfileModel>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new UserProfileModel(),
                };
            }

            DateTime previousLastLogin = retVal.Payload.LastLoginDateTime;
            if (DateTime.Compare(previousLastLogin, jwtAuthTime) != 0)
            {
                this.logger.LogTrace("Updating user last login and year of birth... {Hdid}", hdid);
                retVal.Payload.LastLoginDateTime = jwtAuthTime;
                retVal.Payload.LastLoginClientCode = this.authenticationDelegate.FetchAuthenticatedUserClientType();

                // Update user year of birth.
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);
                DateTime? birthDate = patientResult.ResourcePayload?.Birthdate;
                retVal.Payload.YearOfBirth = birthDate?.Year.ToString(CultureInfo.InvariantCulture);

                this.userProfileDelegate.Update(retVal.Payload);
                this.logger.LogDebug("Finished updating user last login and year of birth... {Hdid}", hdid);
            }

            RequestResult<TermsOfServiceModel> termsOfServiceResult = this.GetActiveTermsOfService();
            UserProfileModel userProfile =
                UserProfileMapUtils.CreateFromDbModel(retVal.Payload, termsOfServiceResult.ResourcePayload?.Id, this.autoMapper);
            DbResult<IEnumerable<UserProfileHistory>> userProfileHistoryDbResult =
                this.userProfileDelegate.GetUserProfileHistories(hdid, this.userProfileHistoryRecordLimit);

            // Populate most recent login date time
            userProfile.LastLoginDateTimes.Add(retVal.Payload.LastLoginDateTime);
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
                ResultStatus = retVal.Status != DbStatusCode.Error ? ResultType.Success : ResultType.Error,
                ResultError = retVal.Status != DbStatusCode.Error
                    ? null
                    : new RequestResultError
                    {
                        ResultMessage = retVal.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    },
                ResourcePayload = userProfile,
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> CreateUserProfile(CreateUserRequest createProfileRequest, DateTime jwtAuthTime, string? jwtEmailAddress)
        {
            this.logger.LogTrace("Creating user profile... {Hdid}", createProfileRequest.Profile.HdId);

            if (this.registrationStatus == RegistrationStatus.Closed)
            {
                this.logger.LogWarning("Registration is closed... {Hdid}", createProfileRequest.Profile.HdId);
                return new RequestResult<UserProfileModel>
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Registration is closed",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }

            // Validate registration age
            string hdid = createProfileRequest.Profile.HdId;
            PrimitiveRequestResult<bool> isMinimumAgeResult = await this.ValidateMinimumAge(hdid).ConfigureAwait(true);
            if (isMinimumAgeResult.ResultStatus != ResultType.Success)
            {
                return new RequestResult<UserProfileModel>
                {
                    ResultStatus = isMinimumAgeResult.ResultStatus,
                    ResultError = isMinimumAgeResult.ResultError,
                };
            }

            if (!isMinimumAgeResult.ResourcePayload)
            {
                this.logger.LogWarning("Patient under minimum age... {Hdid}", createProfileRequest.Profile.HdId);
                return new RequestResult<UserProfileModel>
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Patient under minimum age",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }

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
            DbResult<UserProfile> insertResult = this.userProfileDelegate.InsertUserProfile(newProfile);

            if (insertResult.Status == DbStatusCode.Created)
            {
                UserProfile dbModel = insertResult.Payload;
                string? requestedSmsNumber = createProfileRequest.Profile.SmsNumber;
                string? requestedEmail = createProfileRequest.Profile.Email;

                RequestResult<TermsOfServiceModel> termsOfServiceResult = this.GetActiveTermsOfService();
                UserProfileModel userProfileModel = UserProfileMapUtils.CreateFromDbModel(dbModel, termsOfServiceResult.ResourcePayload?.Id, this.autoMapper);

                NotificationSettingsRequest notificationRequest = new(dbModel, requestedEmail, requestedSmsNumber);

                // Add email verification
                if (!string.IsNullOrWhiteSpace(requestedEmail))
                {
                    bool isEmailVerified = requestedEmail.Equals(jwtEmailAddress, StringComparison.OrdinalIgnoreCase);
                    this.userEmailService.CreateUserEmail(hdid, requestedEmail, isEmailVerified);
                    userProfileModel.Email = requestedEmail;
                    userProfileModel.IsEmailVerified = isEmailVerified;
                }

                // Add SMS verification
                if (!string.IsNullOrWhiteSpace(requestedSmsNumber))
                {
                    MessagingVerification smsVerification = this.userSmsService.CreateUserSms(hdid, requestedSmsNumber);
                    notificationRequest.SmsVerificationCode = smsVerification.SmsValidationCode;
                    userProfileModel.SmsNumber = requestedSmsNumber;
                }

                this.notificationSettingsService.QueueNotificationSettings(notificationRequest);

                this.logger.LogDebug("Finished creating user profile... {Hdid}", insertResult.Payload.HdId);
                return new RequestResult<UserProfileModel>
                {
                    ResourcePayload = userProfileModel,
                    ResultStatus = ResultType.Success,
                };
            }

            this.logger.LogError("Error creating user profile... {Hdid}", insertResult.Payload.HdId);
            return new RequestResult<UserProfileModel>
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = insertResult.Message,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                },
            };
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team decision")]
        public RequestResult<UserProfileModel> CloseUserProfile(string hdid, Guid userId)
        {
            this.logger.LogTrace("Closing user profile... {Hdid}", hdid);

            DbResult<UserProfile> retrieveResult = this.userProfileDelegate.GetUserProfile(hdid);

            if (retrieveResult.Status == DbStatusCode.Read)
            {
                RequestResult<TermsOfServiceModel> termsOfServiceResult = this.GetActiveTermsOfService();

                UserProfile profile = retrieveResult.Payload;
                if (profile.ClosedDateTime != null)
                {
                    this.logger.LogTrace("Finished. Profile already Closed");
                    return new RequestResult<UserProfileModel>
                    {
                        ResourcePayload =
                            UserProfileMapUtils.CreateFromDbModel(profile, termsOfServiceResult.ResourcePayload?.Id, this.autoMapper),
                        ResultStatus = ResultType.Success,
                    };
                }

                profile.ClosedDateTime = DateTime.UtcNow;
                profile.IdentityManagementId = userId;
                DbResult<UserProfile> updateResult = this.userProfileDelegate.Update(profile);
                if (!string.IsNullOrWhiteSpace(profile.Email))
                {
                    this.QueueEmail(profile.Email, EmailTemplateName.AccountClosedTemplate);
                }

                this.logger.LogDebug("Finished closing user profile... {Hdid}", updateResult.Payload.HdId);
                return new RequestResult<UserProfileModel>
                {
                    ResourcePayload = UserProfileMapUtils.CreateFromDbModel(updateResult.Payload, termsOfServiceResult.ResourcePayload?.Id, this.autoMapper),
                    ResultStatus = ResultType.Success,
                };
            }

            return new RequestResult<UserProfileModel>
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = retrieveResult.Message,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                },
            };
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Team decision")]
        public RequestResult<UserProfileModel> RecoverUserProfile(string hdid)
        {
            this.logger.LogTrace("Recovering user profile... {Hdid}", hdid);

            DbResult<UserProfile> retrieveResult = this.userProfileDelegate.GetUserProfile(hdid);

            if (retrieveResult.Status == DbStatusCode.Read)
            {
                RequestResult<TermsOfServiceModel> termsOfServiceResult = this.GetActiveTermsOfService();
                UserProfile profile = retrieveResult.Payload;
                if (profile.ClosedDateTime == null)
                {
                    this.logger.LogTrace("Finished. Profile already is active, recover not needed.");
                    return new RequestResult<UserProfileModel>
                    {
                        ResourcePayload =
                            UserProfileMapUtils.CreateFromDbModel(profile, termsOfServiceResult.ResourcePayload?.Id, this.autoMapper),
                        ResultStatus = ResultType.Success,
                    };
                }

                // Remove values set for deletion
                profile.ClosedDateTime = null;
                profile.IdentityManagementId = null;
                DbResult<UserProfile> updateResult = this.userProfileDelegate.Update(profile);
                if (!string.IsNullOrWhiteSpace(profile.Email))
                {
                    this.QueueEmail(profile.Email, EmailTemplateName.AccountRecoveredTemplate);
                }

                this.logger.LogDebug("Finished recovering user profile... {Hdid}", hdid);
                return new RequestResult<UserProfileModel>
                {
                    ResourcePayload = UserProfileMapUtils.CreateFromDbModel(updateResult.Payload, termsOfServiceResult.ResourcePayload?.Id, this.autoMapper),
                    ResultStatus = ResultType.Success,
                };
            }

            return new RequestResult<UserProfileModel>
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = retrieveResult.Message,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                },
            };
        }

        /// <inheritdoc/>
        public RequestResult<TermsOfServiceModel> GetActiveTermsOfService()
        {
            this.logger.LogDebug("Getting active terms of service...");
            DbResult<LegalAgreement> retVal =
                this.legalAgreementDelegate.GetActiveByAgreementType(LegalAgreementType.TermsOfService);

            return new RequestResult<TermsOfServiceModel>
            {
                ResultStatus = retVal.Status != DbStatusCode.Error ? ResultType.Success : ResultType.Error,
                ResourcePayload = this.autoMapper.Map<TermsOfServiceModel>(retVal.Payload),
                ResultError = retVal.Status != DbStatusCode.Error
                    ? null
                    : new RequestResultError
                    {
                        ResultMessage = retVal.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    },
            };
        }

        /// <inheritdoc/>
        public RequestResult<UserPreferenceModel> UpdateUserPreference(UserPreferenceModel userPreferenceModel)
        {
            this.logger.LogTrace("Updating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);

            UserPreference userPreference = this.autoMapper.Map<UserPreference>(userPreferenceModel);

            DbResult<UserPreference> dbResult = this.userPreferenceDelegate.UpdateUserPreference(userPreference);
            this.logger.LogDebug("Finished updating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);

            RequestResult<UserPreferenceModel> requestResult = new()
            {
                ResourcePayload = this.autoMapper.Map<UserPreferenceModel>(dbResult.Payload),
                ResultStatus = dbResult.Status == DbStatusCode.Updated ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DbStatusCode.Updated
                    ? null
                    : new RequestResultError
                    {
                        ResultMessage = dbResult.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    },
            };
            return requestResult;
        }

        /// <inheritdoc/>
        public RequestResult<UserPreferenceModel> CreateUserPreference(UserPreferenceModel userPreferenceModel)
        {
            this.logger.LogTrace("Creating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);
            UserPreference userPreference = this.autoMapper.Map<UserPreference>(userPreferenceModel);
            DbResult<UserPreference> dbResult = this.userPreferenceDelegate.CreateUserPreference(userPreference);
            this.logger.LogDebug("Finished creating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);

            RequestResult<UserPreferenceModel> requestResult = new()
            {
                ResourcePayload = this.autoMapper.Map<UserPreferenceModel>(dbResult.Payload),
                ResultStatus = dbResult.Status == DbStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DbStatusCode.Created
                    ? null
                    : new RequestResultError
                    {
                        ResultMessage = dbResult.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    },
            };
            return requestResult;
        }

        /// <inheritdoc/>
        public RequestResult<Dictionary<string, UserPreferenceModel>> GetUserPreferences(string hdid)
        {
            this.logger.LogTrace("Getting user preference... {Hdid}", hdid);
            DbResult<IEnumerable<UserPreference>> dbResult = this.userPreferenceDelegate.GetUserPreferences(hdid);
            RequestResult<Dictionary<string, UserPreferenceModel>> requestResult =
                new()
                {
                    ResourcePayload = this.autoMapper.Map<IEnumerable<UserPreferenceModel>>(dbResult.Payload)
                        .ToDictionary(x => x.Preference, x => x),
                    ResultStatus = dbResult.Status == DbStatusCode.Read ? ResultType.Success : ResultType.Error,
                    ResultError = dbResult.Status == DbStatusCode.Read
                        ? null
                        : new RequestResultError
                        {
                            ResultMessage = dbResult.Message,
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        },
                };

            this.logger.LogTrace("Finished getting user preference...{Hdid}", hdid);
            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<PrimitiveRequestResult<bool>> ValidateMinimumAge(string hdid)
        {
            if (this.minPatientAge == 0)
            {
                return new PrimitiveRequestResult<bool>
                {
                    ResourcePayload = true,
                    ResultStatus = ResultType.Success,
                };
            }

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);

            if (patientResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogWarning("Error retrieving patient age... {Hdid}", hdid);
                return new PrimitiveRequestResult<bool>
                {
                    ResultStatus = patientResult.ResultStatus,
                    ResultError = patientResult.ResultError,
                    ResourcePayload = false,
                };
            }

            DateTime birthDate = patientResult.ResourcePayload?.Birthdate ?? default(DateTime);
            return new PrimitiveRequestResult<bool>
            {
                ResultStatus = patientResult.ResultStatus,
                ResourcePayload = birthDate.AddYears(this.minPatientAge) < DateTime.Now,
            };
        }

        /// <inheritdoc/>
        public RequestResult<UserProfileModel> UpdateAcceptedTerms(string hdid, Guid termsOfServiceId)
        {
            RequestResult<UserProfileModel> requestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            DbResult<UserProfile> profileResult = this.userProfileDelegate.GetUserProfile(hdid);
            if (profileResult.Status == DbStatusCode.Read)
            {
                profileResult.Payload.TermsOfServiceId = termsOfServiceId;
                profileResult = this.userProfileDelegate.Update(profileResult.Payload);
                if (profileResult.Status == DbStatusCode.Updated)
                {
                    RequestResult<TermsOfServiceModel> termsOfServiceResult = this.GetActiveTermsOfService();

                    requestResult.ResultStatus = ResultType.Success;
                    requestResult.ResourcePayload = UserProfileMapUtils.CreateFromDbModel(profileResult.Payload, termsOfServiceResult.ResourcePayload?.Id, this.autoMapper);
                }
                else
                {
                    requestResult.ResultError = new()
                    {
                        ResultMessage = "Unable to update the terms of service: DB Error",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    };
                }
            }
            else
            {
                requestResult.ResultError = new()
                {
                    ResultMessage = "Unable to retrieve user profile",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                };
            }

            return requestResult;
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
    }
}
