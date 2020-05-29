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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class UserEmailService : IUserEmailService
    {
        private readonly ILogger logger;
        private readonly IMessagingVerificationDelegate emailInviteDelegate;
        private readonly IProfileDelegate profileDelegate;
        private readonly IEmailQueueService emailQueueService;
        private readonly INotificationSettingsService notificationSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="emailInviteDelegate">The email invite delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        /// <param name="notificationSettingsService">Notification settings delegate.</param>
        public UserEmailService(ILogger<UserEmailService> logger,
            IMessagingVerificationDelegate emailInviteDelegate,
            IProfileDelegate profileDelegate,
            IEmailQueueService emailQueueService,
            INotificationSettingsService notificationSettingsService)
        {
            this.logger = logger;
            this.emailInviteDelegate = emailInviteDelegate;
            this.profileDelegate = profileDelegate;
            this.emailQueueService = emailQueueService;
            this.notificationSettingsService = notificationSettingsService;
        }

        /// <inheritdoc />
        public bool ValidateEmail(string hdid, Guid inviteKey, string bearerToken)
        {
            this.logger.LogTrace($"Validating email... {inviteKey}");
            bool retVal = false;
            MessagingVerification emailInvite = this.emailInviteDelegate.GetByInviteKey(inviteKey);

            if (emailInvite != null &&
                emailInvite.HdId == hdid &&
                !emailInvite.Validated &&
                emailInvite.ExpireDate >= DateTime.UtcNow)
            {
                emailInvite.Validated = true;
                this.emailInviteDelegate.Update(emailInvite);
                UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
                userProfile.Email = emailInvite.Email!.To; // Gets the user email from the email sent.
                this.profileDelegate.Update(userProfile);
                retVal = true;

                // Update the notification settings
                this.UpdateNotificationSettings(userProfile, bearerToken);
            }

            this.logger.LogDebug($"Finished validating email: {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public MessagingVerification RetrieveLastInvite(string hdid)
        {
            this.logger.LogTrace($"Retrieving last invite for {hdid}");
            MessagingVerification emailInvite = this.emailInviteDelegate.GetLastForUser(hdid);
            this.logger.LogDebug($"Finished retrieving email: {JsonConvert.SerializeObject(emailInvite)}");
            return emailInvite;
        }

        /// <inheritdoc />
        public bool UpdateUserEmail(string hdid, string email, Uri hostUri, string bearerToken)
        {
            this.logger.LogTrace($"Updating user email...");
            UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
            MessagingVerification emailInvite = this.RetrieveLastInvite(hdid);

            this.logger.LogInformation($"Removing email from user ${hdid}");
            userProfile.Email = null;
            this.profileDelegate.Update(userProfile);

            // Update the notification settings
            this.UpdateNotificationSettings(userProfile, bearerToken);

            if (emailInvite != null && !emailInvite.Validated && emailInvite.ExpireDate >= DateTime.UtcNow)
            {
                this.logger.LogInformation($"Expiring old email validation for user ${hdid}");
                emailInvite.ExpireDate = DateTime.UtcNow;
                this.emailInviteDelegate.Update(emailInvite);
            }

            if (!string.IsNullOrEmpty(email))
            {
                this.logger.LogInformation($"Sending new email invite for user ${hdid}");
                this.emailQueueService.QueueNewInviteEmail(hdid, email, hostUri);
            }

            this.logger.LogDebug($"Finished updating user email");
            return true;
        }

        private async void UpdateNotificationSettings(UserProfile userProfile, string bearerToken)
        {
            // Update the notification settings
            NotificationSettingsRequest request = new NotificationSettingsRequest(userProfile.Email, userProfile.SMSNumber);
            RequestResult<NotificationSettingsResponse> response = await this.notificationSettingsService.SendNotificationSettings(request, bearerToken).ConfigureAwait(true);
            if (response.ResultStatus == ResultType.Error)
            {
                this.notificationSettingsService.QueueNotificationSettings(request, bearerToken);
            }
        }
    }
}
