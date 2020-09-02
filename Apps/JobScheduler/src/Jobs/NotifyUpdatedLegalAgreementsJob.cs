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
    using System.Globalization;
    using Hangfire;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Utils;
    using HealthGateway.Database.Wrapper;
    using Healthgateway.JobScheduler.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Confirms if a new Legal Agreement is in place and notifies clients.
    /// </summary>
    public class NotifyUpdatedLegalAgreementsJob
    {
        private const string JobKey = "NotifyUpdatedLegalAgreements";
        private const string AgreementsKey = "LegalAgreements";
        private const string ProfilesPageSizeKey = "ProfilesPageSize";
        private const string HostKey = "Host";
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes

        private readonly IConfiguration configuration;
        private readonly ILogger<NotifyUpdatedLegalAgreementsJob> logger;
        private readonly IApplicationSettingsDelegate applicationSettingsDelegate;
        private readonly ILegalAgreementDelegate legalAgreementDelegate;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IEmailQueueService emailService;
        private readonly GatewayDbContext dbContext;
        private readonly int profilesPageSize;
        private readonly string host;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdatedLegalAgreementsJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="applicationSettingsDelegate">The application settings delegate.</param>
        /// <param name="legalAgreementDelegate">The legal agreement delegate.</param>
        /// <param name="profileDelegate">The profile delegate.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="dbContext">The db context to use.</param>
        public NotifyUpdatedLegalAgreementsJob(
            IConfiguration configuration,
            ILogger<NotifyUpdatedLegalAgreementsJob> logger,
            IApplicationSettingsDelegate applicationSettingsDelegate,
            ILegalAgreementDelegate legalAgreementDelegate,
            IUserProfileDelegate profileDelegate,
            IEmailQueueService emailService,
            GatewayDbContext dbContext)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.applicationSettingsDelegate = applicationSettingsDelegate;
            this.legalAgreementDelegate = legalAgreementDelegate;
            this.profileDelegate = profileDelegate;
            this.emailService = emailService;
            this.dbContext = dbContext;
            this.profilesPageSize = this.configuration.GetValue<int>($"{JobKey}:{ProfilesPageSizeKey}");
            this.host = this.configuration.GetValue<string>($"{HostKey}");
        }

        /// <summary>
        /// Determines if any legal agreements notifications need to be sent out to the users.
        /// </summary>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void Process()
        {
            Console.WriteLine(this.GetType().Name);
            List<LegalAgreementConfig> agreementConfigs = this.configuration.GetSection($"{JobKey}:{AgreementsKey}").Get<List<LegalAgreementConfig>>();
            this.logger.LogInformation($"Found {agreementConfigs.Count} agreements to process");
            foreach (LegalAgreementConfig lac in agreementConfigs)
            {
                this.logger.LogInformation($"Processing {lac.Name}, looking up Legal Agreement code {lac.Code}");
                LegalAgreementType agreement = EnumUtility.ToEnum<LegalAgreementType>(lac.Code, true);
                DBResult<LegalAgreement> legalAgreementsResult = this.legalAgreementDelegate.GetActiveByAgreementType(agreement);
                if (legalAgreementsResult.Status == DBStatusCode.Read)
                {
                    this.ProcessLegalAgreement(legalAgreementsResult.Payload, lac);
                }
                else
                {
                    this.logger.LogCritical($"Unable to read {lac.Name} from the DB ABORTING...");
                }
            }
        }

        private void ProcessLegalAgreement(LegalAgreement agreement, LegalAgreementConfig config)
        {
            this.logger.LogInformation($"{config.Name} found, last updated {agreement.EffectiveDate}");
            this.logger.LogInformation($"Fetching {config.LastCheckedKey} from application settings");
            ApplicationSetting lastCheckedSetting = this.applicationSettingsDelegate.GetApplicationSetting(ApplicationType.JobScheduler, this.GetType().Name, config.LastCheckedKey);
            this.logger.LogInformation($"Found {config.LastCheckedKey} with value of {lastCheckedSetting.Value}");
            DateTime lastChecked = System.DateTime.Parse(lastCheckedSetting.Value!, CultureInfo.InvariantCulture);
            if (agreement.EffectiveDate > lastChecked)
            {
                Dictionary<string, string> keyValues = new Dictionary<string, string>();
                keyValues.Add("host", this.host);
                keyValues.Add("path", config.Path);
                keyValues.Add("effectivedate", agreement.EffectiveDate.Value.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture));
                keyValues.Add("contactemail", config.ContactEmail);
                int page = 0;
                DBResult<List<UserProfile>> profileResult;
                do
                {
                    profileResult = this.profileDelegate.GetAllUserProfilesAfter(agreement.EffectiveDate.Value, page, this.profilesPageSize);
                    foreach (UserProfile profile in profileResult.Payload)
                    {
                        this.emailService.QueueNewEmail(profile.Email!, config.EmailTemplate, keyValues, false);
                    }

                    this.logger.LogInformation($"Sent {profileResult.Payload.Count} emails");

                    // TODO: Resume functionality??
                    this.dbContext.SaveChanges(); // commit after every page
                    page++;
                }
                while (profileResult.Payload.Count == this.profilesPageSize);
                this.logger.LogInformation($"Completed sending emails after processing {page} page(s) with pagesize set to {this.profilesPageSize}");
                lastCheckedSetting.Value = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                this.logger.LogInformation($"Saving rundate of {lastCheckedSetting.Value} to DB");
                this.dbContext.SaveChanges();
            }
            else
            {
                this.logger.LogInformation($"{config.Name} has not been updated since last run");
            }
        }
    }
}
