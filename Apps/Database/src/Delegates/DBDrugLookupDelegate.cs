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
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of IDrugLookupDelegate that uses a DB connection for data management.
    /// </summary>
    public class DBDrugLookupDelegate : IDrugLookupDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBDrugLookupDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the databaase.</param>
        public DBDrugLookupDelegate(
            ILogger<DBDrugLookupDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public List<DrugProduct> GetDrugProductsByDIN(List<string> drugIdentifiers)
        {
            this.logger.LogTrace($"Getting list of drug products from DB... {JsonSerializer.Serialize(drugIdentifiers)}");
            List<DrugProduct> retVal = this.dbContext.DrugProduct.Where(dp => drugIdentifiers.Contains(dp.DrugIdentificationNumber))
                                        .Include(c => c.Company)
                                        .Include(a => a.ActiveIngredient)
                                        .Include(f => f.Form)
                        .ToList();

            this.logger.LogDebug($"Finished getting list of drug products from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc/>
        public List<PharmaCareDrug> GetPharmaCareDrugsByDIN(List<string> drugIdentifiers)
        {
            this.logger.LogTrace($"Getting list of pharmacare drug products from DB... {JsonSerializer.Serialize(drugIdentifiers)}");

            DateTime now = DateTime.UtcNow;
            List<PharmaCareDrug> retVal = this.dbContext.PharmaCareDrug
                .Where(dp => drugIdentifiers.Contains(dp.DINPIN) && (now > dp.EffectiveDate && now <= dp.EndDate)).ToList()
                .GroupBy(pcd => pcd.DINPIN).Select(g => g.OrderByDescending(p => p.EndDate).FirstOrDefault())
                .ToList();

            this.logger.LogDebug($"Finished getting list of pharmacare drug products from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetDrugsBrandNameByDIN(List<string> drugIdentifiers)
        {
            Contract.Requires(drugIdentifiers != null);
            this.logger.LogTrace($"Getting drug brand names from DB... {JsonSerializer.Serialize(drugIdentifiers)}");

            // Retrieve the brand names using the provincial data
            List<PharmaCareDrug> pharmaCareDrugs = this.GetPharmaCareDrugsByDIN(drugIdentifiers);
            Dictionary<string, string> provicialBrandNames = pharmaCareDrugs.ToDictionary(pcd => pcd.DINPIN, pcd => pcd.BrandName);

            if (drugIdentifiers.Count > provicialBrandNames.Count)
            {
                // Get the DINs not found on the previous query
                List<string> notFoundDins = drugIdentifiers.Where(din => !provicialBrandNames.Keys.Contains(din)).ToList();

                // Retrieve the brand names using the federal data
                List<DrugProduct> drugProducts = this.GetDrugProductsByDIN(notFoundDins);
                Dictionary<string, string> federalBrandNames = drugProducts.ToDictionary(dp => dp.DrugIdentificationNumber, dp => dp.BrandName);

                // Merge both data sets
                federalBrandNames.ToList().ForEach(x => provicialBrandNames.Add(x.Key, x.Value));
            }

            this.logger.LogDebug($"Finished getting drug brand names from DB. {JsonSerializer.Serialize(provicialBrandNames)}");
            return provicialBrandNames;
        }
    }
}