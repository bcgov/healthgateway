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
namespace Healthgateway.JobScheduler.Jobs
{
    using Hangfire;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Database.Context;
    using HealthGateway.DrugMaintainer;
    using HealthGateway.DrugMaintainer.Apps;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Utility program to load the BC Government PharmaCare drug file.
    /// https://www2.gov.bc.ca/gov/content/health/practitioner-professional-resources/pharmacare/health-industry-professionals/downloadable-drug-data-files.
    /// </summary>
    public class ProvincialDrugJob : BCPProvDrugDBApp
    {
        private const int ConcurrencyTimeout = 5 * 60; // Set the ConcurrentTimeout to 5 minutes

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvincialDrugJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="parser">The file parser.</param>
        /// <param name="downloadService">The download utility.</param>
        /// <param name="configuration">The IConfiguration to use.</param>
        /// <param name="drugDBContext">The database context to interact with.</param>
        public ProvincialDrugJob(ILogger<BCPProvDrugDBApp> logger, IPharmaCareDrugParser parser, IFileDownloadService downloadService, IConfiguration configuration, GatewayDbContext drugDBContext)
            : base(logger, parser, downloadService, configuration, drugDBContext)
        {
        }

        /// <inheritdoc/>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public override void Process(string configSectionName)
        {
            this.logger.LogDebug($"Finished processing provincial drug files {configSectionName}");
            base.Process(configSectionName);
            this.logger.LogDebug($"Finished processing provincial drug files {configSectionName}");
        }
    }
}