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
namespace HealthGateway.DrugMaintainer.Apps
{
    using System.Collections.Generic;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Utility program to load the Federal Government Drug Product database.
    /// Reads the AllFiles zip as located and documented at
    /// https://www.canada.ca/en/health-canada/services/drugs-health-products/drug-products/drug-product-database/what-data-extract-drug-product-database.html
    /// </summary>
    public class FedDrugDBApp : BaseDrugApp<IDrugProductParser>
    {
        /// <summary>
        /// The name used to lookup configuration.
        /// </summary>
        private const string CONFIG_SECTION = "DrugProductDatabase";

        /// <inheritdoc/>
        public FedDrugDBApp(ILogger<FedDrugDBApp> logger, IDrugProductParser parser, IFileDownloadService downloadService, IConfiguration configuration, DrugDbContext drugDBContext)
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
            this.logger.LogInformation("Parsing files...");
            List<DrugProduct> drugProducts = this.parser.ParseDrugFile(sourceFolder);
            List<ActiveIngredient> ingredients = this.parser.ParseActiveIngredientFile(sourceFolder, drugProducts);
            List<Company> companies = this.parser.ParseCompanyFile(sourceFolder, drugProducts);
            List<Status> statuses = this.parser.ParseStatusFile(sourceFolder, drugProducts);
            List<Form> forms = this.parser.ParseFormFile(sourceFolder, drugProducts);
            List<Packaging> packagings = this.parser.ParsePackagingFile(sourceFolder, drugProducts);
            List<PharmaceuticalStd> pharmaceuticals = this.parser.ParsePharmaceuticalStdFile(sourceFolder, drugProducts);
            List<Route> routes = this.parser.ParseRouteFile(sourceFolder, drugProducts);
            List<Schedule> schedules = this.parser.ParseScheduleFile(sourceFolder, drugProducts);
            List<TherapeuticClass> therapeuticClasses = this.parser.ParseTherapeuticFile(sourceFolder, drugProducts);
            List<VeterinarySpecies> veterinarySpecies = this.parser.ParseVeterinarySpeciesFile(sourceFolder, drugProducts);
            logger.LogInformation("Adding entities to context");
            this.drugDbContext.DrugProduct.AddRange(drugProducts);
            this.drugDbContext.ActiveIngredient.AddRange(ingredients);
            this.drugDbContext.Company.AddRange(companies);
            this.drugDbContext.Status.AddRange(statuses);
            this.drugDbContext.Form.AddRange(forms);
            this.drugDbContext.Packaging.AddRange(packagings);
            this.drugDbContext.PharmaceuticalStd.AddRange(pharmaceuticals);
            this.drugDbContext.Route.AddRange(routes);
            this.drugDbContext.Schedule.AddRange(schedules);
            this.drugDbContext.TherapeuticClass.AddRange(therapeuticClasses);
            this.drugDbContext.VeterinarySpecies.AddRange(veterinarySpecies);
            AddFileToDB(downloadedFile);
            logger.LogInformation("Saving Entities to the database");
            this.drugDbContext.SaveChanges();
        }
    }
}
