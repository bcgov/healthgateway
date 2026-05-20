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
    using Hangfire;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Providers;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly IApplicationSettingsService applicationSettingsService;
        private readonly IGatewayApiMappingService mappingService;
        private readonly ICryptoDelegate cryptoDelegate;
        private readonly IEmailQueueService emailQueueService;
        private readonly ILegalAgreementService legalAgreementService;
        private readonly ILogger logger;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly int minPatientAge;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IPatientService patientService;
        private readonly IUserPreferenceService userPreferenceService;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly int userProfileHistoryRecordLimit;
        private readonly IPatientRepository patientRepository;
        private readonly bool accountsChangeFeedEnabled;
        private readonly bool notificationsChangeFeedEnabled;
        private readonly IMessagingVerificationService messagingVerificationService;
        private readonly IOutboxStoreService outboxStoreService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IGatewayDbContextTransactionProvider transactionProvider;
        private readonly EmailTemplateConfig emailTemplateConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="patientService">The patient service.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="notificationSettingsService">The Notifications Settings service.</param>
        /// <param name="userProfileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="userPreferenceService">The user preference service.</param>
        /// <param name="legalAgreementService">The legal agreement service.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="mappingService">The inject automapper provider.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        /// <param name="applicationSettingsService">The injected Application Settings service.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="messagingVerificationService">The injected message verification service.</param>
        /// <param name="outboxStoreService">The injected outbox store service.</param>
        /// <param name="backgroundJobClient">Hangfire background job client.</param>
        /// <param name="transactionProvider">
        /// Provides database transaction and persistence operations for the current request
        /// scope.
        /// </param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public UserProfileService(
            ILogger<UserProfileService> logger,
            IPatientService patientService,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService,
            IUserProfileDelegate userProfileDelegate,
            IUserPreferenceService userPreferenceService,
            ILegalAgreementService legalAgreementService,
            IMessagingVerificationDelegate messageVerificationDelegate,
            ICryptoDelegate cryptoDelegate,
            IConfiguration configuration,
            IGatewayApiMappingService mappingService,
            IAuthenticationDelegate authenticationDelegate,
            IApplicationSettingsService applicationSettingsService,
            IPatientRepository patientRepository,
            IMessagingVerificationService messagingVerificationService,
            IOutboxStoreService outboxStoreService,
            IBackgroundJobClient backgroundJobClient,
            IGatewayDbContextTransactionProvider transactionProvider)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
            this.userProfileDelegate = userProfileDelegate;
            this.userPreferenceService = userPreferenceService;
            this.legalAgreementService = legalAgreementService;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.cryptoDelegate = cryptoDelegate;
            this.userProfileHistoryRecordLimit = configuration.GetSection(WebClientConfigSection)
                .GetValue(UserProfileHistoryRecordLimitKey, 4);
            this.minPatientAge = configuration.GetSection(WebClientConfigSection).GetValue(MinPatientAgeKey, 12);
            this.mappingService = mappingService;
            this.authenticationDelegate = authenticationDelegate;
            this.applicationSettingsService = applicationSettingsService;
            this.patientRepository = patientRepository;
            this.messagingVerificationService = messagingVerificationService;
            this.outboxStoreService = outboxStoreService;
            this.backgroundJobClient = backgroundJobClient;
            this.transactionProvider = transactionProvider;
            ChangeFeedOptions? changeFeedConfiguration = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>();
            this.accountsChangeFeedEnabled = changeFeedConfiguration?.Accounts.Enabled ?? false;
            this.notificationsChangeFeedEnabled = changeFeedConfiguration?.Notifications.Enabled ?? false;
            this.emailTemplateConfig = configuration.GetSection(EmailTemplateConfig.ConfigurationSectionKey).Get<EmailTemplateConfig>() ?? new();
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> GetUserProfileAsync(string hdid, DateTime jwtAuthTime, CancellationToken ct = default)
        {
            UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, true, ct);
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
                userProfile.LastLoginDateTime = jwtAuthTime;
                userProfile.LastLoginClientCode = this.authenticationDelegate.FetchAuthenticatedUserClientType();

                // Update user year of birth.
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(hdid, ct: ct);
                DateTime? birthDate = patientResult.ResourcePayload?.Birthdate;
                userProfile.YearOfBirth = birthDate?.Year;

                // Try to update user profile with last login time; ignore any failures
                this.logger.LogDebug("Updating last login date and year of birth");
                await this.userProfileDelegate.UpdateAsync(userProfile, ct: ct);
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
                MessagingVerification? emailInvite =
                    await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
                userProfileModel.Email = emailInvite?.Email?.To;
            }

            if (!userProfileModel.IsSmsNumberVerified)
            {
                MessagingVerification? smsInvite =
                    await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Sms, ct);
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
            RequestResult<UserProfileModel>? validationResult = await this.ValidateUserProfileAsync(createProfileRequest, ct);
            if (validationResult != null)
            {
                return validationResult;
            }

            string hdid = createProfileRequest.Profile.HdId;

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(hdid, ct: ct);
            DateTime? birthDate = patientResult.ResourcePayload?.Birthdate;

            // Begin transaction for all database updates
            await using IDbContextTransaction transaction =
                await this.transactionProvider.BeginTransactionAsync(ct);

            string? requestedEmail = createProfileRequest.Profile.Email;
            bool isEmailVerified = !string.IsNullOrWhiteSpace(requestedEmail) && requestedEmail.Equals(jwtEmailAddress, StringComparison.OrdinalIgnoreCase);

            // Create profile
            UserProfile newProfile = new()
            {
                HdId = hdid,
                IdentityManagementId = null,
                TermsOfServiceId = createProfileRequest.Profile.TermsOfServiceId,
                Email = isEmailVerified ? requestedEmail : string.Empty,
                SmsNumber = null,
                CreatedBy = hdid,
                UpdatedBy = hdid,
                LastLoginDateTime = jwtAuthTime,
                EncryptionKey = this.cryptoDelegate.GenerateKey(),
                YearOfBirth = birthDate?.Year,
                LastLoginClientCode = this.authenticationDelegate.FetchAuthenticatedUserClientType(),
            };

            await this.userProfileDelegate.InsertUserProfileAsync(newProfile, false, ct);

            if (this.accountsChangeFeedEnabled)
            {
                // Store an event indicating the account was created.
                await this.outboxStoreService.QueueAccountCreatedEventAsync(hdid, false, ct);
            }

            MessagingVerification? emailVerification = null;

            // Add email verification
            if (!string.IsNullOrWhiteSpace(requestedEmail))
            {
                emailVerification = await this.messagingVerificationService.AddEmailVerificationAsync(hdid, requestedEmail, isEmailVerified, false, ct);
                switch (isEmailVerified)
                {
                    case true when this.notificationsChangeFeedEnabled:
                        // Store an event indicating the email channel has been verified.
                        await this.outboxStoreService.QueueEmailVerificationEventAsync(hdid, requestedEmail, false, ct);
                        break;
                    case false:
                        // Persist the email for background processing.
                        await this.emailQueueService.QueueNewEmailAsync(emailVerification.Email, false, ct);
                        break;
                }
            }

            string? requestedSmsNumber = createProfileRequest.Profile.SmsNumber;
            NotificationSettingsRequest notificationRequest = new(newProfile, isEmailVerified ? requestedEmail : string.Empty, requestedSmsNumber);

            // Add SMS verification
            if (!string.IsNullOrWhiteSpace(requestedSmsNumber))
            {
                MessagingVerification smsVerification = await this.messagingVerificationService.AddSmsVerificationAsync(hdid, requestedSmsNumber, false, ct);
                notificationRequest.SmsVerificationCode = smsVerification.SmsValidationCode;
            }

            try
            {
                // Persist changes within transaction
                await this.transactionProvider.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (DbUpdateConcurrencyException e)
            {
                this.logger.LogError(e, "Concurrency error saving transaction");
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, e.Message);
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, "Database error persisting changes");
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, e.Message);
            }

            // Dispatch outbox events after commit
            this.logger.LogDebug("Dispatching events after commit");
            this.backgroundJobClient.Enqueue<DbOutboxStore>(store =>
                store.DispatchOutboxItemsAsync(ct));

            if (emailVerification != null && !isEmailVerified)
            {
                // Queue a background job to send the email.
                this.backgroundJobClient.Enqueue<IEmailJob>(j => j.SendEmailAsync(emailVerification.Email!.Id, ct));
            }

            // Update notification settings after commit
            await this.notificationSettingsService.QueueNotificationSettingsAsync(notificationRequest, ct);

            UserProfileModel userProfileModel = await this.BuildUserProfileModelAsync(newProfile, ct: ct);
            userProfileModel.Email = requestedEmail;
            userProfileModel.IsEmailVerified = isEmailVerified;
            userProfileModel.SmsNumber = requestedSmsNumber;

            return RequestResultFactory.Success(userProfileModel);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> CloseUserProfileAsync(string hdid, Guid userId, CancellationToken ct = default)
        {
            UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct: ct);
            if (userProfile == null)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, ErrorMessages.UserProfileNotFound);
            }

            if (userProfile.ClosedDateTime != null)
            {
                this.logger.LogDebug("User profile is already closed");
                return RequestResultFactory.Success(await this.BuildUserProfileModelAsync(userProfile, ct: ct));
            }

            // Begin transaction for all database updates
            await using IDbContextTransaction transaction =
                await this.transactionProvider.BeginTransactionAsync(ct);

            userProfile.ClosedDateTime = DateTime.UtcNow;
            userProfile.IdentityManagementId = userId;
            await this.userProfileDelegate.UpdateAsync(userProfile, false, ct);

            Email? email = null;

            if (!string.IsNullOrWhiteSpace(userProfile.Email))
            {
                Dictionary<string, string> keyValues = new() { [EmailTemplateVariable.Host] = this.emailTemplateConfig.Host };
                email = await this.emailQueueService.QueueNewEmailAndReturnAsync(
                    userProfile.Email,
                    EmailTemplateName.AccountClosedTemplate,
                    keyValues,
                    false,
                    ct);
            }

            try
            {
                // Persist changes within transaction
                await this.transactionProvider.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (DbUpdateConcurrencyException e)
            {
                this.logger.LogError(e, "Concurrency error saving transaction");
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, e.Message);
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, "Database error persisting changes");
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, e.Message);
            }

            // Dispatch email event after commit
            if (email is not null && email.Id != Guid.Empty)
            {
                this.backgroundJobClient.Enqueue<IEmailJob>(j => j.SendEmailAsync(email.Id, ct));
            }

            return RequestResultFactory.Success(await this.BuildUserProfileModelAsync(userProfile, ct: ct));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> RecoverUserProfileAsync(string hdid, CancellationToken ct = default)
        {
            UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct: ct);
            if (userProfile == null)
            {
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, ErrorMessages.UserProfileNotFound);
            }

            if (userProfile.ClosedDateTime == null)
            {
                this.logger.LogDebug("User profile is not closed");
                return RequestResultFactory.Success(await this.BuildUserProfileModelAsync(userProfile, ct: ct));
            }

            // Begin transaction for all database updates
            await using IDbContextTransaction transaction =
                await this.transactionProvider.BeginTransactionAsync(ct);

            // Remove values set for deletion
            userProfile.ClosedDateTime = null;
            userProfile.IdentityManagementId = null;

            Email? email = null;

            if (!string.IsNullOrWhiteSpace(userProfile.Email))
            {
                Dictionary<string, string> keyValues = new() { [EmailTemplateVariable.Host] = this.emailTemplateConfig.Host };
                email = await this.emailQueueService.QueueNewEmailAndReturnAsync(
                    userProfile.Email,
                    EmailTemplateName.AccountRecoveredTemplate,
                    keyValues,
                    false,
                    ct);
            }

            try
            {
                // Persist changes within transaction
                await this.transactionProvider.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (DbUpdateConcurrencyException e)
            {
                this.logger.LogError(e, "Concurrency error saving transaction");
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, e.Message);
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, "Database error persisting changes");
                return RequestResultFactory.ServiceError<UserProfileModel>(ErrorType.CommunicationInternal, ServiceType.Database, e.Message);
            }

            // Dispatch email event after commit
            if (email is not null && email.Id != Guid.Empty)
            {
                this.backgroundJobClient.Enqueue<IEmailJob>(j => j.SendEmailAsync(email.Id, ct));
            }

            return RequestResultFactory.Success(await this.BuildUserProfileModelAsync(userProfile, ct: ct));
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
                return RequestResultFactory.Error(false, patientResult.ResultError);
            }

            bool isValid = AgeRangeValidator.IsValid(patientResult.ResourcePayload.Birthdate, this.minPatientAge);

            return RequestResultFactory.Success(isValid);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>> UpdateAcceptedTermsAsync(string hdid, Guid termsOfServiceId, CancellationToken ct = default)
        {
            UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(hdid, ct: ct);
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
                this.logger.LogDebug("User profile did not pass age validation");
                return RequestResultFactory.Error<UserProfileModel>(ErrorType.InvalidState, "Patient under minimum age");
            }

            // Validate UserProfile inputs
            if (!await UserProfileValidator.ValidateUserProfileSmsNumberAsync(createProfileRequest.Profile.SmsNumber, ct))
            {
                this.logger.LogDebug("User profile did not pass SMS validation");
                return RequestResultFactory.Error<UserProfileModel>(ErrorType.SmsInvalid, "Profile values entered are invalid");
            }

            return null;
        }

        private async Task<UserProfileModel> BuildUserProfileModelAsync(UserProfile userProfile, UserProfileHistory[]? profileHistoryCollection = null, CancellationToken ct = default)
        {
            Guid? termsOfServiceId = await this.legalAgreementService.GetActiveLegalAgreementId(LegalAgreementType.TermsOfService, ct);
            UserProfileModel userProfileModel = this.mappingService.MapToUserProfileModel(userProfile, termsOfServiceId);

            DateTime? latestTourChangeDateTime = await this.applicationSettingsService.GetLatestTourChangeDateTimeAsync(ct);
            userProfileModel.HasTourUpdated = profileHistoryCollection != null &&
                                              profileHistoryCollection.Length != 0 &&
                                              latestTourChangeDateTime != null &&
                                              profileHistoryCollection.Max(x => x.LastLoginDateTime) < latestTourChangeDateTime;

            userProfileModel.BlockedDataSources = await this.patientRepository.GetDataSourcesAsync(userProfile.HdId, ct);
            userProfileModel.Preferences = await this.userPreferenceService.GetUserPreferencesAsync(userProfileModel.HdId, ct);

            return userProfileModel;
        }
    }
}
