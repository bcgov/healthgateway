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
    using System.Threading;
    using System.Threading.Tasks;
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
        private readonly ILogger<DbResourceDelegateDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbResourceDelegateDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbResourceDelegateDelegate(
            ILogger<DbResourceDelegateDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<DbResult<ResourceDelegate>> InsertAsync(ResourceDelegate resourceDelegate, bool commit, CancellationToken ct = default)
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
                    await this.dbContext.SaveChangesAsync(ct);
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
                        this.logger.LogError(e, "Error inserting resource delegate to DB with exception {Message}", e.Message);
                        result.Message = e.Message;
                    }
                }
            }

            this.logger.LogTrace("Finished inserting resource delegate to DB with id");
            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<ResourceDelegate>> GetAsync(string delegateId, int page = 0, int pageSize = 500, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting resource delegates from DB... {DelegateId}", delegateId);
            return await DbDelegateHelper.GetPagedDbResultAsync(
                this.dbContext.ResourceDelegate
                    .Where(resourceDelegate => resourceDelegate.ProfileHdid == delegateId)
                    .OrderBy(resourceDelegate => resourceDelegate.CreatedDateTime),
                page,
                pageSize,
                ct);
        }

        /// <inheritdoc/>
        public async Task<IList<ResourceDelegate>> GetAsync(DateTime fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting resource delegates from DB for date...{FromDate}", fromDate);
            toDate ??= DateTime.MaxValue;

            return await DbDelegateHelper.GetPagedDbResultAsync(
                this.dbContext.ResourceDelegate
                    .Where(resourceDelegate => resourceDelegate.CreatedDateTime >= fromDate && resourceDelegate.CreatedDateTime <= toDate)
                    .OrderBy(resourceDelegate => resourceDelegate.CreatedDateTime),
                page,
                pageSize,
                ct);
        }

        /// <inheritdoc/>
        public async Task<int> GetDependentCountAsync(CancellationToken ct = default)
        {
            return await this.dbContext.ResourceDelegate.Where(x => x.ReasonCode == ResourceDelegateReason.Guardian).CountAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<DateOnly, int>> GetDailyDependentRegistrationCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default)
        {
            return await this.dbContext.ResourceDelegate
                .Where(x => x.ReasonCode == ResourceDelegateReason.Guardian && x.CreatedDateTime >= startDateTimeOffset.UtcDateTime && x.CreatedDateTime <= endDateTimeOffset.UtcDateTime)
                .Select(x => new { x.ProfileHdid, x.ResourceOwnerHdid, createdDate = x.CreatedDateTime.AddMinutes(startDateTimeOffset.TotalOffsetMinutes).Date })
                .GroupBy(x => x.createdDate)
                .Select(x => new { createdDate = x.Key, count = x.Count() })
                .ToDictionaryAsync(x => DateOnly.FromDateTime(x.createdDate), x => x.count, ct);
        }

        /// <inheritdoc/>
        public async Task<DbResult<Dictionary<string, int>>> GetTotalDelegateCountsAsync(IEnumerable<string> dependentHdids, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting total delegate counts from DB...");
            string[] dependentArray = dependentHdids.ToArray();
            DbResult<Dictionary<string, int>> result = new()
            {
                Payload = await this.dbContext.ResourceDelegate
                    .Where(d => dependentArray.Contains(d.ResourceOwnerHdid))
                    .GroupBy(d => d.ResourceOwnerHdid)
                    .ToDictionaryAsync(g => g.Key, g => g.Count(), ct),
                Status = DbStatusCode.Read,
            };
            this.logger.LogTrace("Finished getting total delegate counts from DB");
            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<ResourceDelegate>> DeleteAsync(ResourceDelegate resourceDelegate, bool commit, CancellationToken ct = default)
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
                    await this.dbContext.SaveChangesAsync(ct);
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
        public async Task<bool> ExistsAsync(string ownerId, string delegateId, CancellationToken ct = default)
        {
            return await this.dbContext.ResourceDelegate.AnyAsync(rd => rd.ResourceOwnerHdid == ownerId && rd.ProfileHdid == delegateId, ct);
        }

        /// <inheritdoc/>
        public async Task<ResourceDelegateQueryResult> SearchAsync(ResourceDelegateQuery query, CancellationToken ct = default)
        {
            IQueryable<ResourceDelegate> dbQuery = this.dbContext.ResourceDelegate;
            if (query.ByOwnerHdid != null)
            {
                dbQuery = dbQuery.Where(rd => rd.ResourceOwnerHdid == query.ByOwnerHdid);
            }

            if (query.ByDelegateHdid != null)
            {
                dbQuery = dbQuery.Where(rd => rd.ProfileHdid == query.ByDelegateHdid);
            }

            if (query.IncludeProfile)
            {
                dbQuery = dbQuery.Include(rd => rd.UserProfile).OrderByDescending(rd => rd.UserProfile.LastLoginDateTime);
            }

            if (query.TakeAmount != null)
            {
                dbQuery = dbQuery.Take(query.TakeAmount.Value);
            }

            IList<ResourceDelegateQueryResultItem> items;
            if (query.IncludeDependent)
            {
                items = await dbQuery.GroupJoin(
                        this.dbContext.Dependent,
                        rd => rd.ResourceOwnerHdid,
                        d => d.HdId,
                        (rd, d) => new ResourceDelegateQueryResultItem { ResourceDelegate = rd, Dependent = d.FirstOrDefault() })
                    .ToListAsync(ct);
            }
            else
            {
                items = await dbQuery.Select(rd => new ResourceDelegateQueryResultItem { ResourceDelegate = rd }).ToListAsync(ct);
            }

            return new() { Items = items };
        }
    }
}
