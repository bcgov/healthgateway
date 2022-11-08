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
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of IDrugLookupDelegate that uses a DB connection for data management.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DbDrugLookupDelegate : IDrugLookupDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDrugLookupDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the databaase.</param>
        public DbDrugLookupDelegate(
            ILogger<DbDrugLookupDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IList<DrugProduct> GetDrugProductsByDin(IList<string> drugIdentifiers)
        {
            this.logger.LogDebug("Getting list of drug products from DB");
            IList<string> uniqueDrugIdentifers = drugIdentifiers.Distinct().ToList();
            IList<DrugProduct> retVal = this.dbContext.DrugProduct
                .Include(c => c.Company)
                .Include(a => a.ActiveIngredient)
                .Include(f => f.Form)
                .Where(dp => uniqueDrugIdentifers.Contains(dp.DrugIdentificationNumber))
                .AsEnumerable()
                .GroupBy(dp => dp.DrugIdentificationNumber)
                .Select(drug => drug.OrderByDescending(o => o.LastUpdate).First())
                .ToList();
            this.logger.LogDebug("Finished getting list of drug products from DB");
            return retVal;
        }

        /// <inheritdoc/>
        public IList<PharmaCareDrug> GetPharmaCareDrugsByDin(IList<string> drugIdentifiers)
        {
            this.logger.LogDebug("Getting list of pharmacare drug products from DB");
            IList<string> uniqueDrugIdentifers = drugIdentifiers.Distinct().ToList();
            DateTime now = DateTime.UtcNow;
            IList<PharmaCareDrug> retVal = this.dbContext.PharmaCareDrug
                .Where(dp => uniqueDrugIdentifers.Contains(dp.DINPIN) && now > dp.EffectiveDate && now <= dp.EndDate)
                .AsEnumerable()
                .GroupBy(pcd => pcd.DINPIN)
                .Select(g => g.OrderByDescending(p => p.EndDate).First())
                .ToList();

            this.logger.LogDebug("Finished getting list of pharmacare drug products from DB");
            return retVal;
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetDrugsBrandNameByDin(IList<string> drugIdentifiers)
        {
            this.logger.LogDebug("Getting drug brand names from DB");
            List<string> uniqueDrugIdentifers = drugIdentifiers.Distinct().ToList();
            this.logger.LogDebug("Total DrugIdentifiers: {Count} | Unique identifiers:{UniqueCount} ", drugIdentifiers.Count, uniqueDrugIdentifers.Count);

            // Retrieve the brand names using the Federal data
            IList<DrugProduct> drugProducts = this.GetDrugProductsByDin(uniqueDrugIdentifers);
            Dictionary<string, string> brandNames = drugProducts.ToDictionary(pcd => pcd.DrugIdentificationNumber, pcd => pcd.BrandName);

            if (uniqueDrugIdentifers.Count > brandNames.Count)
            {
                // Get the DINs not found on the previous query
                IList<string> notFoundDins = uniqueDrugIdentifers.Where(din => !brandNames.ContainsKey(din)).ToList();

                // Retrieve the brand names using the provincial data
                IList<PharmaCareDrug> pharmaCareDrugs = this.GetPharmaCareDrugsByDin(notFoundDins);

                Dictionary<string, string> provicialBrandNames = pharmaCareDrugs.ToDictionary(dp => dp.DINPIN, dp => dp.BrandName);

                // Merge both data sets
                provicialBrandNames.ToList().ForEach(x => brandNames.Add(x.Key, x.Value));
            }

            this.logger.LogDebug("Finished getting drug brand names from DB");
            return brandNames;
        }
    }
}
