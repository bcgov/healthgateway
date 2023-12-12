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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DbRatingDelegate : IRatingDelegate
    {
        private readonly ILogger<DbRatingDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbRatingDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbRatingDelegate(
            ILogger<DbRatingDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public DbResult<Rating> InsertRating(Rating rating)
        {
            this.logger.LogTrace("Inserting rating to DB");
            DbResult<Rating> result = new();
            this.dbContext.Add(rating);
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

            this.logger.LogDebug("Finished inserting rating {Rating} to DB..", rating.Id);
            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<Rating>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            this.logger.LogTrace("Retrieving all the ratings for the page #{Page} with pageSize: {PageSize}...", page, pageSize);
            return await DbDelegateHelper.GetPagedDbResultAsync(this.dbContext.Rating.OrderBy(rating => rating.CreatedDateTime), page, pageSize, ct);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, int>> GetRatingsSummaryAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default)
        {
            this.logger.LogTrace("Retrieving the ratings summary between {StartDate} and {EndDate}...", startDateTimeOffset, endDateTimeOffset);
            return await this.dbContext.Rating
                .Where(r => r.CreatedDateTime >= startDateTimeOffset.UtcDateTime && r.CreatedDateTime <= endDateTimeOffset.UtcDateTime && !r.Skip)
                .GroupBy(x => x.RatingValue)
                .Select(r => new { Value = r.Key, Count = r.Count() })
                .ToDictionaryAsync(r => r.Value.ToString(CultureInfo.CurrentCulture), r => r.Count, ct);
        }
    }
}
