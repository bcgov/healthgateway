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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    public class DbDelegationDelegate(ILogger<DbDelegationDelegate> logger, GatewayDbContext dbContext) : IDelegationDelegate
    {
        /// <inheritdoc/>
        public async Task<Dependent?> GetDependentAsync(string hdid, bool includeAllowedDelegation = false, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving dependent from DB");

            IQueryable<Dependent> query = dbContext.Dependent
                .Where(d => d.HdId == hdid);

            if (includeAllowedDelegation)
            {
                query = query.Include(d => d.AllowedDelegations);
            }

            return await query.SingleOrDefaultAsync(ct);
        }

        /// <inheritdoc/>
        public async Task UpdateDelegationAsync(Dependent dependent, IEnumerable<ResourceDelegate> resourceDelegatesToRemove, AgentAudit agentAudit, bool commit = true, CancellationToken ct = default)
        {
            if (dependent.Version == 0)
            {
                logger.LogDebug("Adding dependent to DB");
                dbContext.Dependent.Add(dependent);
            }
            else
            {
                logger.LogDebug("Updating dependent in DB");
                dbContext.Dependent.Update(dependent);
            }

            foreach (ResourceDelegate resourceDelegate in resourceDelegatesToRemove)
            {
                logger.LogDebug("Removing resource delegate from DB for delegate {DelegateHdid}", resourceDelegate.ProfileHdid);
                dbContext.ResourceDelegate.Remove(resourceDelegate);
            }

            logger.LogDebug("Adding agent audit to DB");
            dbContext.AgentAudit.Add(agentAudit);

            if (commit)
            {
                await dbContext.SaveChangesAsync(ct);
            }
        }

        /// <inheritdoc/>
        public async Task<(IList<string> Hdids, int TotalHdids)> GetProtectedDependentHdidsAsync(int page, int pageSize, SortDirection sortDirection, CancellationToken ct = default)
        {
            int safePageSize = pageSize > 0 ? pageSize : 25;
            int recordsToSkip = int.Max(page, 0) * safePageSize;

            logger.LogDebug("Retrieving protected dependent HDIDs from DB, page #{PageNumber} with page size {PageSize}, sorted {SortDirection}", page, safePageSize, sortDirection);

            // Begin query for protected dependents only
            IQueryable<Dependent> query = dbContext.Dependent
                .Where(d => d.Protected);

            // Configure the sort direction
            query = sortDirection == SortDirection.Ascending
                ? query.OrderBy(d => d.HdId)
                : query.OrderByDescending(d => d.HdId);

            // Get the records for the page
            List<string> records = await query.Skip(recordsToSkip)
                .Take(safePageSize)
                .Select(d => d.HdId)
                .ToListAsync(ct);

            int totalCount = await dbContext.Dependent.CountAsync(d => d.Protected, ct);

            return (records, totalCount);
        }
    }
}
