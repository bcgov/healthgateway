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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.DBMaintainer.FileDownload;
    using HealthGateway.DBMaintainer.Parsers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Utility program to load the Federal Government Drug Product database.
    /// Reads the AllFiles zip as located and documented at
    /// See
    /// https://www.canada.ca/en/health-canada/services/drugs-health-products/drug-products/drug-product-database/what-data-extract-drug-product-database.html
    /// for reference.
    /// </summary>
    public class FedDrugDbApp : BaseDrugApp<IDrugProductParser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FedDrugDbApp"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="parser">The parser to use.</param>
        /// <param name="downloadService">The Download Service Utility.</param>
        /// <param name="configuration">The Configuration.</param>
        /// <param name="drugDbContext">The database context.</param>
        public FedDrugDbApp(ILogger<FedDrugDbApp> logger, IDrugProductParser parser, IFileDownloadService downloadService, IConfiguration configuration, GatewayDbContext drugDbContext)
            : base(logger, parser, downloadService, configuration, drugDbContext)
        {
        }

        /// <inheritdoc/>
        protected override async Task ProcessDownloadAsync(string sourceFolder, FileDownload downloadedFile, CancellationToken ct = default)
        {
            this.Logger.LogInformation("Parsing Drug File and adding to DB Context");
            IList<DrugProduct> drugProducts = this.Parser.ParseDrugFile(sourceFolder, downloadedFile);
            await this.DrugDbContext.DrugProduct.AddRangeAsync(drugProducts, ct);
            this.Logger.LogInformation("Parsing Other files and adding to DB Context");
            await this.DrugDbContext.ActiveIngredient.AddRangeAsync(this.Parser.ParseActiveIngredientFile(sourceFolder, drugProducts), ct);
            await this.DrugDbContext.Company.AddRangeAsync(this.Parser.ParseCompanyFile(sourceFolder, drugProducts), ct);
            await this.DrugDbContext.Status.AddRangeAsync(this.Parser.ParseStatusFile(sourceFolder, drugProducts), ct);
            await this.DrugDbContext.Form.AddRangeAsync(this.Parser.ParseFormFile(sourceFolder, drugProducts), ct);
            await this.DrugDbContext.Packaging.AddRangeAsync(this.Parser.ParsePackagingFile(sourceFolder, drugProducts), ct);
            await this.DrugDbContext.PharmaceuticalStd.AddRangeAsync(this.Parser.ParsePharmaceuticalStdFile(sourceFolder, drugProducts), ct);
            await this.DrugDbContext.Route.AddRangeAsync(this.Parser.ParseRouteFile(sourceFolder, drugProducts), ct);
            await this.DrugDbContext.Schedule.AddRangeAsync(this.Parser.ParseScheduleFile(sourceFolder, drugProducts), ct);
            await this.DrugDbContext.VeterinarySpecies.AddRangeAsync(this.Parser.ParseVeterinarySpeciesFile(sourceFolder, drugProducts), ct);
            await this.AddFileToDbAsync(downloadedFile, ct);
            this.Logger.LogInformation("Removing old Drug File from DB");
            this.RemoveOldFiles(downloadedFile);
            this.Logger.LogInformation("Saving Entities to the database");
            await this.DrugDbContext.SaveChangesAsync(ct);
        }
    }
}
