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
    public class DBUserDelegateDelegate : IUserDelegateDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBUserDelegateDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBUserDelegateDelegate(
            ILogger<DBFeedbackDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<UserDelegate> Insert(UserDelegate userDelegate, bool commit = true)
        {
            this.logger.LogTrace($"Inserting user delegate to DB... {JsonSerializer.Serialize(userDelegate)}");
            DBResult<UserDelegate> result = new DBResult<UserDelegate>()
            {
                Payload = userDelegate,
                Status = DBStatusCode.Deferred,
            };

            this.dbContext.Add<UserDelegate>(userDelegate);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError($"Error inserting user delegate to DB with exception ({e.ToString()})");
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogTrace($"Finished inserting user delegate to DB... {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<IEnumerable<UserDelegate>> Get(string delegateId, int page, int pageSize)
        {
            this.logger.LogTrace($"Getting user delegates from DB... {delegateId}");
            var result = DBDelegateHelper.GetPagedDBResult(
                this.dbContext.UserDelegate
                    .Where(dependent => dependent.DelegateId == delegateId),
                page,
                pageSize);
            this.logger.LogTrace($"Finished getting user delegates from DB... {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<UserDelegate> Delete(string ownerId, string delegateId, bool commit)
        {
            this.logger.LogTrace($"Deleting UserDelegate (ownerId: {ownerId}, delegateId: {delegateId}) from DB...");
            UserDelegate userDelegate = new UserDelegate()
            {
                OwnerId = ownerId,
                DelegateId = delegateId,
            };
            DBResult<UserDelegate> result = new DBResult<UserDelegate>()
            {
                Payload = userDelegate,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.UserDelegate.Remove(userDelegate);

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

            this.logger.LogDebug($"Finished deleting UserDelegate from DB");
            return result;
        }

        /// <inheritdoc />
        public bool Exists(string ownerId, string delegateId)
        {
            var userDelegate = this.dbContext.UserDelegate.Find(delegateId, ownerId);
            if (userDelegate != null)
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
