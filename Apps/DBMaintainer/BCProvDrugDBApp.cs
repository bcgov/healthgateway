// //-------------------------------------------------------------------------
// // Copyright © 2019 Province of British Columbia
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //-------------------------------------------------------------------------
namespace HealthGateway.DrugMaintainer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;

    public class BCPProvDrugDBApp
    {
        private readonly ILogger logger;
        private IPharmaCareDrugParser parser;
        private IFileDownloadService downloadService;
        private readonly IConfiguration configuration;
        private readonly DrugDbContext drugDBContext;
        private const string CONFIG_SECTION = "PharmaCareDrugFile";

        public BCPProvDrugDBApp(ILogger<BCPProvDrugDBApp> logger, IPharmaCareDrugParser parser, IFileDownloadService downloadService, IConfiguration configuration, DrugDbContext drugDBContext)
        {
            this.logger = logger;
            this.parser = parser;
            this.downloadService = downloadService;
            this.configuration = configuration;
            this.drugDBContext = drugDBContext;
        }

        public async Task UpdatePharmaCareDrugs()
        {
            logger.LogInformation(AppDomain.CurrentDomain.BaseDirectory);
            string targetFolder = configuration.GetSection(CONFIG_SECTION).GetValue<string>("TargetFolder");
            DownloadedFile downloadedFile = await asyncDownloadFiles(targetFolder);
            string unzippedPath = extractFiles(downloadedFile);
            logger.LogInformation("Updating Database");
            updateDatabase(unzippedPath);

            logger.LogInformation($"Removing extracted files under {unzippedPath}");
            Directory.Delete(unzippedPath, true);

        }

        private async Task<DownloadedFile> asyncDownloadFiles(string targetFolder)
        {
            logger.LogInformation("Downloading file...");
            Uri filePath = configuration.GetSection(CONFIG_SECTION).GetValue<Uri>("Url");
            Task<DownloadedFile> downloadedFile = downloadService.GetFileFromUrl(filePath, targetFolder, true);
            return await downloadedFile;
        }

        private string extractFiles(DownloadedFile downloadedFile)
        {
            string filename = Path.Combine(downloadedFile.LocalFilePath, downloadedFile.FileName);
            logger.LogInformation($"Extracting zip file: {filename}");
            string unzipedPath = Path.Combine(downloadedFile.LocalFilePath, Path.GetFileNameWithoutExtension(downloadedFile.FileName));
            ZipFile.ExtractToDirectory(filename, unzipedPath);
            logger.LogInformation("Deleting Zip file");
            File.Delete(filename);
            return unzipedPath;
        }

        private void updateDatabase(string unzippedPath)
        {
            string[] files = Directory.GetFiles(unzippedPath, "pddf*.csv");
            if (files.Length > 1)
            {
                throw new ApplicationException($"The zip file contained {files.Length} CSV files, very confused.");
            }
            this.logger.LogInformation("Parsing Provincial PharmaCare file");
            List<PharmaCareDrug> pharmaCareDrugs = this.parser.ParsePharmaCareDrugFile(files[0]);

            DrugDbContext ctx = this.drugDBContext;
            //foreach(PharmaCareDrug drug in pharmaCareDrugs)
            //{
            //    ctx.Add(drug);
            //    try
            //    {
            //        ctx.SaveChanges();
            //    }
            //    catch (Exception e)
            //    {
            //        logger.LogCritical(e.ToString());
            //    }
            //}
            //ctx.PharmaCareDrug.AddRange(pharmaCareDrugs);
            //logger.LogInformation("Saving PharmaCare Drugs");
            //ctx.SaveChanges();
        }
    }
}
