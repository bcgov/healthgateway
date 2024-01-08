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
    /// Utility program to load the BC Government PharmaCare drug file.
    /// https://www2.gov.bc.ca/gov/content/health/practitioner-professional-resources/pharmacare/health-industry-professionals/downloadable-drug-data-files.
    /// </summary>
    public class ProvincialDrugJob : BcpProvDrugDbApp
    {
        private const int ConcurrencyTimeout = 5 * 60; // Set the ConcurrentTimeout to 5 minutes

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvincialDrugJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="parser">The file parser.</param>
        /// <param name="downloadService">The download utility.</param>
        /// <param name="configuration">The IConfiguration to use.</param>
        /// <param name="drugDbContext">The database context to interact with.</param>
        /// <param name="pharmacyAssessmentParser">The pharmacy assessment parser to use.</param>
        public ProvincialDrugJob(
            ILogger<ProvincialDrugJob> logger,
            IPharmaCareDrugParser parser,
            IFileDownloadService downloadService,
            IConfiguration configuration,
            GatewayDbContext drugDbContext,
            IPharmacyAssessmentParser pharmacyAssessmentParser)
            : base(logger, parser, downloadService, configuration, drugDbContext, pharmacyAssessmentParser)
        {
        }

        /// <inheritdoc/>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public override async Task ProcessAsync(string configSectionName, CancellationToken ct = default)
        {
            this.Logger.LogDebug("Finished processing provincial drug files {ConfigSectionName}", configSectionName);
            await base.ProcessAsync(configSectionName, ct);
            this.Logger.LogDebug("Finished processing provincial drug files {ConfigSectionName}", configSectionName);
        }
    }
}
