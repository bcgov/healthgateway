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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Npgsql;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DbResourceDelegateDelegate : IResourceDelegateDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbResourceDelegateDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbResourceDelegateDelegate(
            ILogger<DbFeedbackDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public DbResult<ResourceDelegate> Insert(ResourceDelegate resourceDelegate, bool commit)
        {
            this.logger.LogTrace("Inserting resource delegate to DB...");
            DbResult<ResourceDelegate> result = new()
            {
                Payload = resourceDelegate,
                Status = DbStatusCode.Deferred,
            };

            this.dbContext.Add(resourceDelegate);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    result.Status = DbStatusCode.Error;

                    if (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                    {
                        result.Message = "You have already added this dependent.";
                    }
                    else
                    {
                        this.logger.LogError("Error inserting resource delegate to DB with exception {Exception}", e.ToString());
                        result.Message = e.Message;
                    }
                }
            }

            this.logger.LogTrace("Finished inserting resource delegate to DB with id");
            return result;
        }

        /// <inheritdoc/>
        public DbResult<IEnumerable<ResourceDelegate>> Get(string delegateId, int page, int pageSize)
        {
            this.logger.LogTrace("Getting resource delegates from DB... {DelegateId}", delegateId);
            DbResult<IEnumerable<ResourceDelegate>> result = DbDelegateHelper.GetPagedDbResult(
                this.dbContext.ResourceDelegate
                    .Where(resourceDelegate => resourceDelegate.ProfileHdid == delegateId)
                    .OrderBy(resourceDelegate => resourceDelegate.CreatedDateTime),
                page,
                pageSize);
            this.logger.LogTrace("Finished getting resource delegates from DB for Id {DelegateId}", delegateId);
            return result;
        }

        /// <inheritdoc/>
        public IDictionary<DateTime, int> GetDailyDependentCount(TimeSpan offset)
        {
            this.logger.LogTrace("Counting resource delegates from DB...");
            Dictionary<DateTime, int> dateCount = this.dbContext.ResourceDelegate
                .Select(x => new { x.ProfileHdid, x.ResourceOwnerHdid, createdDate = GatewayDbContext.DateTrunc("days", x.CreatedDateTime.AddMinutes(offset.TotalMinutes)) })
                .GroupBy(x => x.createdDate)
                .Select(x => new { createdDate = x.Key, count = x.Count() })
                .OrderBy(x => x.createdDate)
                .ToDictionary(x => x.createdDate, x => x.count);
            this.logger.LogTrace("Finished counting resource delegates from DB...");

            return dateCount;
        }

        /// <inheritdoc/>
        public DbResult<ResourceDelegate> Delete(ResourceDelegate resourceDelegate, bool commit)
        {
            this.logger.LogTrace("Deleting resourceDelegate from DB...");
            DbResult<ResourceDelegate> result = new()
            {
                Status = DbStatusCode.Deferred,
            };
            this.dbContext.ResourceDelegate.Remove(resourceDelegate);

            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DbStatusCode.Deleted;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished deleting resourceDelegate from DB");
            return result;
        }

        /// <inheritdoc/>
        public bool Exists(string ownerId, string delegateId)
        {
            if (this.dbContext.ResourceDelegate.Any(rd => rd.ResourceOwnerHdid == ownerId && rd.ProfileHdid == delegateId))
            {
                return true;
            }

            return false;
        }
    }
}
