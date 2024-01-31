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
    [ExcludeFromCodeCoverage]
    public class DbDrugLookupDelegate : IDrugLookupDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDrugLookupDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected logger provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbDrugLookupDelegate(ILogger<DbDrugLookupDelegate> logger, GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<IList<DrugProduct>> GetDrugProductsByDinAsync(IList<string> drugIdentifiers, CancellationToken ct = default)
        {
            this.logger.LogDebug("Getting list of drug products from DB");
            List<string> uniqueDrugIdentifiers = drugIdentifiers.Distinct().ToList();

            IList<DrugProduct> drugProducts = await this.dbContext.DrugProduct
                .Include(c => c.Company)
                .Include(a => a.ActiveIngredient)
                .Include(f => f.Form)
                .Where(dp => uniqueDrugIdentifiers.Contains(dp.DrugIdentificationNumber))
                .ToListAsync(ct);

            IList<DrugProduct> retVal = drugProducts
                .GroupBy(dp => dp.DrugIdentificationNumber)
                .Select(drug => drug.OrderByDescending(o => o.LastUpdate).First())
                .ToList();

            this.logger.LogDebug("Finished getting list of drug products from DB");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<IList<PharmaCareDrug>> GetPharmaCareDrugsByDinAsync(IList<string> drugIdentifiers, CancellationToken ct = default)
        {
            this.logger.LogDebug("Getting list of PharmaCare drug products from DB");
            List<string> uniqueDrugIdentifiers = drugIdentifiers.Distinct().ToList();
            DateTime now = DateTime.UtcNow;

            IList<PharmaCareDrug> pharmaCareDrugs = await this.dbContext.PharmaCareDrug
                .Where(dp => uniqueDrugIdentifiers.Contains(dp.DinPin) && now > dp.EffectiveDate && now <= dp.EndDate)
                .ToListAsync(ct);

            IList<PharmaCareDrug> retVal = pharmaCareDrugs
                .GroupBy(pcd => pcd.DinPin)
                .Select(g => g.OrderByDescending(p => p.EndDate).First())
                .ToList();

            this.logger.LogDebug("Finished getting list of PharmaCare drug products from DB");
            return retVal;
        }
    }
}
