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
        /// <inheritdoc/>
        public FedDrugDBApp(ILogger<FedDrugDBApp> logger, IDrugProductParser parser, IFileDownloadService downloadService, IConfiguration configuration, GatewayDbContext drugDBContext)
            : base(logger, parser, downloadService, configuration, drugDBContext)
        {
        }

        /// <inheritdoc/>
        public override void ProcessDownload(string sourceFolder, FileDownload downloadedFile)
        {
            this.logger.LogInformation("Parsing Drug File and adding to DB Context");
            List<DrugProduct> drugProducts = this.parser.ParseDrugFile(sourceFolder, downloadedFile);
            this.drugDbContext.DrugProduct.AddRange(drugProducts);
            this.logger.LogInformation("Parsing Other files and adding to DB Context");
            this.drugDbContext.ActiveIngredient.AddRange(this.parser.ParseActiveIngredientFile(sourceFolder, drugProducts));
            this.drugDbContext.Company.AddRange(this.parser.ParseCompanyFile(sourceFolder, drugProducts));
            this.drugDbContext.Status.AddRange(this.parser.ParseStatusFile(sourceFolder, drugProducts));
            this.drugDbContext.Form.AddRange(this.parser.ParseFormFile(sourceFolder, drugProducts));
            this.drugDbContext.Packaging.AddRange(this.parser.ParsePackagingFile(sourceFolder, drugProducts));
            this.drugDbContext.PharmaceuticalStd.AddRange(this.parser.ParsePharmaceuticalStdFile(sourceFolder, drugProducts));
            this.drugDbContext.Route.AddRange(this.parser.ParseRouteFile(sourceFolder, drugProducts));
            this.drugDbContext.Schedule.AddRange(this.parser.ParseScheduleFile(sourceFolder, drugProducts));
            this.drugDbContext.TherapeuticClass.AddRange(this.parser.ParseTherapeuticFile(sourceFolder, drugProducts));
            this.drugDbContext.VeterinarySpecies.AddRange(this.parser.ParseVeterinarySpeciesFile(sourceFolder, drugProducts));
            this.AddFileToDB(downloadedFile);
            this.RemoveOldFiles(downloadedFile);
            this.logger.LogInformation("Saving Entities to the database");
            this.drugDbContext.SaveChanges();
        }
    }
}
