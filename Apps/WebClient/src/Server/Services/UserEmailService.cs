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
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class UserEmailService : IUserEmailService
    {
        private readonly ILogger logger;
        private readonly IEmailDelegate emailDelegate;
        private readonly IProfileDelegate profileDelegate;
        private readonly IEmailQueueService emailQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="emailDelegate">The email delegate to interact with the DB.</param>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        public UserEmailService(ILogger<UserEmailService> logger, IEmailDelegate emailDelegate, IProfileDelegate profileDelegate, IEmailQueueService emailQueueService)
        {
            this.logger = logger;
            this.emailDelegate = emailDelegate;
            this.profileDelegate = profileDelegate;
            this.emailQueueService = emailQueueService;
        }

        /// <inheritdoc />
        public bool ValidateEmail(string hdid, Guid inviteKey)
        {
            this.logger.LogTrace($"Validating email... {inviteKey}");
            bool retVal = false;
            EmailInvite emailInvite = this.emailDelegate.GetEmailInvite(inviteKey);

            if (emailInvite != null && emailInvite.HdId == hdid)
            {
                if (!emailInvite.Validated)
                {
                    emailInvite.Validated = true;
                    this.emailDelegate.UpdateEmailInvite(emailInvite);
                    UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
                    userProfile.Email = emailInvite.Email.To; // Gets the user email from the email sent.
                    this.profileDelegate.UpdateUserProfile(userProfile);
                }

                retVal = true;
            }

            this.logger.LogDebug($"Finished validating email: {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public EmailInvite RetrieveLastInvite(string hdid)
        {
            this.logger.LogTrace($"Retrieving last invite for {hdid}");
            EmailInvite emailInvite = this.emailDelegate.GetLastEmailInviteForUser(hdid);
            this.logger.LogDebug($"Finished retrieving email: {JsonConvert.SerializeObject(emailInvite)}");
            return emailInvite;
        }

        /// <inheritdoc />
        public bool UpdateUserEmail(string hdid, string email, Uri hostUri)
        {
            this.logger.LogTrace($"Updating user email...");
            bool retVal = false;
            UserProfile userProfile = this.profileDelegate.GetUserProfile(hdid).Payload;
            EmailInvite emailInvite = this.emailDelegate.GetLastEmailInviteForUser(hdid);

            if (email != userProfile.Email)
            {
                if (string.IsNullOrEmpty(email))
                {
                    // Removing the email
                    this.logger.LogDebug($"Removing email");

                    // Remove the current email until it gets validated
                    userProfile.Email = null;
                    this.profileDelegate.UpdateUserProfile(userProfile);

                    emailInvite.ExpireDate = DateTime.Now;
                    this.emailDelegate.UpdateEmailInvite(emailInvite);
                }
                else if (emailInvite?.Email?.To != email || emailInvite.ExpireDate < DateTime.Now)
                {
                    // Create a new invite email 
                    this.logger.LogDebug($"Updating email");

                    // Remove the current email until it gets validated
                    userProfile.Email = null;
                    this.profileDelegate.UpdateUserProfile(userProfile);

                    // Expire the previous invite email
                    if (emailInvite?.Email?.To != null)
                    {
                        emailInvite.ExpireDate = DateTime.Now;
                        this.emailDelegate.UpdateEmailInvite(emailInvite);
                    }

                    this.emailQueueService.QueueNewInviteEmail(hdid, email, hostUri);
                }
                else
                {
                    // Same email, validation needs to be resent
                    this.logger.LogDebug($"Re-queueing email");

                    // Add the existing email to the queue
                    this.emailQueueService.QueueInviteEmail(emailInvite.Id);
                }
                retVal = true;
                this.logger.LogDebug($"Finished updating user email");
            }

            return retVal;
        }
    }
}
