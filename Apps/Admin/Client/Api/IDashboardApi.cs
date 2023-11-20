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

namespace HealthGateway.Admin.Client.Api;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthGateway.Admin.Common.Models;
using Refit;

/// <summary>
/// API to fetch dashboard data.
/// </summary>
public interface IDashboardApi
{
    /// <summary>
    /// Retrieves the daily counts of user registrations.
    /// </summary>
    /// <param name="timeOffset">The local timezone offset from UTC in minutes.</param>
    /// <returns>The number of user registrations by date.</returns>
    [Get("/RegisteredCount")]
    Task<IDictionary<DateOnly, int>> GetDailyUserRegistrationCountsAsync(int timeOffset);

    /// <summary>
    /// Retrieves the daily counts of dependent registrations.
    /// </summary>
    /// <param name="timeOffset">The local timezone offset from UTC in minutes.</param>
    /// <returns>The number of dependent registrations by date.</returns>
    [Get("/DependentCount")]
    Task<IDictionary<DateOnly, int>> GetDailyDependentRegistrationCountsAsync(int timeOffset);

    /// <summary>
    /// Retrieves the daily counts of unique user logins over a date range.
    /// </summary>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The local timezone offset from UTC in minutes.</param>
    /// <returns>The number of unique user logins by date.</returns>
    [Get("/LoggedInCount")]
    Task<IDictionary<DateOnly, int>> GetDailyUniqueLoginCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);

    /// <summary>
    /// Retrieves a count of recurring users over a date range.
    /// </summary>
    /// <param name="days">Minimum number of days users must have logged in within the period to count as recurring.</param>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The local timezone offset from UTC in minutes.</param>
    /// <returns>A count of recurring users.</returns>
    [Get("/RecurringUserCount")]
    Task<int> GetRecurringUserCountAsync(int days, DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);

    /// <summary>
    /// Retrieves unique app login counts over a date range.
    /// </summary>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The local timezone offset from UTC in minutes.</param>
    /// <returns>The login counts for Health Gateway applications.</returns>
    [Get("/AppLoginCounts")]
    Task<AppLoginCounts> GetAppLoginCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);

    /// <summary>
    /// Retrieves the ratings summary.
    /// </summary>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The local timezone offset from UTC in minutes.</param>
    /// <returns>A dictionary pairing the ratings with the counts.</returns>
    [Get("/Ratings/Summary")]
    Task<IDictionary<string, int>> GetRatingsSummaryAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);

    /// <summary>
    /// Retrieves year of birth counts for users that have logged in between two dates.
    /// </summary>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The local timezone offset from UTC in minutes.</param>
    /// <returns>A dictionary mapping birth years to user counts.</returns>
    [Get("/YearOfBirthCounts")]
    Task<IDictionary<string, int>> GetYearOfBirthCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);
}
