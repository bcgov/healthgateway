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
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Npgsql;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbResourceDelegateDelegate(ILogger<DbResourceDelegateDelegate> logger, GatewayDbContext dbContext) : IResourceDelegateDelegate
    {
        /// <inheritdoc/>
        public async Task<DbResult<ResourceDelegate>> InsertAsync(ResourceDelegate resourceDelegate, bool commit, CancellationToken ct = default)
        {
            logger.LogDebug("Adding resource delegate to DB");

            DbResult<ResourceDelegate> result = new()
            {
                Payload = resourceDelegate,
                Status = DbStatusCode.Deferred,
            };

            dbContext.Add(resourceDelegate);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    result.Status = DbStatusCode.Error;

                    if (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                    {
                        logger.LogDebug("Resource delegate failed unique constraint");
                        result.Message = "You have already added this dependent.";
                    }
                    else
                    {
                        logger.LogError(e, "Error adding resource delegate to DB");
                        result.Message = e.Message;
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<ResourceDelegate>> GetAsync(string delegateId, int page = 0, int pageSize = 500, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving resource delegates from DB for delegate {DelegateId}, page #{PageNumber} with page size {PageSize}", delegateId, page, pageSize);

            return await DbDelegateHelper.GetPagedDbResultAsync(
                dbContext.ResourceDelegate
                    .Where(resourceDelegate => resourceDelegate.ProfileHdid == delegateId)
                    .OrderBy(resourceDelegate => resourceDelegate.CreatedDateTime),
                page,
                pageSize,
                ct);
        }

        /// <inheritdoc/>
        public async Task<IList<ResourceDelegate>> GetAsync(DateTime fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct = default)
        {
            toDate ??= DateTime.MaxValue;

            logger.LogDebug(
                "Retrieving resource delegates from DB created between {StartDate} and {EndDate}, page #{PageNumber} with page size {PageSize}",
                DateFormatter.ToShortDateAndTime(fromDate),
                DateFormatter.ToShortDateAndTime(toDate),
                page,
                pageSize);

            return await DbDelegateHelper.GetPagedDbResultAsync(
                dbContext.ResourceDelegate
                    .Where(resourceDelegate => resourceDelegate.CreatedDateTime >= fromDate && resourceDelegate.CreatedDateTime <= toDate)
                    .OrderBy(resourceDelegate => resourceDelegate.CreatedDateTime),
                page,
                pageSize,
                ct);
        }

        /// <inheritdoc/>
        public async Task<int> GetDependentCountAsync(CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving dependent count");
            return await dbContext.ResourceDelegate.Where(x => x.ReasonCode == ResourceDelegateReason.Guardian).CountAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<DateOnly, int>> GetDailyDependentRegistrationCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default)
        {
            logger.LogDebug(
                "Retrieving daily dependent registrations between {FromDate} and {ToDate}",
                DateFormatter.ToShortDateAndTime(startDateTimeOffset.UtcDateTime),
                DateFormatter.ToShortDateAndTime(endDateTimeOffset.UtcDateTime));

            return await dbContext.ResourceDelegate
                .Where(x => x.ReasonCode == ResourceDelegateReason.Guardian && x.CreatedDateTime >= startDateTimeOffset.UtcDateTime && x.CreatedDateTime <= endDateTimeOffset.UtcDateTime)
                .Select(x => new { x.ProfileHdid, x.ResourceOwnerHdid, createdDate = x.CreatedDateTime.AddMinutes(startDateTimeOffset.TotalOffsetMinutes).Date })
                .GroupBy(x => x.createdDate)
                .Select(x => new { createdDate = x.Key, count = x.Count() })
                .ToDictionaryAsync(x => DateOnly.FromDateTime(x.createdDate), x => x.count, ct);
        }

        /// <inheritdoc/>
        public async Task<DbResult<Dictionary<string, int>>> GetTotalDelegateCountsAsync(IEnumerable<string> dependentHdids, CancellationToken ct = default)
        {
            logger.LogDebug("Getting total delegate counts from DB for dependents {DependentHdids}", dependentHdids);

            string[] dependentArray = dependentHdids.ToArray();

            return new()
            {
                Payload = await dbContext.ResourceDelegate
                    .Where(d => Enumerable.Contains(dependentArray, d.ResourceOwnerHdid))
                    .GroupBy(d => d.ResourceOwnerHdid)
                    .ToDictionaryAsync(g => g.Key, g => g.Count(), ct),
                Status = DbStatusCode.Read,
            };
        }

        /// <inheritdoc/>
        public async Task<DbResult<ResourceDelegate>> DeleteAsync(ResourceDelegate resourceDelegate, bool commit, CancellationToken ct = default)
        {
            logger.LogDebug("Removing resource delegate from DB");

            DbResult<ResourceDelegate> result = new()
            {
                Status = DbStatusCode.Deferred,
            };

            dbContext.ResourceDelegate.Remove(resourceDelegate);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Deleted;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error removing resource delegate from DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string ownerId, string delegateId, CancellationToken ct = default)
        {
            logger.LogDebug("Checking if resource delegation exists for owner {OwnerId} and delegate {DelegateId}", ownerId, delegateId);
            return await dbContext.ResourceDelegate.AnyAsync(rd => rd.ResourceOwnerHdid == ownerId && rd.ProfileHdid == delegateId, ct);
        }

        /// <inheritdoc/>
        public async Task<ResourceDelegateQueryResult> SearchAsync(ResourceDelegateQuery query, CancellationToken ct = default)
        {
            logger.LogDebug("Querying resource delegates: {@Query}", query);

            IQueryable<ResourceDelegate> dbQuery = dbContext.ResourceDelegate;
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

            IList<ResourceDelegateQueryResultItem> items = query.IncludeDependent
                ? await dbQuery.GroupJoin(
                        dbContext.Dependent,
                        rd => rd.ResourceOwnerHdid,
                        d => d.HdId,
                        (rd, d) => new ResourceDelegateQueryResultItem
                        {
                            ResourceDelegate = rd,
                            Dependent = d.FirstOrDefault(),
                        })
                    .ToListAsync(ct)
                : await dbQuery.Select(rd => new ResourceDelegateQueryResultItem
                    {
                        ResourceDelegate = rd,
                    })
                    .ToListAsync(ct);

            return new() { Items = items };
        }
    }
}
