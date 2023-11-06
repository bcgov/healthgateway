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
using Refit;

/// <summary>
/// API to fetch the Dashboard from the server.
/// </summary>
public interface IDashboardApi
{
    /// <summary>
    /// Retrieves the count of registered users.
    /// </summary>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>The count of registered users.</returns>
    [Get("/RegisteredCount")]
    Task<IDictionary<DateTime, int>> GetRegisteredUserCountAsync(int timeOffset);

    /// <summary>
    /// Retrieves the count of logged in user in the last day.
    /// </summary>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>The count of logged in users in the current day.</returns>
    [Get("/LoggedInCount")]
    Task<IDictionary<DateTime, int>> GetLoggedinUsersCountAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);

    /// <summary>
    /// Retrieves the count of dependents.
    /// </summary>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>The count of logged in users in the current day.</returns>
    [Get("/DependentCount")]
    Task<IDictionary<DateTime, int>> GetDependentCountAsync(int timeOffset);

    /// <summary>
    /// Retrieves user counts.
    /// </summary>
    /// <param name="days">The number of unique days for evaluating a recurring user.</param>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>The counts of recurring users, mobile logins, and web logins.</returns>
    [Get("/RecurringUserCounts")]
    Task<IDictionary<string, int>> GetUserCountsAsync(int days, DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);

    /// <summary>
    /// Retrieves the ratings summary.
    /// </summary>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>A dictionary pairing the ratings with the counts.</returns>
    [Get("/Ratings/Summary")]
    Task<IDictionary<string, int>> GetRatingsSummaryAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);

    /// <summary>
    /// Retrieves year of birth counts for users that have logged in between two dates.
    /// </summary>
    /// <param name="startDateLocal">The local start date to query.</param>
    /// <param name="endDateLocal">The local end date to query.</param>
    /// <param name="timeOffset">The current timezone offset from the client browser to UTC.</param>
    /// <returns>A dictionary mapping birth years to user counts.</returns>
    [Get("/YearOfBirthCounts")]
    Task<IDictionary<string, int>> GetYearOfBirthCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset);
}
