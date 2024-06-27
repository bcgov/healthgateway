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
namespace HealthGateway.JobScheduler.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

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

        private const string AuthConfigSectionName = "KeycloakAdmin";

        private readonly IAuthenticationDelegate authDelegate;

        private readonly GatewayDbContext dbContext;
        private readonly IEmailQueueService emailService;
        private readonly string emailTemplate;
        private readonly int hoursBeforeDeletion;

        private readonly ILogger<CloseAccountJob> logger;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly int profilesPageSize;
        private readonly ClientCredentialsRequest clientCredentialsRequest;
        private readonly IKeycloakAdminApi keycloakAdminApi;
        private readonly IMessageSender messageSender;
        private readonly bool accountsChangeFeedEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseAccountJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="profileDelegate">The profile delegate.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="keycloakAdminApi">The Keycloak admin API.</param>
        /// <param name="dbContext">The db context to use.</param>
        /// <param name="messageSender">The message sender.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public CloseAccountJob(
            IConfiguration configuration,
            ILogger<CloseAccountJob> logger,
            IUserProfileDelegate profileDelegate,
            IEmailQueueService emailService,
            IAuthenticationDelegate authDelegate,
            IKeycloakAdminApi keycloakAdminApi,
            GatewayDbContext dbContext,
            IMessageSender messageSender)
        {
            this.logger = logger;
            this.profileDelegate = profileDelegate;
            this.emailService = emailService;
            this.authDelegate = authDelegate;
            this.keycloakAdminApi = keycloakAdminApi;
            this.dbContext = dbContext;
            this.messageSender = messageSender;
            this.profilesPageSize = configuration.GetValue<int>($"{JobKey}:{ProfilesPageSizeKey}");
            this.hoursBeforeDeletion = configuration.GetValue<int>($"{JobKey}:{HoursDeletionKey}") * -1;
            this.emailTemplate = configuration.GetValue<string>($"{JobKey}:{EmailTemplateKey}") ??
                                 throw new ArgumentNullException(nameof(configuration), $"{JobKey}:{EmailTemplateKey} is null");
            this.clientCredentialsRequest = this.authDelegate.GetClientCredentialsRequestFromConfig(AuthConfigSectionName);
            ChangeFeedOptions? changeFeedConfiguration = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>();
            this.accountsChangeFeedEnabled = changeFeedConfiguration?.Accounts.Enabled ?? false;
        }

        /// <summary>
        /// Deletes any closed accounts that are over n hours old.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public async Task ProcessAsync(CancellationToken ct = default)
        {
            DateTime deleteDate = DateTime.UtcNow.AddHours(this.hoursBeforeDeletion);
            this.logger.LogInformation("Looking for closed accounts that are earlier than {DeleteDate}", deleteDate);
            int page = 0;
            List<UserProfile> userProfiles;

            do
            {
                userProfiles = await this.profileDelegate.GetClosedProfilesAsync(deleteDate, page, this.profilesPageSize, ct);

                foreach (UserProfile profile in userProfiles)
                {
                    await this.DeleteAccount(profile, ct);
                }

                this.logger.LogInformation("Removed and sent emails for {Count} closed profiles", userProfiles.Count);
                await this.dbContext.SaveChangesAsync(ct); // commit after every page
                page++;
            }
            while (userProfiles.Count == this.profilesPageSize);

            this.logger.LogInformation("Completed processing {Page} page(s) with page size set to {ProfilesPageSize}", page, this.profilesPageSize);
        }

        private async Task DeleteAccount(UserProfile profile, CancellationToken ct)
        {
            this.dbContext.UserProfile.Remove(profile);
            if (this.accountsChangeFeedEnabled)
            {
                MessageEnvelope[] events = [new(new AccountClosedEvent(profile.HdId, DateTime.Now), profile.HdId)];
                await this.messageSender.SendAsync(events, ct);
            }

            if (!string.IsNullOrWhiteSpace(profile.Email))
            {
                await this.emailService.QueueNewEmailAsync(profile.Email!, this.emailTemplate, false, ct);
            }

            JwtModel jwtModel = await this.authDelegate.AuthenticateAsSystemAsync(this.clientCredentialsRequest, ct: ct);

            try
            {
                await this.keycloakAdminApi.DeleteUserAsync(profile.IdentityManagementId!.Value, jwtModel.AccessToken, ct);
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogError(
                    e,
                    "Error deleting {Id} from Keycloak with exception: {Message}",
                    profile.IdentityManagementId,
                    e.Message);
            }
        }
    }
}
