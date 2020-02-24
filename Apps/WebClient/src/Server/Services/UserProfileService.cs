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
    using System.Diagnostics.Contracts;
    using System.Text.Json;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constant;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="emailDelegate">The email delegate to interact with the DB.</param>
        /// <param name="emailInviteDelegate">The email invite delegate to interact with the DB.</param>
        /// <param name="configuration">The configuration service.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        public UserProfileService(ILogger<UserProfileService> logger, IProfileDelegate profileDelegate, IEmailDelegate emailDelegate, IEmailInviteDelegate emailInviteDelegate, IConfigurationService configuration, IEmailQueueService emailQueueService)
        {
            this.logger = logger;
            this.profileDelegate = profileDelegate;
            this.emailDelegate = emailDelegate;
            this.emailInviteDelegate = emailInviteDelegate;
            this.configurationService = configuration;
            this.emailQueueService = emailQueueService;
        }

        /// <inheritdoc />
        public RequestResult<UserProfile> GetUserProfile(string hdid, DateTime lastLogin)
        {
            this.logger.LogTrace($"Getting user profile... {hdid}");
            DBResult<UserProfile> retVal = this.profileDelegate.GetUserProfile(hdid);
            this.logger.LogDebug($"Finished getting user profile. {JsonSerializer.Serialize(retVal)}");

            this.logger.LogTrace($"Updating user last login... {hdid}");
            retVal.Payload.LastLogin = lastLogin;
            DBResult<UserProfile> updateResult = this.profileDelegate.UpdateUserProfile(retVal.Payload);
            this.logger.LogDebug($"Finished updating user last login. {JsonSerializer.Serialize(updateResult)}");

            return new RequestResult<UserProfile>()
            {
                ResultStatus = retVal.Status != DBStatusCode.Error ? ResultType.Success : ResultType.Error,
                ResultMessage = retVal.Message,
                ResourcePayload = retVal.Payload,
            };
        }

        /// <inheritdoc />
        public RequestResult<UserProfile> CreateUserProfile(CreateUserRequest createProfileRequest, Uri hostUri)
        {
            Contract.Requires(createProfileRequest != null && hostUri != null);
            this.logger.LogTrace($"Creating user profile... {JsonSerializer.Serialize(createProfileRequest)}");

            string registrationStatus = this.configurationService.GetConfiguration().WebClient.RegistrationStatus;

            RequestResult<UserProfile> requestResult = new RequestResult<UserProfile>();

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

            string? email = createProfileRequest.Profile.Email;
            createProfileRequest.Profile.Email = string.Empty;

            createProfileRequest.Profile.CreatedBy = hdid;
            createProfileRequest.Profile.UpdatedBy = hdid;

            DBResult<UserProfile> insertResult = this.profileDelegate.InsertUserProfile(createProfileRequest.Profile);

            if (insertResult.Status == DBStatusCode.Created)
            {
                if (emailInvite != null)
                {
                    // Validates the invite email
                    emailInvite.Validated = true;
                    emailInvite.HdId = hdid;
                    this.emailInviteDelegate.Update(emailInvite);
                }

                if (!string.IsNullOrEmpty(email))
                {
                    this.emailQueueService.QueueNewInviteEmail(hdid, email, hostUri);
                }

                requestResult.ResourcePayload = insertResult.Payload;
                requestResult.ResultStatus = ResultType.Success;
            }

            this.logger.LogDebug($"Finished creating user profile. {JsonSerializer.Serialize(insertResult)}");
            return requestResult;
        }
    }
}
