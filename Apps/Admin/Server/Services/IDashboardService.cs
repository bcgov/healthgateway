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
namespace HealthGateway.Admin.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// Service that provides functionality for the Admin dashboard.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves all-time counts.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A model containing the all-time counts.</returns>
        Task<AllTimeCounts> GetAllTimeCountsAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves daily usage counts over a date range.
        /// </summary>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A model containing daily usage counts.</returns>
        Task<DailyUsageCounts> GetDailyUsageCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a count of recurring users over a date range.
        /// </summary>
        /// <param name="dayCount">Minimum number of days users must have logged in within the period to count as recurring.</param>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A count of recurring users.</returns>
        Task<int> GetRecurringUserCountAsync(int dayCount, DateOnly startDateLocal, DateOnly endDateLocal, CancellationToken ct = default);

        /// <summary>
        /// Retrieves unique app login counts over a date range.
        /// </summary>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The login counts for Health Gateway applications.</returns>
        Task<AppLoginCounts> GetAppLoginCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, CancellationToken ct = default);

        /// <summary>
        /// Retrieves app ratings over a date range.
        /// </summary>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A dictionary pairing the ratings with the counts.</returns>
        Task<IDictionary<string, int>> GetRatingsSummaryAsync(DateOnly startDateLocal, DateOnly endDateLocal, CancellationToken ct = default);

        /// <summary>
        /// Retrieves age counts for users that have logged in between two dates.
        /// </summary>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A dictionary mapping ages to user counts.</returns>
        Task<IDictionary<int, int>> GetAgeCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, CancellationToken ct = default);
    }
}
