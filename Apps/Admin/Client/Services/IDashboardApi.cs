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

namespace HealthGateway.Admin.Client.Services;

using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    Task<IDictionary<DateTime, int>> GetRegisteredUserCount(int timeOffset);

    /// <summary>
    /// Retrieves the count of logged in user in the last day.
    /// </summary>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>The count of logged in users in the current day.</returns>
    [Get("/LoggedInCount")]
    Task<IDictionary<DateTime, int>> GetLoggedinUsersCount(int timeOffset);

    /// <summary>
    /// Retrieves the count of dependents.
    /// </summary>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>The count of logged in users in the current day.</returns>
    [Get("/DependentCount")]
    Task<IDictionary<DateTime, int>> GetDependentCount(int timeOffset);

    /// <summary>
    /// Retrieves the count recurring users.
    /// </summary>
    /// <param name="days">The number of unique days for evaluating a user.</param>
    /// <param name="startPeriod">The period start over which to evaluate the user.</param>
    /// <param name="endPeriod">The period end over which to evaluate the user.</param>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>The count of recurrent users.</returns>
    [Get("/RecurringUsers")]
    Task<int> GetRecurringUsersCount(int days, string startPeriod, string endPeriod, int timeOffset);

    /// <summary>
    /// Retrieves the ratings summary.
    /// </summary>
    /// <param name="startPeriod">The period start to calculate the summary.</param>
    /// <param name="endPeriod">The period end to calculate the summary.</param>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    /// <returns>A dictionary pairing the ratings with the counts.</returns>
    [Get("/Ratings/Summary?startPeriod={startPeriod}&endPeriod={endPeriod}&timeOffset={timeOffset}")]
    Task<IDictionary<string, int>> GetRatingsSummary(string startPeriod, string endPeriod, int timeOffset);
}
