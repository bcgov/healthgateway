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
namespace Healthgateway.JobScheduler.Jobs
{
    using System;
    using System.Collections.Generic;
    using Hangfire;
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Confirms if a new Legal Agreement is in place and notifies clients.
    /// </summary>
    public class CloseAccountJob
    {
        private const string JobKey = "CloseAccounts";
        private const string ProfilesPageSizeKey = "ProfilesPageSize";
        private const string HoursDeletionKey = "HoursBeforeDeletion";
        private const string EmailTemplateKey = "EmailTemplate";
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes

        private const string AuthConfigSectionName = "ClientAuthentication";

        private readonly ILogger<CloseAccountJob> logger;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IEmailQueueService emailService;

        private readonly IAuthenticationDelegate authDelegate;

        private readonly IUserAdminDelegate userAdminDelegate;

        private readonly GatewayDbContext dbContext;
        private readonly int profilesPageSize;
        private readonly int hoursBeforeDeletion;
        private readonly string emailTemplate;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly Uri tokenUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseAccountJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="profileDelegate">The profile delegate.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="userAdminDelegate">The AccessManagement userAdmin delegate.</param>
        /// <param name="dbContext">The db context to use.</param>
        public CloseAccountJob(
            IConfiguration configuration,
            ILogger<CloseAccountJob> logger,
            IUserProfileDelegate profileDelegate,
            IEmailQueueService emailService,
            IAuthenticationDelegate authDelegate,
            IUserAdminDelegate userAdminDelegate,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.profileDelegate = profileDelegate;
            this.emailService = emailService;
            this.authDelegate = authDelegate;
            this.userAdminDelegate = userAdminDelegate;
            this.dbContext = dbContext;
            this.profilesPageSize = configuration.GetValue<int>($"{JobKey}:{ProfilesPageSizeKey}");
            this.hoursBeforeDeletion = configuration.GetValue<int>($"{JobKey}:{HoursDeletionKey}") * -1;
            this.emailTemplate = configuration.GetValue<string>($"{JobKey}:{EmailTemplateKey}");

            IConfigurationSection? configSection = configuration?.GetSection(AuthConfigSectionName);
            this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");

            this.tokenRequest = new ClientCredentialsTokenRequest();
            configSection.Bind(this.tokenRequest);
        }

        /// <summary>
        /// Deletes any closed accounts that are over n hours old.
        /// </summary>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void Process()
        {
            DateTime deleteDate = DateTime.UtcNow.AddHours(this.hoursBeforeDeletion);
            this.logger.LogInformation($"Looking for closed accounts that are earlier than {deleteDate}");
            int page = 0;
            DBResult<List<UserProfile>> profileResult;
            do
            {
                profileResult = this.profileDelegate.GetClosedProfiles(deleteDate, page, this.profilesPageSize);
                foreach (UserProfile profile in profileResult.Payload)
                {
                    this.dbContext.UserProfile.Remove(profile);
                    if (!string.IsNullOrWhiteSpace(profile.Email))
                    {
                        this.emailService.QueueNewEmail(profile.Email!, this.emailTemplate, false);
                    }

                    JwtModel jwtModel = this.authDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest);

                    this.userAdminDelegate.DeleteUser(profile.IdentityManagementId!.Value, jwtModel);
                }

                this.logger.LogInformation($"Removed and sent emails for {profileResult.Payload.Count} closed profiles");
                this.dbContext.SaveChanges(); // commit after every page
                page++;
            }
            while (profileResult.Payload.Count == this.profilesPageSize);

            this.logger.LogInformation($"Completed processing {page} page(s) with pagesize set to {this.profilesPageSize}");
        }
    }
}
