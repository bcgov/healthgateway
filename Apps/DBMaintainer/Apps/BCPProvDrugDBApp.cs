//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.DBMaintainer.Apps
{
    using System;
    using System.IO;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.DBMaintainer.FileDownload;
    using HealthGateway.DBMaintainer.Parsers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Utility program to load the BC Government PharmaCare drug file.
    /// See
    /// https://www2.gov.bc.ca/gov/content/health/practitioner-professional-resources/pharmacare/health-industry-professionals/downloadable-drug-data-files
    /// for reference.
    /// </summary>
    public class BcpProvDrugDbApp : BaseDrugApp<IPharmaCareDrugParser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BcpProvDrugDbApp"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="parser">The parser to use.</param>
        /// <param name="downloadService">The Download Service Utility.</param>
        /// <param name="configuration">The Configuration.</param>
        /// <param name="drugDbContext">The database context.</param>
        public BcpProvDrugDbApp(ILogger<BcpProvDrugDbApp> logger, IPharmaCareDrugParser parser, IFileDownloadService downloadService, IConfiguration configuration, GatewayDbContext drugDbContext)
            : base(logger, parser, downloadService, configuration, drugDbContext)
        {
        }

        /// <inheritdoc/>
        public override void ProcessDownload(string sourceFolder, FileDownload downloadedFile)
        {
            string[] files = Directory.GetFiles(sourceFolder, "pddf*.csv");
            if (files.Length > 1)
            {
                throw new FormatException($"The zip file contained {files.Length} CSV files, very confused.");
            }

            this.Logger.LogInformation("Parsing Provincial PharmaCare file");
            this.RemoveOldFiles(downloadedFile);
            this.AddFileToDb(downloadedFile);
            this.DrugDbContext.AddRange(this.Parser.ParsePharmaCareDrugFile(files[0], downloadedFile));
            this.Logger.LogInformation("Saving PharmaCare Drugs");
            this.DrugDbContext.SaveChanges();
        }
    }
}
