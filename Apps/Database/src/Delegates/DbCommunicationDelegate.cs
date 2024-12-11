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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Npgsql;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbCommunicationDelegate(ILogger<DbCommunicationDelegate> logger, GatewayDbContext dbContext) : ICommunicationDelegate
    {
        private const string BannerCommunicationOverlapMessage = "Banner post could not be added because there is an existing banner post.";
        private const string UniqueConstraintSqlStateError = "23P01";
        private const string UniqueConstraintDatetimeRange = "unique_date_range";

        /// <inheritdoc/>
        public async Task<Communication?> GetNextAsync(CommunicationType communicationType, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving next non-expired communication from DB for communication type {CommunicationType}", communicationType);
            return await dbContext.Communication
                .Where(
                    c => c.CommunicationTypeCode == communicationType &&
                         c.CommunicationStatusCode == CommunicationStatus.New &&
                         DateTime.UtcNow < c.ExpiryDateTime)
                .OrderBy(c => c.EffectiveDateTime)
                .FirstOrDefaultAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<DbResult<Communication>> AddAsync(Communication communication, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Adding communication to DB");

            DbResult<Communication> result = new()
            {
                Payload = communication,
                Status = DbStatusCode.Deferred,
            };

            dbContext.Communication.Add(communication);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Error adding communication to DB");
                    result.Status = DbStatusCode.Error;
                    result.Message = IsUniqueConstraintDbError(e) ? BannerCommunicationOverlapMessage : e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<Communication>> GetAllAsync(CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving communications from DB");
            return await dbContext.Communication
                .OrderBy(o => o.CreatedDateTime)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<DbResult<Communication>> UpdateAsync(Communication communication, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Updating communication in DB");

            DbResult<Communication> result = new()
            {
                Payload = communication,
                Status = DbStatusCode.Deferred,
            };

            dbContext.Communication.Update(communication);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error updating communication in DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Error updating communication in DB");
                    result.Status = DbStatusCode.Error;
                    result.Message = IsUniqueConstraintDbError(e) ? BannerCommunicationOverlapMessage : e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<Communication>> DeleteAsync(Communication communication, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Removing communication from DB");

            DbResult<Communication> result = new()
            {
                Payload = communication,
                Status = DbStatusCode.Deferred,
            };

            dbContext.Communication.Remove(communication);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Deleted;
                }
                catch (DbUpdateException e)
                {
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        private static bool IsUniqueConstraintDbError(DbUpdateException exception)
        {
            PostgresException? postgresException = exception.InnerException as PostgresException;
            return postgresException is { SqlState: UniqueConstraintSqlStateError, ConstraintName: UniqueConstraintDatetimeRange };
        }
    }
}
