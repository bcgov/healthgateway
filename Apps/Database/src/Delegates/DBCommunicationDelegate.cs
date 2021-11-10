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
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Npgsql;

    /// <inheritdoc />
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

        /// <inheritdoc />
        public DBResult<Communication> GetActiveBanner(CommunicationType communicationType)
        {
            this.logger.LogTrace($"Getting active Communication from DB...");
            DBResult<Communication> result = new DBResult<Communication>()
            {
                Status = DBStatusCode.NotFound,
            };
            Communication? communication = this.dbContext.Communication
                .OrderByDescending(c => c.CreatedDateTime)
                .Where(c => c.CommunicationTypeCode == communicationType)
                .Where(c => c.CommunicationStatusCode == CommunicationStatus.New)
                .Where(c => DateTime.UtcNow >= c.EffectiveDateTime && DateTime.UtcNow <= c.ExpiryDateTime)
                .FirstOrDefault();

            if (communication != null)
            {
                result.Status = DBStatusCode.Read;
                result.Payload = communication;
            }

            return result;
        }

        /// <inheritdoc />
        public DBResult<Communication> Add(Communication communication, bool commit = true)
        {
            this.logger.LogTrace($"Adding Communication to DB...");
            DBResult<Communication> result = new DBResult<Communication>()
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
                    this.logger.LogError($"Unable to save Communication to DB {e}");
                    result.Status = DBStatusCode.Error;
                    result.Message = IsUniqueConstraintDbError(e) ? BannerCommunicationOverlapMessage : e.Message;
                }
            }

            this.logger.LogDebug($"Finished adding Communication in DB");
            return result;
        }

        /// <inheritdoc />
        public DBResult<IEnumerable<Communication>> GetAll()
        {
            this.logger.LogTrace($"Getting all communication entries...");
            DBResult<IEnumerable<Communication>> result = new DBResult<IEnumerable<Communication>>();
            result.Payload = this.dbContext.Communication
                    .OrderBy(o => o.CreatedDateTime)
                    .ToList();
            result.Status = result.Payload != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }

        /// <inheritdoc />
        public DBResult<Communication> Update(Communication communication, bool commit = true)
        {
            this.logger.LogTrace($"Updating Communication in DB...");
            DBResult<Communication> result = new DBResult<Communication>()
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
                    this.logger.LogError($"Unable to update Communication to DB {e}");
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError($"Unable to update Communication to DB {e}");
                    result.Status = DBStatusCode.Error;
                    result.Message = IsUniqueConstraintDbError(e) ? BannerCommunicationOverlapMessage : e.Message;
                }
            }

            this.logger.LogDebug($"Finished updating Communication in DB");
            return result;
        }

        /// <inheritdoc />
        public IList<Communication> GetEmailCommunicationsToSend()
        {
            this.logger.LogTrace($"Getting Communications by Type and Status Code from DB...");
            IList<Communication> retVal = this.dbContext.Communication.Where(c =>
                c.CommunicationTypeCode == CommunicationType.Email &&
                c.CommunicationStatusCode != CommunicationStatus.Draft &&
                c.CommunicationStatusCode != CommunicationStatus.Processed &&
                c.ScheduledDateTime <= DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedDateTime).ToList();
            this.logger.LogDebug($"Finished getting list of New & Processing Email Communications from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public DBResult<Communication> Delete(Communication communication, bool commit = true)
        {
            this.logger.LogTrace($"Deleting Communication from DB...");
            DBResult<Communication> result = new DBResult<Communication>()
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

            this.logger.LogDebug($"Finished deleting Communication in DB");
            return result;
        }

        private static bool IsUniqueConstraintDbError(DbUpdateException exception)
        {
            PostgresException? postgresException = exception.InnerException as PostgresException;
            if (postgresException?.SqlState == UniqueConstraintSqlStateError && postgresException?.ConstraintName == UniqueConstraintDatetimeRange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
