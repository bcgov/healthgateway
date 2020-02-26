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
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Delegates;
    using Healthgateway.JobScheduler.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Globalization;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Validates that the HNClient Endpoint is responding correctly.
    /// If the endpoint does not respond then an email will be sent.
    /// </summary>
    public class NotifyUpdatedLegalAgreementsJob
    {
        private const string JobKey = "NotifyUpdatedLegalAgreements";
        private const string AgreementsKey = "LegalDocuments";
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes

        private readonly IConfiguration configuration;
        private readonly ILogger<NotifyUpdatedLegalAgreementsJob> logger;
        private readonly IApplicationSettingsDelegate applicationSettingsDelegate;
        private readonly ILegalAgreementDelegate legalAgreementDelegate;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdatedLegalAgreementsJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="applicationSettingsDelegate">The application settings delegate.</param>
        /// <param name="legalAgreementDelegate">The legal agreement delegate.</param>
        /// <param name="dbContext">The db context to use.</param>
        public NotifyUpdatedLegalAgreementsJob(
            IConfiguration configuration,
            ILogger<NotifyUpdatedLegalAgreementsJob> logger,
            IApplicationSettingsDelegate applicationSettingsDelegate,
            ILegalAgreementDelegate legalAgreementDelegate,
            GatewayDbContext dbContext)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.applicationSettingsDelegate = applicationSettingsDelegate;
            this.legalAgreementDelegate = legalAgreementDelegate;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Determines if any legal agreements notifications need to be sent out to the users.
        /// </summary>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void Process()
        {
            Console.WriteLine(this.GetType().Name);
            List<LegalDocument> documents = this.configuration.GetSection($"{JobKey}:{AgreementsKey}").Get<List<LegalDocument>>();
            this.logger.LogInformation($"Found {documents.Count} agreements to process");
            foreach (LegalDocument document in documents)
            {
                this.logger.LogInformation($"Processing {document.Name}, looking up Legal Agreement code {document.Code}");
                DBResult<LegalAgreement> result = this.legalAgreementDelegate.GetActiveByAgreementType(document.Code);
                if (result.Status == DBStatusCode.Read)
                {
                    this.logger.LogInformation($"Fetching {document.LastCheckedKey} from application settings");
                    ApplicationSetting lastCheckedSetting = this.applicationSettingsDelegate.GetApplicationSetting(ApplicationType.JobScheduler, this.GetType().Name, document.LastCheckedKey);
                    this.logger.LogInformation($"Found {document.LastCheckedKey} with value of {lastCheckedSetting.Value}");
                    DateTime lastChecked = System.DateTime.Parse(lastCheckedSetting.Value!, CultureInfo.InvariantCulture);
                    if (result.Payload.EffectiveDate > lastChecked)
                    {
                        // TODO: Pull All profiles
                        // TODO: For each profile send email template
                    }
                }
                else
                {
                    this.logger.LogCritical($"Unable to read {document.Name} from the DB ABORTING...");
                }
            }
        }
    }
}
