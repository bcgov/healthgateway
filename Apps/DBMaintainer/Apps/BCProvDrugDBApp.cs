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
namespace HealthGateway.DrugMaintainer.Apps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Utility program to load the BC Government PharmaCare drug file.
    /// https://www2.gov.bc.ca/gov/content/health/practitioner-professional-resources/pharmacare/health-industry-professionals/downloadable-drug-data-files
    /// </summary>
    public class BCPProvDrugDBApp : BaseDrugApp<IPharmaCareDrugParser>
    {
        /// <summary>
        /// The name used to lookup configuration.
        /// </summary>
        private const string CONFIG_SECTION = "PharmaCareDrugFile";

        /// <inheritdoc/>
        public BCPProvDrugDBApp(ILogger<BCPProvDrugDBApp> logger, IPharmaCareDrugParser parser, IFileDownloadService downloadService, IConfiguration configuration, DrugDbContext drugDBContext)
            : base(logger, parser, downloadService, configuration, drugDBContext)
        {
        }

        /// <inheritdoc/>
        protected override string configurationName
        {
            get
            {
                return CONFIG_SECTION;
            }
        }

        /// <inheritdoc/>
        public override void ProcessDownload(string sourceFolder, FileDownload downloadedFile)
        {
            downloadedFile.ProgramTypeCodeId = Database.Constant.ProgramType.Provincial;
            string[] files = Directory.GetFiles(sourceFolder, "pddf*.csv");
            if (files.Length > 1)
            {
                throw new ApplicationException($"The zip file contained {files.Length} CSV files, very confused.");
            }
            this.logger.LogInformation("Parsing Provincial PharmaCare file");
            this.drugDbContext.AddRange(this.parser.ParsePharmaCareDrugFile(files[0], downloadedFile));
            this.AddFileToDB(downloadedFile);
            this.RemoveOldFiles(downloadedFile);
            logger.LogInformation("Saving PharmaCare Drugs");
            this.drugDbContext.SaveChanges();
        }
    }
}
