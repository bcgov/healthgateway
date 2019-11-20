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
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <inheritdoc />
    public class EmailValidationService : IEmailValidationService
    {
        private readonly IEmailDelegate emailDelegate;
        private readonly IProfileDelegate profileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailValidationService"/> class.
        /// </summary>
        /// <param name="emailDelegate">The email delegate to interact with the DB.</param>
        public EmailValidationService(IEmailDelegate emailDelegate, IProfileDelegate profileDelegate)
        {
            this.emailDelegate = emailDelegate;
            this.profileDelegate = profileDelegate;
        }

        /// <inheritdoc />
        public bool ValidateEmail(string hdid, Guid inviteKey)
        {
            bool retVal = false;
            EmailInvite emailInvite = this.emailDelegate.GetEmailInvite(hdid, inviteKey);
            if (emailInvite != null)
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

            return retVal;
        }
    }
}
