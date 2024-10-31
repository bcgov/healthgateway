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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of IDrugLookupDelegate that uses a DB connection for data management.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbDrugLookupDelegate(ILogger<DbDrugLookupDelegate> logger, GatewayDbContext dbContext) : IDrugLookupDelegate
    {
        /// <inheritdoc/>
        public async Task<IList<DrugProduct>> GetDrugProductsByDinAsync(IList<string> drugIdentifiers, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving drug products from DB");

            List<string> uniqueDrugIdentifiers = drugIdentifiers.Distinct().ToList();

            IList<DrugProduct> drugProducts = await dbContext.DrugProduct
                .Include(c => c.Company)
                .Include(a => a.ActiveIngredient)
                .Include(f => f.Form)
                .Where(dp => uniqueDrugIdentifiers.Contains(dp.DrugIdentificationNumber))
                .ToListAsync(ct);

            return drugProducts
                .GroupBy(dp => dp.DrugIdentificationNumber)
                .Select(drug => drug.OrderByDescending(o => o.LastUpdate).First())
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<IList<PharmaCareDrug>> GetPharmaCareDrugsByDinAsync(IList<string> drugIdentifiers, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving PharmaCare drug products from DB");

            List<string> uniqueDrugIdentifiers = drugIdentifiers.Distinct().ToList();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            IList<PharmaCareDrug> pharmaCareDrugs = await dbContext.PharmaCareDrug
                .Where(dp => uniqueDrugIdentifiers.Contains(dp.DinPin) && today > dp.EffectiveDate && today <= dp.EndDate)
                .ToListAsync(ct);

            return pharmaCareDrugs
                .GroupBy(pcd => pcd.DinPin)
                .Select(g => g.OrderByDescending(p => p.EndDate).First())
                .ToList();
        }
    }
}
