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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="patientDetailsService">The injected patient details service.</param>
    /// <param name="emailQueueService">The injected service to queue emails.</param>
    /// <param name="userProfileDelegate">The injected user profile database delegate.</param>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="authenticationDelegate">The injected authentication delegate.</param>
    /// <param name="userProfileModelService">The injected user profile model service.</param>
    public class UserProfileServiceV2(
        ILogger<UserProfileServiceV2> logger,
        IPatientDetailsService patientDetailsService,
        IEmailQueueService emailQueueService,
        IUserProfileDelegate userProfileDelegate,
        IConfiguration configuration,
        IAuthenticationDelegate authenticationDelegate,
        IUserProfileModelService userProfileModelService) : IUserProfileServiceV2
    {
        private const string UserProfileHistoryRecordLimitKey = "UserProfileHistoryRecordLimit";
        private const string WebClientConfigSection = "WebClient";
        private readonly EmailTemplateConfig emailTemplateConfig = configuration.GetSection(EmailTemplateConfig.ConfigurationSectionKey).Get<EmailTemplateConfig>() ?? new();
        private readonly int userProfileHistoryRecordLimit = configuration.GetSection(WebClientConfigSection).GetValue(UserProfileHistoryRecordLimitKey, 4);

        /// <inheritdoc/>
        public async Task<UserProfileModel> GetUserProfileAsync(string hdid, DateTime jwtAuthTime, CancellationToken ct = default)
        {
            logger.LogTrace("Getting user profile... {Hdid}", hdid);
            UserProfile? userProfile = await userProfileDelegate.GetUserProfileAsync(hdid, true, ct);
            logger.LogDebug("Finished getting user profile...{Hdid}", hdid);

            if (userProfile == null)
            {
                return new UserProfileModel();
            }

            DateTime previousLastLogin = userProfile.LastLoginDateTime;
            if (DateTime.Compare(previousLastLogin, jwtAuthTime) != 0)
            {
                logger.LogTrace("Updating user last login and year of birth... {Hdid}", hdid);
                userProfile.LastLoginDateTime = jwtAuthTime;
                userProfile.LastLoginClientCode = authenticationDelegate.FetchAuthenticatedUserClientType();

                // Update user year of birth.
                PatientDetails patient = await patientDetailsService.GetPatientAsync(hdid, ct: ct);
                userProfile.YearOfBirth = patient.Birthdate.Year;

                // Try to update user profile with last login time; ignore any failures
                await userProfileDelegate.UpdateAsync(userProfile, ct: ct);

                logger.LogDebug("Finished updating user last login and year of birth... {Hdid}", hdid);
            }

            UserProfileModel userProfileModel = await userProfileModelService.BuildUserProfileModelAsync(userProfile, this.userProfileHistoryRecordLimit, ct);

            return userProfileModel;
        }

        /// <inheritdoc/>
        public async Task CloseUserProfileAsync(string hdid, CancellationToken ct = default)
        {
            logger.LogTrace("Closing user profile... {Hdid}", hdid);

            UserProfile userProfile = await userProfileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            if (userProfile.ClosedDateTime != null)
            {
                logger.LogTrace("Profile already closed");
                return;
            }

            // Mark profile for deletion
            userProfile.ClosedDateTime = DateTime.UtcNow;
            userProfile.IdentityManagementId = new(authenticationDelegate.FetchAuthenticatedUserId());
            DbResult<UserProfile> dbResult = await userProfileDelegate.UpdateAsync(userProfile, ct: ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }

            await this.SendEmailAsync(dbResult.Payload.Email, EmailTemplateName.AccountClosedTemplate, ct);
        }

        /// <inheritdoc/>
        public async Task RecoverUserProfileAsync(string hdid, CancellationToken ct = default)
        {
            logger.LogTrace("Recovering user profile... {Hdid}", hdid);
            UserProfile userProfile = await userProfileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            if (userProfile.ClosedDateTime == null)
            {
                logger.LogTrace("Profile already is active, recover not needed");
                return;
            }

            // Unmark profile for deletion
            userProfile.ClosedDateTime = null;
            userProfile.IdentityManagementId = null;
            DbResult<UserProfile> dbResult = await userProfileDelegate.UpdateAsync(userProfile, true, ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }

            await this.SendEmailAsync(dbResult.Payload.Email, EmailTemplateName.AccountRecoveredTemplate, ct);
        }

        /// <inheritdoc/>
        public async Task UpdateAcceptedTermsAsync(string hdid, Guid termsOfServiceId, CancellationToken ct = default)
        {
            UserProfile userProfile = await userProfileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);
            userProfile.TermsOfServiceId = termsOfServiceId;

            DbResult<UserProfile> result = await userProfileDelegate.UpdateAsync(userProfile, ct: ct);
            if (result.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(result.Message);
            }
        }

        private async Task SendEmailAsync(string? emailAddress, string emailTemplateName, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                Dictionary<string, string> keyValues = new() { [EmailTemplateVariable.Host] = this.emailTemplateConfig.Host };
                await emailQueueService.QueueNewEmailAsync(emailAddress, emailTemplateName, keyValues, ct: ct);
            }
        }
    }
}
