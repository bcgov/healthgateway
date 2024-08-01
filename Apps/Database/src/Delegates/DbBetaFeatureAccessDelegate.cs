// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;

    /// <inheritdoc/>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    public class DbBetaFeatureAccessDelegate(GatewayDbContext dbContext) : IBetaFeatureAccessDelegate
    {
        /// <inheritdoc/>
        public async Task AddRangeAsync(IEnumerable<BetaFeatureAccess> betaFeatureAccessAssociations, bool commit = true, CancellationToken ct = default)
        {
            dbContext.BetaFeatureAccess.AddRange(betaFeatureAccessAssociations);
            if (commit)
            {
                await dbContext.SaveChangesAsync(ct);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteRangeAsync(IEnumerable<BetaFeatureAccess> betaFeatureAccessAssociations, bool commit = true, CancellationToken ct = default)
        {
            dbContext.BetaFeatureAccess.RemoveRange(betaFeatureAccessAssociations);
            if (commit)
            {
                await dbContext.SaveChangesAsync(ct);
            }
        }

        /// <inheritdoc/>
        public async Task<IList<BetaFeatureAccess>> GetAsync(IEnumerable<string> hdids, CancellationToken ct = default)
        {
            return await dbContext.BetaFeatureAccess
                .Where(x => hdids.Contains(x.Hdid))
                .OrderBy(x => x.Hdid)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<PaginatedResult<IGrouping<string, BetaFeatureAccess>>> GetAllAsync(int pageIndex, int pageSize, CancellationToken ct = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(pageIndex);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

            IQueryable<string> emailQuery = dbContext.BetaFeatureAccess
                .Where(p => p.UserProfile.Email != null)
                .Select(p => p.UserProfile.Email!)
                .Distinct();

            int totalCount = await emailQuery.CountAsync(ct);

            IList<string> emailAddresses = await emailQuery
                .OrderBy(p => p)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            List<BetaFeatureAccess> data = await dbContext.BetaFeatureAccess
                .Include(p => p.UserProfile)
                .Where(p => p.UserProfile.Email != null)
                .Where(p => emailAddresses.Contains(p.UserProfile.Email))
                .ToListAsync(ct);

            return new() { Data = data.GroupBy(p => p.UserProfile.Email!).ToList(), PageIndex = pageIndex, PageSize = pageSize, TotalCount = totalCount };
        }
    }
}
