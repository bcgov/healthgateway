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
    public class DbAgentAuditDelegate : IAgentAuditDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbAgentAuditDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbAgentAuditDelegate(ILogger<DbAgentAuditDelegate> logger, GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AgentAudit>> GetAgentAuditsAsync(string hdid, AuditGroup? group = null, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting agent audit for group: {Group} - hdid : {Hdid}", group, hdid);

            IQueryable<AgentAudit> dbQuery = this.dbContext.AgentAudit
                .Where(a => a.Hdid == hdid);

            if (group != null)
            {
                dbQuery = dbQuery.Where(d => d.GroupCode == group);
            }

            return await dbQuery.ToListAsync(ct).ConfigureAwait(true);
        }
    }
}
