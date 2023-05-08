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
    using System.Threading.Tasks;
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
        public async Task<Dependent?> GetDependentAsync(string hdid, bool includeAllowedDelegation = false)
        {
            this.logger.LogTrace("Getting dependent - includeAllowedDelegation : {IncludeAllowedDelegation}", includeAllowedDelegation.ToString());

            IQueryable<Dependent> query = this.dbContext.Dependent
                .Where(d => d.HdId == hdid);

            if (includeAllowedDelegation)
            {
                query = query.Include(d => d.AllowedDelegations);
            }

            return await query.SingleOrDefaultAsync().ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<AgentAuditQueryResult> GetAgentAuditsAsync(AgentAuditQuery query)
        {
            this.logger.LogTrace("Getting agent audit for group: {Group} - hdid : {Hdid}", query.GroupCode, query.Hdid);

            IQueryable<AgentAudit> dbQuery = this.dbContext.AgentAudit;
            dbQuery = dbQuery.Where(d => d.Hdid == query.Hdid && d.GroupCode == query.GroupCode);
            IEnumerable<AgentAudit> items = await dbQuery.ToListAsync().ConfigureAwait(true);

            return new AgentAuditQueryResult
            {
                Items = items,
            };
        }

        /// <inheritdoc/>
        public async Task UpdateDelegationAsync(Dependent dependent, IEnumerable<ResourceDelegate> resourceDelegatesToRemove, AgentAudit agentAudit)
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

            await this.dbContext.SaveChangesAsync().ConfigureAwait(true);
        }
    }
}
