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
    using System.Diagnostics.Contracts;
    using System.Text.Json;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Constant;
    using HealthGateway.WebClient.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class UserProfileService : IUserProfileService
    {
        private readonly ILogger logger;
        private readonly IProfileDelegate profileDelegate;
        private readonly IEmailDelegate emailDelegate;
        private readonly IEmailInviteDelegate emailInviteDelegate;
        private readonly IConfigurationService configurationService;
        private readonly IEmailQueueService emailQueueService;
        private readonly ILegalAgreementDelegate legalAgreementDelegate;
        private readonly ICryptoDelegate cryptoDelegate;

#pragma warning disable SA1310 // Disable _ in variable name
        private const string HOST_TEMPLATE_VARIABLE = "host";
#pragma warning restore SA1310 // Restore warnings

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="emailDelegate">The email delegate to interact with the DB.</param>
        /// <param name="emailInviteDelegate">The email invite delegate to interact with the DB.</param>
        /// <param name="configuration">The configuration service.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="legalAgreementDelegate">The terms of service delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        public UserProfileService(
            ILogger<UserProfileService> logger,
            IProfileDelegate profileDelegate,
            IEmailDelegate emailDelegate,
            IEmailInviteDelegate emailInviteDelegate,
            IConfigurationService configuration,
            IEmailQueueService emailQueueService,
            ILegalAgreementDelegate legalAgreementDelegate,
            ICryptoDelegate cryptoDelegate)
        {
            this.logger = logger;
            this.profileDelegate = profileDelegate;
            this.emailDelegate = emailDelegate;
            this.emailInviteDelegate = emailInviteDelegate;
            this.configurationService = configuration;
            this.emailQueueService = emailQueueService;
            this.legalAgreementDelegate = legalAgreementDelegate;
            this.cryptoDelegate = cryptoDelegate;
        }

        /// <inheritdoc />
        public RequestResult<UserProfileModel> GetUserProfile(string hdid, DateTime? lastLogin = null)
        {
            this.logger.LogTrace($"Getting user profile... {hdid}");
            DBResult<UserProfile> retVal = this.profileDelegate.GetUserProfile(hdid);
            this.logger.LogDebug($"Finished getting user profile. {JsonSerializer.Serialize(retVal)}");

            if (retVal.Status == DBStatusCode.NotFound)
            {
                return new RequestResult<UserProfileModel>()
                {
                    ResultStatus = retVal.Status != DBStatusCode.Error ? ResultType.Success : ResultType.Error,
                    ResultMessage = retVal.Message,
                    ResourcePayload = null,
                };
            }

            DateTime? previousLastLogin = retVal.Payload.LastLoginDateTime;
            if (lastLogin.HasValue)
            {
                this.logger.LogTrace($"Updating user last login... {hdid}");
                retVal.Payload.LastLoginDateTime = lastLogin;
                DBResult<UserProfile> updateResult = this.profileDelegate.Update(retVal.Payload);
                this.logger.LogDebug($"Finished updating user last login. {JsonSerializer.Serialize(updateResult)}");
            }

            RequestResult<TermsOfServiceModel> termsOfServiceResult = this.GetActiveTermsOfService();

            UserProfileModel userProfile = UserProfileModel.CreateFromDbModel(retVal.Payload);
            userProfile.HasTermsOfServiceUpdated =
                previousLastLogin.HasValue &&
                termsOfServiceResult.ResourcePayload?.EffectiveDate > previousLastLogin.Value;

            return new RequestResult<UserProfileModel>()
            {
                ResultStatus = retVal.Status != DBStatusCode.Error ? ResultType.Success : ResultType.Error,
                ResultMessage = retVal.Message,
                ResourcePayload = userProfile,
            };
        }

        /// <inheritdoc />
        public RequestResult<UserProfileModel> CreateUserProfile(CreateUserRequest createProfileRequest, Uri hostUri)
        {
            Contract.Requires(createProfileRequest != null && hostUri != null);
            this.logger.LogTrace($"Creating user profile... {JsonSerializer.Serialize(createProfileRequest)}");

            string registrationStatus = this.configurationService.GetConfiguration().WebClient.RegistrationStatus;

            RequestResult<UserProfileModel> requestResult = new RequestResult<UserProfileModel>();

            if (registrationStatus == RegistrationStatus.Closed)
            {
                requestResult.ResultStatus = ResultType.Error;
                requestResult.ResultMessage = "Registration is closed";
                this.logger.LogWarning($"Registration is closed. {JsonSerializer.Serialize(createProfileRequest)}");
                return requestResult;
            }

            string hdid = createProfileRequest.Profile.HdId;
            EmailInvite emailInvite = null;
            if (registrationStatus == RegistrationStatus.InviteOnly)
            {
                if (!Guid.TryParse(createProfileRequest.InviteCode, out Guid inviteKey))
                {
                    requestResult.ResultStatus = ResultType.Error;
                    requestResult.ResultMessage = "Invalid email invite";
                    this.logger.LogWarning($"Invalid email invite code. {JsonSerializer.Serialize(createProfileRequest)}");
                    return requestResult;
                }

                emailInvite = this.emailInviteDelegate.GetByInviteKey(inviteKey);
                bool hdidIsValid = string.IsNullOrEmpty(emailInvite.HdId) || (emailInvite.HdId == createProfileRequest.Profile.HdId);

                // Fails if...
                // Email invite not found or
                // Email invite was already validated or
                // Email invite must have a blank/null HDID or be the same as the one in the request
                // Email address doesn't match the invite
                if (emailInvite == null ||
                    emailInvite.Validated ||
                    !hdidIsValid ||
                    !emailInvite.Email.To.Equals(createProfileRequest.Profile.Email, StringComparison.CurrentCultureIgnoreCase))
                {
                    requestResult.ResultStatus = ResultType.Error;
                    requestResult.ResultMessage = "Invalid email invite";
                    this.logger.LogWarning($"Invalid email invite. {JsonSerializer.Serialize(createProfileRequest)}");
                    return requestResult;
                }
            }

            string? requestedEmail = createProfileRequest.Profile.Email;
            UserProfile newProfile = createProfileRequest.Profile;
            newProfile.Email = string.Empty;
            newProfile.CreatedBy = hdid;
            newProfile.UpdatedBy = hdid;
            newProfile.EncryptionKey = this.cryptoDelegate.GenerateKey();

            DBResult<UserProfile> insertResult = this.profileDelegate.InsertUserProfile(newProfile);

            if (insertResult.Status == DBStatusCode.Created)
            {
                if (emailInvite != null)
                {
                    // Validates the invite email
                    emailInvite.Validated = true;
                    emailInvite.HdId = hdid;
                    this.emailInviteDelegate.Update(emailInvite);
                }

                if (!string.IsNullOrWhiteSpace(requestedEmail))
                {
                    this.emailQueueService.QueueNewInviteEmail(hdid, requestedEmail, hostUri);
                }

                requestResult.ResourcePayload = UserProfileModel.CreateFromDbModel(insertResult.Payload);
                requestResult.ResultStatus = ResultType.Success;
            }

            this.logger.LogDebug($"Finished creating user profile. {JsonSerializer.Serialize(insertResult)}");
            return requestResult;
        }

        /// <inheritdoc />
        public RequestResult<UserProfileModel> CloseUserProfile(string hdid, Guid userId, string hostUrl)
        {
            this.logger.LogTrace($"Closing user profile... {hdid}");

            string registrationStatus = this.configurationService.GetConfiguration().WebClient.RegistrationStatus;

            RequestResult<UserProfileModel> requestResult = new RequestResult<UserProfileModel>();

            DBResult<UserProfile> retrieveResult = this.profileDelegate.GetUserProfile(hdid);

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
                DBResult<UserProfile> updateResult = this.profileDelegate.Update(profile);
                if (!string.IsNullOrWhiteSpace(profile.Email))
                {
                    Dictionary<string, string> keyValues = new Dictionary<string, string>();
                    keyValues.Add(HOST_TEMPLATE_VARIABLE, hostUrl);
                    this.emailQueueService.QueueNewEmail(profile.Email, EmailTemplateName.ACCOUNT_CLOSED, keyValues);
                }

                requestResult.ResourcePayload = UserProfileModel.CreateFromDbModel(updateResult.Payload);
                requestResult.ResultStatus = ResultType.Success;
                this.logger.LogDebug($"Finished closing user profile. {JsonSerializer.Serialize(updateResult)}");
            }

            return requestResult;
        }

        /// <inheritdoc />
        public RequestResult<UserProfileModel> RecoverUserProfile(string hdid, string hostUrl)
        {
            this.logger.LogTrace($"Recovering user profile... {hdid}");

            string registrationStatus = this.configurationService.GetConfiguration().WebClient.RegistrationStatus;

            RequestResult<UserProfileModel> requestResult = new RequestResult<UserProfileModel>();

            DBResult<UserProfile> retrieveResult = this.profileDelegate.GetUserProfile(hdid);

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
                DBResult<UserProfile> updateResult = this.profileDelegate.Update(profile);
                if (!string.IsNullOrWhiteSpace(profile.Email))
                {
                    Dictionary<string, string> keyValues = new Dictionary<string, string>();
                    keyValues.Add(HOST_TEMPLATE_VARIABLE, hostUrl);
                    this.emailQueueService.QueueNewEmail(profile.Email, EmailTemplateName.ACCOUNT_RECOVERED, keyValues);
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
            this.logger.LogTrace($"Getting active terms of service...");
            DBResult<LegalAgreement> retVal = this.legalAgreementDelegate.GetActiveByAgreementType(AgreementType.TermsofService);
            this.logger.LogDebug($"Finished getting terms of service. {JsonSerializer.Serialize(retVal)}");

            return new RequestResult<TermsOfServiceModel>()
            {
                ResultStatus = retVal.Status != DBStatusCode.Error ? ResultType.Success : ResultType.Error,
                ResultMessage = retVal.Message,
                ResourcePayload = TermsOfServiceModel.CreateFromDbModel(retVal.Payload),
            };
        }
    }
}
