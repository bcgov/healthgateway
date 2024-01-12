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
    [ExcludeFromCodeCoverage]
    public class DbCommunicationDelegate : ICommunicationDelegate
    {
        private const string BannerCommunicationOverlapMessage = "Banner post could not be added because there is an existing banner post.";
        private const string UniqueConstraintSqlStateError = "23P01";
        private const string UniqueConstraintDatetimeRange = "unique_date_range";
        private readonly ILogger<DbCommunicationDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbCommunicationDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbCommunicationDelegate(ILogger<DbCommunicationDelegate> logger, GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<Communication?> GetNextAsync(CommunicationType communicationType, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting next non-expired Communication from DB...");
            return await this.dbContext.Communication
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
            this.logger.LogTrace("Adding Communication to DB...");
            DbResult<Communication> result = new()
            {
                Payload = communication,
                Status = DbStatusCode.Deferred,
            };

            this.dbContext.Communication.Add(communication);
            if (commit)
            {
                try
                {
                    await this.dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to save Communication to DB {Exception}", e.ToString());
                    result.Status = DbStatusCode.Error;
                    result.Message = IsUniqueConstraintDbError(e) ? BannerCommunicationOverlapMessage : e.Message;
                }
            }

            this.logger.LogDebug("Finished adding Communication in DB");
            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<Communication>> GetAllAsync(CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting all communication entries...");
            return await this.dbContext.Communication
                .OrderBy(o => o.CreatedDateTime)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<DbResult<Communication>> UpdateAsync(Communication communication, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Updating Communication in DB...");
            DbResult<Communication> result = new()
            {
                Payload = communication,
                Status = DbStatusCode.Deferred,
            };
            this.dbContext.Communication.Update(communication);
            if (commit)
            {
                try
                {
                    await this.dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    this.logger.LogError("Unable to update Communication to DB {Exception}", e.ToString());
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to update Communication to DB {Exception}", e.ToString());
                    result.Status = DbStatusCode.Error;
                    result.Message = IsUniqueConstraintDbError(e) ? BannerCommunicationOverlapMessage : e.Message;
                }
            }

            this.logger.LogDebug("Finished updating Communication in DB");
            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<Communication>> DeleteAsync(Communication communication, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Deleting Communication from DB...");
            DbResult<Communication> result = new()
            {
                Payload = communication,
                Status = DbStatusCode.Deferred,
            };
            this.dbContext.Communication.Remove(communication);
            if (commit)
            {
                try
                {
                    await this.dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Deleted;
                }
                catch (DbUpdateException e)
                {
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished deleting Communication in DB");
            return result;
        }

        private static bool IsUniqueConstraintDbError(DbUpdateException exception)
        {
            PostgresException? postgresException = exception.InnerException as PostgresException;
            return postgresException is { SqlState: UniqueConstraintSqlStateError, ConstraintName: UniqueConstraintDatetimeRange };
        }
    }
}
