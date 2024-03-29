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
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Database.Context;
    using HealthGateway.DBMaintainer.Apps;
    using HealthGateway.DBMaintainer.FileDownload;
    using HealthGateway.DBMaintainer.Parsers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Utility program to load the Federal Government Drug Product database.
    /// Reads the AllFiles zip as located and documented at
    /// https://www.canada.ca/en/health-canada/services/drugs-health-products/drug-products/drug-product-database/what-data-extract-drug-product-database.html.
    /// </summary>
    public class FedDrugJob : FedDrugDbApp
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
        /// <param name="drugDbContext">The database context to interact with.</param>
        public FedDrugJob(ILogger<FedDrugJob> logger, IDrugProductParser parser, IFileDownloadService downloadService, IConfiguration configuration, GatewayDbContext drugDbContext)
            : base(logger, parser, downloadService, configuration, drugDbContext)
        {
        }

        /// <inheritdoc/>
        [MaximumConcurrentExecutions(MaxConcurrency, ConcurrencyTimeout)]
        public override async Task ProcessAsync(string configSectionName, CancellationToken ct = default)
        {
            this.Logger.LogDebug("Processing federal drug files {ConfigSectionName}", configSectionName);
            await base.ProcessAsync(configSectionName, ct);
            this.Logger.LogDebug("Finished processing federal drug files {ConfigSectionName}", configSectionName);
        }
    }
}
