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
            DateTimeOffset startDateTimeOffset = GetStartDateTimeOffset(startDateLocal, timeOffset);
            DateTimeOffset endDateTimeOffset = GetEndDateTimeOffset(endDateLocal, timeOffset);
            return this.userProfileDelegate.GetLoggedInUsersCount(startDateTimeOffset, endDateTimeOffset);
        }

        /// <inheritdoc/>
        public IDictionary<DateTime, int> GetDailyDependentCount(int timeOffset)
        {
            TimeSpan ts = new(0, timeOffset, 0);
            return this.dependentDelegate.GetDailyDependentCount(ts);
        }

        /// <inheritdoc/>
        public IDictionary<string, int> GetRecurrentUserCounts(int dayCount, DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset)
        {
            DateTimeOffset startDate = GetStartDateTimeOffset(startDateLocal, timeOffset);
            DateTimeOffset endDate = GetEndDateTimeOffset(endDateLocal, timeOffset);

            this.logger.LogDebug(
                "Start Date (Local): {StartDate} - End Date (Local): {EndDate} - StartDate (UTC): {StartDateUtc} - End Date (UTC): {EndDateUtc} - TimeOffset (UI): {TimeOffset}",
                startDate.DateTime,
                endDate.DateTime,
                startDate.UtcDateTime,
                endDate.UtcDateTime,
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
        public IDictionary<string, int> GetRatingSummary(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset)
        {
            DateTimeOffset startDate = GetStartDateTimeOffset(startDateLocal, timeOffset);
            DateTimeOffset endDate = GetEndDateTimeOffset(endDateLocal, timeOffset);
            return this.ratingDelegate.GetSummary(startDate, endDate);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, int>> GetYearOfBirthCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset, CancellationToken ct)
        {
            DateTimeOffset startDate = GetStartDateTimeOffset(startDateLocal, timeOffset);
            DateTimeOffset endDate = GetEndDateTimeOffset(endDateLocal, timeOffset);
            return await this.userProfileDelegate.GetLoggedInUserYearOfBirthCountsAsync(startDate, endDate, ct);
        }

        private static DateTimeOffset GetStartDateTimeOffset(DateOnly startDateLocal, int timeOffset)
        {
            TimeSpan offsetSpan = TimeSpan.FromMinutes(timeOffset);
            DateTimeOffset startDateTimeOffset = new(startDateLocal.ToDateTime(TimeOnly.MinValue), offsetSpan);
            return startDateTimeOffset;
        }

        private static DateTimeOffset GetEndDateTimeOffset(DateOnly endDateLocal, int timeOffset)
        {
            TimeSpan offsetSpan = TimeSpan.FromMinutes(timeOffset);
            DateTimeOffset endDateTimeOffset = new(endDateLocal.ToDateTime(TimeOnly.MaxValue), offsetSpan);
            return endDateTimeOffset;
        }
    }
}
