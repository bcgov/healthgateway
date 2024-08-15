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
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class UserEmailServiceV2 : IUserEmailServiceV2
    {
        private const int MaxVerificationAttempts = 5;

        private readonly ILogger logger;
        private readonly IMessagingVerificationDelegate messageVerificationDelegate;
        private readonly IMessagingVerificationService messagingVerificationService;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IJobService jobService;
        private readonly bool notificationsChangeFeedEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailServiceV2"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="messageVerificationDelegate">The message verification delegate to interact with the DB.</param>
        /// <param name="messagingVerificationService">The messaging verification service.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="jobService">The injected job service.</param>
        /// <param name="configuration">Configuration settings.</param>
        public UserEmailServiceV2(
            ILogger<UserEmailServiceV2> logger,
            IMessagingVerificationDelegate messageVerificationDelegate,
            IMessagingVerificationService messagingVerificationService,
            IUserProfileDelegate profileDelegate,
            IJobService jobService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.messageVerificationDelegate = messageVerificationDelegate;
            this.messagingVerificationService = messagingVerificationService;
            this.profileDelegate = profileDelegate;
            this.jobService = jobService;
            this.notificationsChangeFeedEnabled = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>()?.Notifications.Enabled ?? false;
        }

        /// <inheritdoc/>
        public async Task<bool> VerifyEmailAddressAsync(string hdid, Guid inviteKey, CancellationToken ct = default)
        {
            this.logger.LogTrace("Verifying email address... {InviteKey}", inviteKey);

            MessagingVerification? matchingVerification = await this.messageVerificationDelegate.GetLastByInviteKeyAsync(inviteKey, ct);
            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            if (matchingVerification == null || matchingVerification.UserProfileId != hdid || matchingVerification.Deleted)
            {
                this.logger.LogDebug("Invalid email verification");
                await this.IncrementEmailVerificationAttempts(hdid, ct);
                return false;
            }

            if (matchingVerification.VerificationAttempts >= MaxVerificationAttempts || matchingVerification.ExpireDate < DateTime.UtcNow)
            {
                this.logger.LogDebug("Email verification expired");
                return false;
            }

            if (matchingVerification.Validated)
            {
                this.logger.LogDebug("Email already verified");
                throw new AlreadyExistsException("Email already verified");
            }

            await this.SetAddressValidated(userProfile, matchingVerification, true, ct);
            await this.NotifyVerificationSuccessful(hdid, matchingVerification.Email!.To, ct);
            await this.QueueNotificationSettingsRequest(userProfile, ct);

            this.logger.LogDebug("Email verified");
            return true;
        }

        /// <inheritdoc/>
        public async Task UpdateEmailAddressAsync(string hdid, string emailAddress, CancellationToken ct = default)
        {
            this.logger.LogTrace("Updating user email...");
            bool isEmpty = string.IsNullOrEmpty(emailAddress);
            Guid inviteKey = Guid.NewGuid();

            await new OptionalEmailAddressValidator().ValidateAndThrowAsync(emailAddress, ct);
            UserProfile userProfile = await this.profileDelegate.GetUserProfileAsync(hdid, true, ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            // expire previous email verification in DB without committing changes
            MessagingVerification? lastEmailVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
            if (lastEmailVerification != null)
            {
                this.logger.LogInformation("Expiring old email validation for user {Hdid}", hdid);
                await this.messageVerificationDelegate.ExpireAsync(lastEmailVerification, isEmpty, false, ct);
                if (string.Equals(emailAddress, lastEmailVerification.Email?.To, StringComparison.OrdinalIgnoreCase))
                {
                    // reuse same invite key if the last verification was for the same email address
                    inviteKey = lastEmailVerification.InviteKey!.Value;
                }
            }

            // add new messaging verification to DB without committing changes
            MessagingVerification? messagingVerification = null;
            if (!isEmpty)
            {
                messagingVerification = await this.messagingVerificationService.GenerateMessagingVerificationAsync(hdid, emailAddress, inviteKey, false, ct);
                await this.messageVerificationDelegate.InsertAsync(messagingVerification, false, ct);
            }

            this.logger.LogInformation("Removing email from user {Hdid}", hdid);
            userProfile.Email = null;
            userProfile.BetaFeatureCodes = [];

            // update user profile in DB and commit changes
            DbResult<UserProfile> dbResult = await this.profileDelegate.UpdateAsync(userProfile, true, ct);
            if (dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }

            if (messagingVerification != null)
            {
                this.logger.LogInformation("Sending new email verification for user {Hdid}", hdid);
                await this.jobService.SendEmailAsync(messagingVerification.Email, true, ct);
            }

            await this.QueueNotificationSettingsRequest(userProfile, ct);

            this.logger.LogDebug("Finished updating user email");
        }

        private async Task IncrementEmailVerificationAttempts(string hdid, CancellationToken ct)
        {
            MessagingVerification? latestEmailVerification = await this.messageVerificationDelegate.GetLastForUserAsync(hdid, MessagingVerificationType.Email, ct);
            if (latestEmailVerification is { Validated: false })
            {
                latestEmailVerification.VerificationAttempts++;
                await this.messageVerificationDelegate.UpdateAsync(latestEmailVerification, true, ct);
            }
        }

        private async Task SetAddressValidated(UserProfile userProfile, MessagingVerification matchingVerification, bool commit = true, CancellationToken ct = default)
        {
            // update Validated value in MessagingVerification
            matchingVerification.Validated = true;
            await this.messageVerificationDelegate.UpdateAsync(matchingVerification, false, ct);

            // add email address to UserProfile
            userProfile.Email = matchingVerification.Email!.To;
            DbResult<UserProfile> dbResult = await this.profileDelegate.UpdateAsync(userProfile, commit, ct);
            if (commit && dbResult.Status != DbStatusCode.Updated)
            {
                throw new DatabaseException(dbResult.Message);
            }
        }

        private async Task NotifyVerificationSuccessful(string hdid, string emailAddress, CancellationToken ct)
        {
            if (this.notificationsChangeFeedEnabled)
            {
                await this.jobService.NotifyEmailVerificationAsync(hdid, emailAddress, ct);
            }
        }

        private async Task QueueNotificationSettingsRequest(UserProfile userProfile, CancellationToken ct)
        {
            await this.jobService.PushNotificationSettingsToPhsaAsync(userProfile, userProfile.Email, userProfile.SmsNumber, ct: ct);
        }
    }
}
