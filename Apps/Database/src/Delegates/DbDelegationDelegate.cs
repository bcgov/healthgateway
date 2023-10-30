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
    public class DbDelegationDelegate : IDelegationDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDelegationDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbDelegationDelegate(ILogger<DbDelegationDelegate> logger, GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<Dependent?> GetDependentAsync(string hdid, bool includeAllowedDelegation = false, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting dependent - includeAllowedDelegation : {IncludeAllowedDelegation}", includeAllowedDelegation.ToString());

            IQueryable<Dependent> query = this.dbContext.Dependent
                .Where(d => d.HdId == hdid);

            if (includeAllowedDelegation)
            {
                query = query.Include(d => d.AllowedDelegations);
            }

            return await query.SingleOrDefaultAsync(ct);
        }

        /// <inheritdoc/>
        public async Task UpdateDelegationAsync(Dependent dependent, IEnumerable<ResourceDelegate> resourceDelegatesToRemove, AgentAudit agentAudit, bool commit = true)
        {
            if (dependent.Version == 0)
            {
                this.dbContext.Dependent.Add(dependent);
            }
            else
            {
                this.dbContext.Dependent.Update(dependent);
            }

            foreach (ResourceDelegate resourceDelegate in resourceDelegatesToRemove)
            {
                this.dbContext.ResourceDelegate.Remove(resourceDelegate);
            }

            this.dbContext.AgentAudit.Add(agentAudit);

            if (commit)
            {
                await this.dbContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<(IList<string> Hdids, int TotalHdids)> GetProtectedDependentHdidsAsync(int page, int pageSize, SortDirection sortDirection, CancellationToken ct)
        {
            int safePageSize = pageSize > 0 ? pageSize : 25;
            int recordsToSkip = int.Max(page, 0) * safePageSize;

            // Begin query for protected dependents only
            IQueryable<Dependent> query = this.dbContext.Dependent
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
            int totalCount = await this.dbContext.Dependent.CountAsync(d => d.Protected, ct);
            return (records, totalCount);
        }
    }
}
