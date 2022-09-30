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
    using Hangfire;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Database.Context;
    using HealthGateway.DrugMaintainer;
    using HealthGateway.DrugMaintainer.Apps;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Utility program to load the Federal Government Drug Product database.
    /// Reads the AllFiles zip as located and documented at
    /// https://www.canada.ca/en/health-canada/services/drugs-health-products/drug-products/drug-product-database/what-data-extract-drug-product-database.html.
    /// </summary>
    public class FedDrugJob : FedDrugDBApp
    {
        private const int ConcurrencyTimeout = 15 * 60; // Set the ConcurrentTimeout to 15 minutes
        private const int MaxConcurrency = 4; // We allow a maximum of 4 instances of this job (Active, Cancelled, Marketed, and Dormant)

        /// <summary>
        /// Initializes a new instance of the <see cref="FedDrugJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="parser">The file parser.</param>
        /// <param name="downloadService">The download utility.</param>
        /// <param name="configuration">The IConfiguration to use.</param>
        /// <param name="drugDBContext">The database context to interact with.</param>
        public FedDrugJob(ILogger<FedDrugDBApp> logger, IDrugProductParser parser, IFileDownloadService downloadService, IConfiguration configuration, GatewayDbContext drugDBContext)
            : base(logger, parser, downloadService, configuration, drugDBContext)
        {
        }

        /// <inheritdoc/>
        [MaximumConcurrentExecutions(MaxConcurrency, ConcurrencyTimeout)]
        public override void Process(string configSectionName)
        {
            this.Logger.LogDebug("Processing federal drug files {ConfigSectionName}", configSectionName);
            base.Process(configSectionName);
            this.Logger.LogDebug("Finished processing federal drug files {ConfigSectionName}", configSectionName);
        }
    }
}
