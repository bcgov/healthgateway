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

    public class FedDrugDBApp
    {
        private readonly ILogger logger;
        private IDrugProductParser parser;
        private IFileDownloadService downloadService;
        private readonly IConfiguration configuration;
        private readonly DrugDBContext drugDBContext;

        public FedDrugDBApp(ILogger<FedDrugDBApp> logger, IDrugProductParser parser, IFileDownloadService downloadService, IConfiguration configuration, DrugDBContext drugDBContext)
        {
            this.logger = logger;
            this.parser = parser;
            this.downloadService = downloadService;
            this.configuration = configuration;
            this.drugDBContext = drugDBContext;
        }

        public async Task UpdateDrugProducts()
        {
            string targetFolder = configuration.GetSection("DrugProductDatabase").GetValue<string>("TargetFolder");
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
            Uri filePath = configuration.GetSection("DrugProductDatabase").GetValue<Uri>("Url");
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
            this.logger.LogInformation("Adding Entities to DB");
            DrugDBContext ctx = this.drugDBContext;
            List<DrugProduct> drugProducts = this.parser.ParseDrugFile(unzippedPath);
            ctx.DrugProduct.AddRange(drugProducts);
            logger.LogInformation("Saving Drug Products.");
            ctx.SaveChanges();
            List<ActiveIngredient> ingredients = this.parser.ParseActiveIngredientFile(unzippedPath, drugProducts);
            ctx.ActiveIngredient.AddRange(ingredients);
            List<Company> companies = this.parser.ParseCompanyFile(unzippedPath, drugProducts);
            ctx.Company.AddRange(companies);
            List<Status> statuses = this.parser.ParseStatusFile(unzippedPath, drugProducts);
            ctx.Status.AddRange(statuses);
            List<Form> forms = this.parser.ParseFormFile(unzippedPath, drugProducts);
            ctx.Form.AddRange(forms);
            List<Packaging> packagings = this.parser.ParsePackagingFile(unzippedPath, drugProducts);
            ctx.Packaging.AddRange(packagings);
            logger.LogInformation("Saving ingredients, companies, statuses, forms and packagings.");
            ctx.SaveChanges();
            List<PharmaceuticalStd> pharmaceuticals = this.parser.ParsePharmaceuticalStdFile(unzippedPath, drugProducts);
            ctx.PharmaceuticalStd.AddRange(pharmaceuticals);
            List<Route> routes = this.parser.ParseRouteFile(unzippedPath, drugProducts);
            ctx.Route.AddRange(routes);
            List<Schedule> schedules = this.parser.ParseScheduleFile(unzippedPath, drugProducts);
            ctx.Schedule.AddRange(schedules);
            List<TherapeuticClass> therapeuticClasses = this.parser.ParseTherapeuticFile(unzippedPath, drugProducts);
            ctx.TherapeuticClass.AddRange(therapeuticClasses);
            List<VeterinarySpecies> veterinarySpecies = this.parser.ParseVeterinarySpeciesFile(unzippedPath, drugProducts);
            ctx.VeterinarySpecies.AddRange(veterinarySpecies);
            logger.LogInformation("Saving Entities");
            ctx.SaveChanges();
        }
    }
}
