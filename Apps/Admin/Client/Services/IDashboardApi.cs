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
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// API to fetch the Dashboard from the server.
/// </summary>
public interface IDashboardApi
{
    /// <summary>
    /// Retrieves the count of registered users.
    /// </summary>
    /// <returns>HttpResponseMessage.</returns>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    [Get("/RegisteredCount")]
    Task<HttpResponseMessage> GetRegisteredUserCount(int timeOffset);

    /// <summary>
    /// Retrieves the count of logged in user in the last day.
    /// </summary>
    /// <returns>HttpResponseMessage.</returns>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    [Get("/LoggedInCount")]
    Task<HttpResponseMessage> GetLoggedinUsersCount(int timeOffset);

    /// <summary>
    /// Retrieves the count of dependents.
    /// </summary>
    /// <returns>HttpResponseMessage.</returns>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    [Get("/DependentCount")]
    Task<HttpResponseMessage> GetDependentCount(int timeOffset);

    /// <summary>
    /// Retrieves the count recurring users.
    /// </summary>
    /// <returns>HttpResponseMessage.</returns>
    /// <param name="days">The number of unique days for evaluating a user.</param>
    /// <param name="startPeriod">The period start over which to evaluate the user.</param>
    /// <param name="endPeriod">The period end over which to evaluate the user.</param>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    [Get("/RecurringUsers")]
    Task<HttpResponseMessage> GetRecurringUsersCount(int days, string startPeriod, string endPeriod, int timeOffset);

    /// <summary>
    /// Retrieves the ratings summary.
    /// </summary>
    /// <returns>HttpResponseMessage.</returns>
    /// <param name="startPeriod">The period start to calculate the summary.</param>
    /// <param name="endPeriod">The period end to calculate the summary.</param>
    /// <param name="timeOffset">The offset from the client browser to UTC.</param>
    [Get("Ratings/Summary?startPeriod={startPeriod}&endPeriod={endPeriod}&timeOffset={timeOffset}")]
    Task<HttpResponseMessage> GetRatingsSummary(string startPeriod, string endPeriod, int timeOffset);
}
