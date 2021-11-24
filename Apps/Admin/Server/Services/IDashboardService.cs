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
    using HealthGateway.Admin.Server.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Service that provides functionality to the admin dashboard.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves the daily count of registered users.
        /// </summary>
        /// <param name="timeOffset">The time offset from the client browser to UTC.</param>
        /// <returns>The count of user profiles that accepted the terms of service.</returns>
        IDictionary<DateTime, int> GetDailyRegisteredUsersCount(int timeOffset);

        /// <summary>
        /// Retrieves the daily count of logged in users in the current day.
        /// </summary>
        /// <param name="timeOffset">The time offset from the client browser to UTC.</param>
        /// <returns>The count of logged in user.</returns>
        IDictionary<DateTime, int> GetDailyLoggedInUsersCount(int timeOffset);

        /// <summary>
        /// Retrieves the count of dependents.
        /// </summary>
        /// <param name="timeOffset">The time offset from the client browser to UTC.</param>
        /// <returns>The count of dependents.</returns>
        IDictionary<DateTime, int> GetDailyDependentCount(int timeOffset);

        /// <summary>
        /// Retrieves the count recurring users.
        /// </summary>
        /// <param name="dayCount">The number of unique days for evaluating a user.</param>
        /// <param name="startPeriod">The period start over which to evaluate the user.</param>
        /// <param name="endPeriod">The period end over which to evaluate the user.</param>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        /// <returns>The count of recurrent users.</returns>
        int GetRecurrentUserCount(int dayCount, string startPeriod, string endPeriod, int timeOffset);

        /// <summary>
        /// Retrieves a list of message verifications matching the query.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <returns>A list of users matching the query.</returns>
        RequestResult<IEnumerable<MessagingVerification>> GetMessageVerifications(UserQueryType queryType, string queryString);

        /// <summary>
        /// Retrieves the ratings summary.
        /// </summary>
        /// <param name="startPeriod">The period start to calculate the summary.</param>
        /// <param name="endPeriod">The period end to calculate the summary.</param>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        /// <returns>A dictionary pairing the ratings with the counts.</returns>
        IDictionary<string, int> GetRatingSummary(string startPeriod, string endPeriod, int timeOffset);
    }
}
