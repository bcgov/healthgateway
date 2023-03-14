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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class DbDelegationDelegate : IDelegationDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDelegationDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbDelegationDelegate(ILogger<DbDelegationDelegate> logger, GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public DbResult<AllowedDelegation> DeleteAllowedDelegation(AllowedDelegation allowedDelegation, bool commit = true)
        {
            DbResult<AllowedDelegation> result = new()
            {
                Payload = allowedDelegation,
                Status = DbStatusCode.Deferred,
            };
            this.dbContext.AllowedDelegation.Remove(allowedDelegation);

            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
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

        /// <inheritdoc/>
        public DbResult<IList<Dependent>> GetDependents(string hdid, bool includeAllowedDelegation = false)
        {
            this.logger.LogTrace("Getting dependents - includeAllowedDelegation : {IncludeAllowedDelegation}", includeAllowedDelegation);
            if (includeAllowedDelegation)
            {
                return new()
                {
                    Payload = this.dbContext.Dependent
                        .Where(d => d.HdId == hdid)
                        .Include(d => d.AllowedDelegations)
                        .OrderByDescending(d => d.CreatedDateTime)
                        .ToList(),
                    Status = DbStatusCode.Read,
                };
            }

            return new()
            {
                Payload = this.dbContext.Dependent
                    .Where(d => d.HdId == hdid)
                    .OrderByDescending(d => d.CreatedDateTime)
                    .ToList(),
                Status = DbStatusCode.Read,
            };
        }

        /// <inheritdoc/>
        public DbResult<Dependent> InsertDependent(Dependent dependent, bool commit = true)
        {
            DbResult<Dependent> result = new();
            this.dbContext.Dependent.Add(dependent);

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
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public DbResult<AllowedDelegation> InsertAllowedDelegation(AllowedDelegation allowedDelegation, bool commit = true)
        {
            DbResult<AllowedDelegation> result = new();
            this.dbContext.AllowedDelegation.Add(allowedDelegation);

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
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public DbResult<Dependent> UpdateDependent(Dependent dependent, bool commit = true)
        {
            this.dbContext.Dependent.Update(dependent);
            DbResult<Dependent> result = new();

            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DbStatusCode.Updated;
                    result.Payload = dependent;
                }
                catch (DbUpdateException e)
                {
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }
    }
}
