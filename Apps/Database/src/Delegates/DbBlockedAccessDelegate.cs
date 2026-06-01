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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    public class DbBlockedAccessDelegate(ILogger<DbBlockedAccessDelegate> logger, GatewayDbContext dbContext) : IBlockedAccessDelegate
    {
        /// <inheritdoc/>
        public async Task DeleteAsync(BlockedAccess blockedAccess, AgentAudit agentAudit, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Removing blocked access from DB for {Hdid}", blockedAccess.Hdid);

            // only attempt to remove entity if it exists in the DB
            if (blockedAccess.Version != 0)
            {
                dbContext.BlockedAccess.Remove(blockedAccess);
            }

            logger.LogDebug("Adding audit record to DB");
            dbContext.AgentAudit.Add(agentAudit);

            if (commit)
            {
                await dbContext.SaveChangesAsync(ct);
            }
        }

        /// <inheritdoc/>
        public async Task<BlockedAccess?> GetAsync(string hdid, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving blocked access from DB for {Hdid}", hdid);
            return await dbContext.BlockedAccess
                .Where(d => d.Hdid == hdid)
                .SingleOrDefaultAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<BlockedAccess?> GetNoTrackingAsync(string hdid, CancellationToken ct = default)
        {
            return await dbContext.BlockedAccess
                .AsNoTracking()
                .Where(d => d.Hdid == hdid)
                .SingleOrDefaultAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DataSource>> GetDataSourcesAsync(string hdid, CancellationToken ct = default)
        {
            BlockedAccess? blockedAccess = await this.GetNoTrackingAsync(hdid, ct);
            return blockedAccess?.DataSources ?? [];
        }

        /// <inheritdoc/>
        public async Task UpsertAsync(BlockedAccess blockedAccess, AgentAudit agentAudit, bool commit = true, CancellationToken ct = default)
        {
            // Ensure the DataSources list remains logically unique.
            // This property was previously a HashSet<DataSource>, which prevented duplicates automatically.
            // Since EF Core JSON mapping now requires List<T>, duplicates could appear.
            // Remove duplicates before saving while avoiding unnecessary updates when the list is already unique.
            List<DataSource> distinctSources = blockedAccess.DataSources
                .Distinct()
                .ToList();

            if (distinctSources.Count != blockedAccess.DataSources.Count)
            {
                blockedAccess.DataSources = distinctSources;
            }

            EntityEntry<BlockedAccess> entry = dbContext.Entry(blockedAccess);

            if (entry.State == EntityState.Detached)
            {
                if (blockedAccess.Version == 0)
                {
                    logger.LogDebug("Adding blocked access to DB for {Hdid}", blockedAccess.Hdid);
                    dbContext.BlockedAccess.Add(blockedAccess);
                }
                else
                {
                    logger.LogDebug("Updating blocked access in DB for {Hdid}", blockedAccess.Hdid);
                    dbContext.BlockedAccess.Update(blockedAccess);
                }
            }

            logger.LogDebug("Adding audit record to DB");
            dbContext.AgentAudit.Add(agentAudit);

            if (commit)
            {
                await dbContext.SaveChangesAsync(ct);
            }
        }

        /// <inheritdoc/>
        public async Task<IList<BlockedAccess>> GetAllNoTrackingAsync(CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving blocked access from DB");
            return await dbContext.BlockedAccess
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}
