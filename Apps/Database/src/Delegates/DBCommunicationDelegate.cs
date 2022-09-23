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
    public class DBCommunicationDelegate : ICommunicationDelegate
    {
        private const string BannerCommunicationOverlapMessage = "Banner post could not be added because there is an existing banner post.";
        private const string UniqueConstraintSqlStateError = "23P01";
        private const string UniqueConstraintDatetimeRange = "unique_date_range";
        private readonly ILogger<DBNoteDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCommunicationDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBCommunicationDelegate(
            ILogger<DBNoteDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public DBResult<Communication?> GetNext(CommunicationType communicationType)
        {
            this.logger.LogTrace("Getting next non-expired Communication from DB...");
            Communication? communication = this.dbContext.Communication
                .Where(
                    c => c.CommunicationTypeCode == communicationType &&
                         c.CommunicationStatusCode == CommunicationStatus.New &&
                         DateTime.UtcNow < c.ExpiryDateTime)
                .OrderBy(c => c.EffectiveDateTime)
                .FirstOrDefault();
            DBResult<Communication?> result = new()
            {
                Status = communication != null ? DBStatusCode.Read : DBStatusCode.NotFound,
                Payload = communication,
            };

            return result;
        }

        /// <inheritdoc/>
        public DBResult<Communication> Add(Communication communication, bool commit = true)
        {
            this.logger.LogTrace("Adding Communication to DB...");
            DBResult<Communication> result = new()
            {
                Payload = communication,
                Status = DBStatusCode.Deferred,
            };

            this.dbContext.Communication.Add(communication);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to save Communication to DB {Exception}", e.ToString());
                    result.Status = DBStatusCode.Error;
                    result.Message = IsUniqueConstraintDbError(e) ? BannerCommunicationOverlapMessage : e.Message;
                }
            }

            this.logger.LogDebug("Finished adding Communication in DB");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<IEnumerable<Communication>> GetAll()
        {
            this.logger.LogTrace("Getting all communication entries...");
            DBResult<IEnumerable<Communication>> result = new();
            result.Payload = this.dbContext.Communication
                .OrderBy(o => o.CreatedDateTime)
                .ToList();
            result.Status = result.Payload != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }

        /// <inheritdoc/>
        public DBResult<Communication> Update(Communication communication, bool commit = true)
        {
            this.logger.LogTrace("Updating Communication in DB...");
            DBResult<Communication> result = new()
            {
                Payload = communication,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.Communication.Update(communication);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    this.logger.LogError("Unable to update Communication to DB {Exception}", e.ToString());
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to update Communication to DB {Exception}", e.ToString());
                    result.Status = DBStatusCode.Error;
                    result.Message = IsUniqueConstraintDbError(e) ? BannerCommunicationOverlapMessage : e.Message;
                }
            }

            this.logger.LogDebug("Finished updating Communication in DB");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<Communication> Delete(Communication communication, bool commit = true)
        {
            this.logger.LogTrace("Deleting Communication from DB...");
            DBResult<Communication> result = new()
            {
                Payload = communication,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.Communication.Remove(communication);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Deleted;
                }
                catch (DbUpdateException e)
                {
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished deleting Communication in DB");
            return result;
        }

        private static bool IsUniqueConstraintDbError(DbUpdateException exception)
        {
            PostgresException? postgresException = exception.InnerException as PostgresException;
            if (postgresException?.SqlState == UniqueConstraintSqlStateError && postgresException?.ConstraintName == UniqueConstraintDatetimeRange)
            {
                return true;
            }

            return false;
        }
    }
}
