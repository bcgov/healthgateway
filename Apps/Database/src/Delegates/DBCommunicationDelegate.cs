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
    using System.Linq;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBCommunicationDelegate : ICommunicationDelegate
    {
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
        public DBResult<Communication> GetActive()
        {
            this.logger.LogTrace($"Getting active Communication from DB...");
            DBResult<Communication> result = new DBResult<Communication>()
            {
                Status = DBStatusCode.NotFound,
            };
            result.Payload = this.dbContext.Communication
                .OrderByDescending(c => c.CreatedDateTime)
                .FirstOrDefault(c =>
                    DateTime.UtcNow >= c.EffectiveDateTime && DateTime.UtcNow <= c.ExpiryDateTime);

            if (result.Payload != null)
            {
                result.Status = DBStatusCode.Read;
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
            if (this.IsOverlappingDate(communication))
            {
                this.logger.LogDebug($"Communication date range overlap with existing entry");
                result.Message = "Communication Effective and Expiry Dates overlap with existing record";
                result.Status = DBStatusCode.Error;
                return result;
            }

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
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished adding Communication in DB");
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
            if (this.IsOverlappingDate(communication))
            {
                this.logger.LogDebug($"Communication date range overlap with existing entry");
                result.Status = DBStatusCode.Error;
                return result;
            }

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
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished updating Communication in DB");
            return result;
        }

        private bool IsOverlappingDate(Communication communication)
        {
            return this.dbContext.Communication.Any(c =>
                communication.EffectiveDateTime < c.ExpiryDateTime &&
                c.EffectiveDateTime < communication.ExpiryDateTime &&
                c.Id != communication.Id);
        }
    }
}
