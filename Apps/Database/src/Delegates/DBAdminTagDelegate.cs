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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DBAdminTagDelegate : IAdminTagDelegate
    {
        private readonly ILogger<DBAdminTagDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBAdminTagDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBAdminTagDelegate(
            ILogger<DBAdminTagDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public DBResult<AdminTag> Add(AdminTag tag, bool commit = true)
        {
            this.logger.LogTrace("Adding AdminTag to DB...");
            DBResult<AdminTag> result = new()
            {
                Payload = tag,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.AdminTag.Add(tag);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to save AdminTag to DB {Exception}", e.ToString());
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished adding AdminTag to DB");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<AdminTag> Delete(AdminTag tag, bool commit = true)
        {
            this.logger.LogTrace("Deleting AdminTag from DB...");
            DBResult<AdminTag> result = new()
            {
                Payload = tag,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.AdminTag.Remove(tag);
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

            this.logger.LogDebug("Finished deleting AdminTag in DB");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<IEnumerable<AdminTag>> GetAll()
        {
            this.logger.LogTrace("Getting all AdminTag from DB...");
            DBResult<IEnumerable<AdminTag>> result = new();
            result.Payload = this.dbContext.AdminTag
                .OrderBy(o => o.Name)
                .ToList();
            result.Status = result.Payload != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }

        /// <inheritdoc/>
        public DBResult<IEnumerable<AdminTag>> GetAdminTags(ICollection<Guid> adminTagIds)
        {
            this.logger.LogTrace("Getting admin tags from DB for Admin Tag Ids: {AdminTagId}", adminTagIds.ToString());
            DBResult<IEnumerable<AdminTag>> result = new();
            result.Payload = this.dbContext.AdminTag.Where(t => adminTagIds.Contains(t.AdminTagId)).ToList();
            result.Status = DBStatusCode.Read;
            return result;
        }
    }
}
