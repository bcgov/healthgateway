﻿// -------------------------------------------------------------------------
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

namespace HealthGateway.Admin.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Admin.Constants;
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service that provides functionality to admin emails.
    /// </summary>
    public class EmailAdminService : IEmailAdminService
    {
        private const string EmailAdminSectionConfigKey = "EmailAdmin";
        private const string MaxEmailsConfigKey = "MaxEmails";
        private const int DefaultMaxEmails = 1000;

        private readonly IConfiguration configuration;
        private readonly ILogger<EmailAdminService> logger;
        private readonly IEmailDelegate emailDelegate;
        private readonly IMessagingVerificationDelegate emailInviteDelegate;
        private readonly int maxEmails;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAdminService"/> class.
        /// </summary>
        /// <param name="configuration">Injected configuration provider.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="emailDelegate">The email delegate to interact with the DB.</param>
        /// <param name="emailInviteDelegate">The email invite delegate to interact with the DB.</param>
        public EmailAdminService(
            IConfiguration configuration,
            ILogger<EmailAdminService> logger,
            IEmailDelegate emailDelegate,
            IMessagingVerificationDelegate emailInviteDelegate)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.emailDelegate = emailDelegate;
            this.emailInviteDelegate = emailInviteDelegate;
            IConfigurationSection section = configuration!.GetSection(EmailAdminSectionConfigKey);
            this.maxEmails = section.GetValue<int>(MaxEmailsConfigKey, DefaultMaxEmails);
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<AdminEmail>> GetEmails()
        {
            int pageIndex = 0;
            DBResult<List<Email>> dbEmail = this.emailDelegate.GetEmails(pageIndex, this.maxEmails);
            IEnumerable<MessagingVerification> emailInvites = this.emailInviteDelegate.GetAllEmail();
            RequestResult<IEnumerable<AdminEmail>> result = new RequestResult<IEnumerable<AdminEmail>>()
            {
                ResourcePayload = dbEmail.Payload.Select(e =>
                {
                    MessagingVerification emailInvite = emailInvites.FirstOrDefault(ei =>
                        e.To!.Equals(ei.Email?.To, System.StringComparison.CurrentCultureIgnoreCase));
                    string inviteStatus = this.GetEmailInviteStatus(emailInvite);
                    return AdminEmail.CreateFromDbModel(e, inviteStatus);
                }),
                PageIndex = pageIndex,
                PageSize = this.maxEmails,
                TotalResultCount = dbEmail.Payload.Count,
                ResultStatus = dbEmail.Status == DBStatusCode.Read ? ResultType.Success : ResultType.Error,
                ResultMessage = dbEmail.Message,
            };
            return result;
        }

        private string GetEmailInviteStatus(MessagingVerification emailInvite)
        {
            if (emailInvite == null)
            {
                return UserInviteStatus.NotInvited;
            }

            return emailInvite.Validated ? UserInviteStatus.InvitedValidated : UserInviteStatus.InvitedNotValidated;
        }
    }
}