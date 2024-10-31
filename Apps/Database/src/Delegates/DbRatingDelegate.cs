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
    using System.Globalization;
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

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbRatingDelegate(ILogger<DbRatingDelegate> logger, GatewayDbContext dbContext) : IRatingDelegate
    {
        /// <inheritdoc/>
        public async Task<DbResult<Rating>> InsertRatingAsync(Rating rating, CancellationToken ct = default)
        {
            logger.LogDebug("Adding rating to DB");
            dbContext.Add(rating);

            DbResult<Rating> result = new();

            try
            {
                await dbContext.SaveChangesAsync(ct);
                result.Status = DbStatusCode.Created;
                result.Payload = rating;
            }
            catch (DbUpdateException e)
            {
                logger.LogError(e, "Error adding rating to DB");
                result.Status = DbStatusCode.Error;
                result.Message = e.Message;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<Rating>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving ratings from DB, page #{PageNumber} with page size {PageSize}", page, pageSize);
            return await DbDelegateHelper.GetPagedDbResultAsync(dbContext.Rating.OrderBy(rating => rating.CreatedDateTime), page, pageSize, ct);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, int>> GetRatingsSummaryAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default)
        {
            logger.LogDebug(
                "Retrieving and summarizing ratings from DB created between {StartDate} and {EndDate}",
                DateFormatter.ToShortDateAndTime(startDateTimeOffset.UtcDateTime),
                DateFormatter.ToShortDateAndTime(endDateTimeOffset.UtcDateTime));

            return await dbContext.Rating
                .Where(r => r.CreatedDateTime >= startDateTimeOffset.UtcDateTime && r.CreatedDateTime <= endDateTimeOffset.UtcDateTime && !r.Skip)
                .GroupBy(x => x.RatingValue)
                .Select(r => new { Value = r.Key, Count = r.Count() })
                .ToDictionaryAsync(r => r.Value.ToString(CultureInfo.CurrentCulture), r => r.Count, ct);
        }
    }
}
