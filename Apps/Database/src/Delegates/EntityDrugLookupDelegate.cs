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
    using System.Linq;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Implementation of IDrugLookupDelegate that uses a DB connection for data management.
    /// </summary>
    public class EntityDrugLookupDelegate : IDrugLookupDelegate
    {
        private readonly DrugDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDrugLookupDelegate"/> class.
        /// </summary>
        /// <param name="dbContext">The context to be used when accessing the databaase.</param>
        public EntityDrugLookupDelegate(DrugDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public List<DrugProduct> GetDrugProductsByDIN(List<string> drugIdentifiers)
        {
            return this.dbContext.DrugProduct.Where(dp => drugIdentifiers.Contains(dp.DrugIdentificationNumber)).ToList();
        }

        /// <inheritdoc/>
        public List<Form> GetFormByDrugProductId(System.Guid drugProductId)
        {
            return this.dbContext.Form.Where(c => c.DrugProductId == drugProductId).ToList();
        }

        /// <inheritdoc/>
        public List<ActiveIngredient> GetActiveIngredientByDrugProductId(System.Guid drugProductId)
        {
            return this.dbContext.ActiveIngredient.Where(c => c.DrugProductId == drugProductId).ToList();
        }

        /// <inheritdoc/>
        public List<Company> GetCompanyByDrugProductId(System.Guid drugProductId)
        {
            return this.dbContext.Company.Where(c => c.DrugProductId == drugProductId).ToList();
        }

        /// <inheritdoc/>
        public List<PharmaCareDrug> GetPharmaCareDrugsByDIN(List<string> drugIdentifiers)
        {
            DateTime now = DateTime.UtcNow;
            return this.dbContext.PharmaCareDrug
                .Where(dp => drugIdentifiers.Contains(dp.DINPIN) && (now > dp.EffectiveDate && now <= dp.EndDate))
                .GroupBy(pcd => pcd.DINPIN).Select(g => g.OrderByDescending(p => p.EndDate).FirstOrDefault())
                .ToList();
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetDrugsBrandNameByDIN(List<string> drugIdentifiers)
        {
            // Retrieve the brand names using the provincial data
            List<PharmaCareDrug> pharmaCareDrugs = GetPharmaCareDrugsByDIN(drugIdentifiers);
            Dictionary<string, string> provicialBrandNames = pharmaCareDrugs.ToDictionary(pcd => pcd.DINPIN, pcd => pcd.BrandName);

            if (drugIdentifiers.Count > provicialBrandNames.Count())
            {
                // Get the DINs not found on the previous query
                List<string> notFoundDins = drugIdentifiers.Where(din => !provicialBrandNames.Keys.Contains(din)).ToList();

                // Retrieve the brand names using the federal data
                List<DrugProduct> drugProducts = GetDrugProductsByDIN(notFoundDins);
                Dictionary<string, string> federalBrandNames = drugProducts.ToDictionary(dp => dp.DrugIdentificationNumber, dp => dp.BrandName);

                // Merge both data sets
                federalBrandNames.ToList().ForEach(x => provicialBrandNames.Add(x.Key, x.Value));
            }

            return provicialBrandNames;
        }
    }
}