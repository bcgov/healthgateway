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
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBResourceDelegateDelegate : IResourceDelegateDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBResourceDelegateDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBResourceDelegateDelegate(
            ILogger<DBFeedbackDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<ResourceDelegate> Insert(ResourceDelegate resourceDelegate, bool commit = true)
        {
            this.logger.LogTrace($"Inserting resource delegate to DB... {JsonSerializer.Serialize(resourceDelegate)}");
            DBResult<ResourceDelegate> result = new DBResult<ResourceDelegate>()
            {
                Payload = resourceDelegate,
                Status = DBStatusCode.Deferred,
            };

            this.dbContext.Add<ResourceDelegate>(resourceDelegate);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError($"Error inserting resource delegate to DB with exception ({e.ToString()})");
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogTrace($"Finished inserting resource delegate to DB... {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<IEnumerable<ResourceDelegate>> Get(string delegateId, int page, int pageSize)
        {
            this.logger.LogTrace($"Getting resource delegates from DB... {delegateId}");
            var result = DBDelegateHelper.GetPagedDBResult(
                this.dbContext.ResourceDelegate
                    .Where(resourceDelegate => resourceDelegate.ProfileHdid == delegateId),
                page,
                pageSize);
            this.logger.LogTrace($"Finished getting resource delegates from DB... {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<ResourceDelegate> Delete(ResourceDelegate resourceDelegate, bool commit)
        {
            this.logger.LogTrace($"Deleting resourceDelegate {JsonSerializer.Serialize(resourceDelegate)} from DB...");
            DBResult<ResourceDelegate> result = new DBResult<ResourceDelegate>()
            {
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.ResourceDelegate.Remove(resourceDelegate);

            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Deleted;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished deleting resourceDelegate from DB");
            return result;
        }

        /// <inheritdoc />
        public bool Exists(string ownerId, string delegateId)
        {
            var resourceDelegate = this.dbContext.ResourceDelegate.Find(ownerId, delegateId);
            if (resourceDelegate != null)
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
