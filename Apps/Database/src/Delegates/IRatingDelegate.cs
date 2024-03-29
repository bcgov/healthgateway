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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Delegate that performs operations for the Rating model.
    /// </summary>
    public interface IRatingDelegate
    {
        /// <summary>
        /// Creates a Rating object in the database.
        /// </summary>
        /// <param name="rating">The rating to create.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        Task<DbResult<Rating>> InsertRatingAsync(Rating rating, CancellationToken ct = default);

        /// <summary>
        /// Gets a paged list of Ratings from the database.
        /// </summary>
        /// <param name="page">The starting offset for the query.</param>
        /// <param name="pageSize">The maximum amount of rows to return.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of Ratings.</returns>
        Task<IList<Rating>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);

        /// <summary>
        /// Gets the ratings summary.
        /// </summary>
        /// <param name="startDateTimeOffset">The start datetime offset to query.</param>
        /// <param name="endDateTimeOffset">The end datetime offset to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A dictionary pairing the ratings with the counts.</returns>
        Task<IDictionary<string, int>> GetRatingsSummaryAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default);
    }
}
