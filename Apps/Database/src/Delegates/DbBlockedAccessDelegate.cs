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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class DbBlockedAccessDelegate : IBlockedAccessDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbBlockedAccessDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbBlockedAccessDelegate(ILogger<DbBlockedAccessDelegate> logger, GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task DeleteBlockedAccessAsync(BlockedAccess blockedAccess, AgentAudit agentAudit)
        {
            this.logger.LogDebug("Blocked access version: {Version} for hdid: {Hdid}", blockedAccess.Version, blockedAccess.Hdid);

            // Only attempt to remove entity if version is not 0
            if (blockedAccess.Version != 0)
            {
                this.dbContext.BlockedAccess.Remove(blockedAccess);
                this.logger.LogDebug("Blocked access removed for Hdid: {Hdid}", blockedAccess.Hdid);
            }

            this.dbContext.AgentAudit.Add(agentAudit);

            await this.dbContext.SaveChangesAsync().ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<BlockedAccess?> GetBlockedAccessAsync(string hdid)
        {
            IQueryable<BlockedAccess> query = this.dbContext.BlockedAccess
                .Where(d => d.Hdid == hdid);

            return await query.SingleOrDefaultAsync().ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DataSource>> GetDataSourcesAsync(string hdid)
        {
            IQueryable<BlockedAccess> query = this.dbContext.BlockedAccess.Where(d => d.Hdid == hdid);
            BlockedAccess? blockedAccess = await query.SingleOrDefaultAsync().ConfigureAwait(true);

            return blockedAccess?.DataSources ?? new HashSet<DataSource>();
        }

        /// <inheritdoc/>
        public async Task UpdateBlockedAccessAsync(BlockedAccess blockedAccess, AgentAudit agentAudit)
        {
            if (blockedAccess.Version == 0)
            {
                this.dbContext.BlockedAccess.Add(blockedAccess);
            }
            else
            {
                this.dbContext.BlockedAccess.Update(blockedAccess);
            }

            this.dbContext.AgentAudit.Add(agentAudit);

            await this.dbContext.SaveChangesAsync().ConfigureAwait(true);
        }
    }
}
