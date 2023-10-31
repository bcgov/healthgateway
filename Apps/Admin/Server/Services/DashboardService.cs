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
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class DashboardService : IDashboardService
    {
        private readonly IResourceDelegateDelegate dependentDelegate;
        private readonly ILogger logger;
        private readonly IRatingDelegate ratingDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dependentDelegate">The dependent delegate to interact with the DB.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="ratingDelegate">The rating delegate.</param>
        public DashboardService(
            ILogger<DashboardService> logger,
            IResourceDelegateDelegate dependentDelegate,
            IUserProfileDelegate userProfileDelegate,
            IRatingDelegate ratingDelegate)
        {
            this.logger = logger;
            this.dependentDelegate = dependentDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.ratingDelegate = ratingDelegate;
        }

        /// <inheritdoc/>
        public IDictionary<DateTime, int> GetDailyRegisteredUsersCount(int timeOffset)
        {
            TimeSpan ts = new(0, timeOffset, 0);
            return this.userProfileDelegate.GetDailyRegisteredUsersCount(ts);
        }

        /// <inheritdoc/>
        public IDictionary<DateTime, int> GetDailyLoggedInUsersCount(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset)
        {
            TimeSpan offsetSpan = TimeSpan.FromMinutes(timeOffset);
            DateTimeOffset startDateTimeOffset = new(startDateLocal.ToDateTime(TimeOnly.MinValue), offsetSpan);
            DateTimeOffset endDateTimeOffset = new(endDateLocal.ToDateTime(TimeOnly.MaxValue), offsetSpan);
            return this.userProfileDelegate.GetLoggedInUsersCount(startDateTimeOffset, endDateTimeOffset);
        }

        /// <inheritdoc/>
        public IDictionary<DateTime, int> GetDailyDependentCount(int timeOffset)
        {
            TimeSpan ts = new(0, timeOffset, 0);
            return this.dependentDelegate.GetDailyDependentCount(ts);
        }

        /// <inheritdoc/>
        public IDictionary<string, int> GetRecurrentUserCounts(int dayCount, string startPeriod, string endPeriod, int timeOffset)
        {
            DateTime startDate = GetStartDateTime(startPeriod, timeOffset);
            DateTime endDate = GetEndDateTime(endPeriod, timeOffset);

            this.logger.LogDebug(
                "Start Period (Local): {StartPeriod} - End Period (Local): {EndPeriod} - StartDate (UTC): {StartDate} - End Date (UTC): {EndDate} - TimeOffset (UI): {TimeOffset}",
                startPeriod,
                endPeriod,
                startDate,
                endDate,
                timeOffset.ToString(CultureInfo.InvariantCulture));

            IDictionary<string, int> lastLoginCounts = this.userProfileDelegate.GetLastLoginClientCounts(startDate, endDate);
            int recurringUserCount = this.userProfileDelegate.GetRecurrentUserCount(dayCount, startDate, endDate);

            IDictionary<string, int> recurringUserCounts = new Dictionary<string, int>
            {
                { UserLoginClientType.Mobile.ToString(), lastLoginCounts.TryGetValue(UserLoginClientType.Mobile.ToString(), out int mobileCount) ? mobileCount : 0 },
                { UserLoginClientType.Web.ToString(), lastLoginCounts.TryGetValue(UserLoginClientType.Web.ToString(), out int webCount) ? webCount : 0 },
                { "RecurringUserCount", recurringUserCount },
            };

            return recurringUserCounts;
        }

        /// <inheritdoc/>
        public IDictionary<string, int> GetRatingSummary(string startPeriod, string endPeriod, int timeOffset)
        {
            DateTime startDate = GetStartDateTime(startPeriod, timeOffset);
            DateTime endDate = GetEndDateTime(endPeriod, timeOffset);
            return this.ratingDelegate.GetSummary(startDate, endDate);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, int>> GetYearOfBirthCountsAsync(string startPeriod, string endPeriod, int timeOffset, CancellationToken ct)
        {
            DateTime startDate = GetStartDateTime(startPeriod, timeOffset);
            DateTime endDate = GetEndDateTime(endPeriod, timeOffset);
            return await this.userProfileDelegate.GetLoggedInUserYearOfBirthCountsAsync(startDate, endDate, ct);
        }

        private static DateTime GetStartDateTime(string startPeriod, int timeOffset)
        {
            TimeSpan ts = new(0, GetOffset(timeOffset), 0);
            DateTime startDate = DateTime.Parse(startPeriod, CultureInfo.InvariantCulture).Date.Add(ts);
            return DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        }

        private static DateTime GetEndDateTime(string endPeriod, int timeOffset)
        {
            TimeSpan ts = new(0, GetOffset(timeOffset), 0);
            DateTime endDate = DateTime.Parse(endPeriod, CultureInfo.InvariantCulture).Date.Add(ts).AddDays(1).AddMilliseconds(-1);
            return DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
        }

        /// <summary>
        /// Returns an offset value that can be used to create a date in UTC.
        /// </summary>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        /// <returns>The offset value used to create UTC.</returns>
        private static int GetOffset(int timeOffset)
        {
            // If timeOffset is a negative value, then it means current timezone is [n] minutes behind UTC so we need to change this value to a positive when creating TimeSpan for DateTime object in UTC.
            // If timeOffset is a positive value, then it means current timezone is [n] minutes ahead of UTC so we need to change this value to a negative when creating TimeSpan for DateTime object in UTC.
            // If timeOffset is 0, then it means current timezone is UTC.
            return timeOffset * -1;
        }
    }
}
