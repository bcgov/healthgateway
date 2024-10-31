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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbAdminTagDelegate(ILogger<DbAdminTagDelegate> logger, GatewayDbContext dbContext) : IAdminTagDelegate
    {
        /// <inheritdoc/>
        public async Task<DbResult<AdminTag>> AddAsync(AdminTag tag, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Adding admin tag to DB: {TagName}", tag.Name);

            DbResult<AdminTag> result = new()
            {
                Payload = tag,
                Status = DbStatusCode.Deferred,
            };

            dbContext.AdminTag.Add(tag);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Error adding admin tag to DB");
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<AdminTag>> DeleteAsync(AdminTag tag, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Removing admin tag from DB: {TagName}", tag.Name);

            DbResult<AdminTag> result = new()
            {
                Payload = tag,
                Status = DbStatusCode.Deferred,
            };

            dbContext.AdminTag.Remove(tag);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Deleted;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error removing admin tag from DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AdminTag>> GetAllAsync(CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving admin tags from DB");

            return await dbContext.AdminTag
                .OrderBy(o => o.Name)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<DbResult<IEnumerable<AdminTag>>> GetAdminTagsAsync(ICollection<Guid> adminTagIds, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving admin tags from DB matching tag IDs {AdminTagIds}", adminTagIds);

            DbResult<IEnumerable<AdminTag>> result = new()
            {
                Payload = await dbContext.AdminTag.Where(t => adminTagIds.Contains(t.AdminTagId)).ToListAsync(ct),
                Status = DbStatusCode.Read,
            };

            return result;
        }
    }
}
