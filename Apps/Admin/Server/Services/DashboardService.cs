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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Delegates;

    /// <inheritdoc/>
    /// <param name="dependentDelegate">The dependent delegate to interact with the DB.</param>
    /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
    /// <param name="ratingDelegate">The rating delegate.</param>
    public class DashboardService(
        IResourceDelegateDelegate dependentDelegate,
        IUserProfileDelegate userProfileDelegate,
        IRatingDelegate ratingDelegate) : IDashboardService
    {
        /// <inheritdoc/>
        public async Task<AllTimeCounts> GetAllTimeCountsAsync(CancellationToken ct = default)
        {
            int userProfileCount = await userProfileDelegate.GetUserProfileCount(ct);
            int dependentCount = await dependentDelegate.GetDependentCountAsync(ct);
            int closedUserProfileCount = await userProfileDelegate.GetClosedUserProfileCount(ct);

            return new()
            {
                RegisteredUsers = userProfileCount,
                Dependents = dependentCount,
                ClosedAccounts = closedUserProfileCount,
            };
        }

        /// <inheritdoc/>
        public async Task<DailyUsageCounts> GetDailyUsageCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset, CancellationToken ct = default)
        {
            DateTimeOffset startDateTimeOffset = GetStartDateTimeOffset(startDateLocal, timeOffset);
            DateTimeOffset endDateTimeOffset = GetEndDateTimeOffset(endDateLocal, timeOffset);

            IDictionary<DateOnly, int> userRegistrationCounts = await userProfileDelegate.GetDailyUserRegistrationCountsAsync(startDateTimeOffset, endDateTimeOffset, ct);
            IDictionary<DateOnly, int> userLoginCounts = await userProfileDelegate.GetDailyUniqueLoginCountsAsync(startDateTimeOffset, endDateTimeOffset, ct);
            IDictionary<DateOnly, int> dependentRegistrationCounts = await dependentDelegate.GetDailyDependentRegistrationCountsAsync(startDateTimeOffset, endDateTimeOffset, ct);

            return new()
            {
                UserRegistrations = new SortedDictionary<DateOnly, int>(userRegistrationCounts),
                UserLogins = new SortedDictionary<DateOnly, int>(userLoginCounts),
                DependentRegistrations = new SortedDictionary<DateOnly, int>(dependentRegistrationCounts),
            };
        }

        /// <inheritdoc/>
        public async Task<int> GetRecurringUserCountAsync(int dayCount, DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset, CancellationToken ct = default)
        {
            DateTimeOffset startDate = GetStartDateTimeOffset(startDateLocal, timeOffset);
            DateTimeOffset endDate = GetEndDateTimeOffset(endDateLocal, timeOffset);
            return await userProfileDelegate.GetRecurringUserCountAsync(dayCount, startDate, endDate, ct);
        }

        /// <inheritdoc/>
        public async Task<AppLoginCounts> GetAppLoginCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset, CancellationToken ct = default)
        {
            DateTimeOffset startDate = GetStartDateTimeOffset(startDateLocal, timeOffset);
            DateTimeOffset endDate = GetEndDateTimeOffset(endDateLocal, timeOffset);

            IDictionary<UserLoginClientType, int> lastLoginClientCounts = await userProfileDelegate.GetLoginClientCountsAsync(startDate, endDate, ct);
            return new(
                lastLoginClientCounts.TryGetValue(UserLoginClientType.Web, out int webCount) ? webCount : 0,
                lastLoginClientCounts.TryGetValue(UserLoginClientType.Mobile, out int mobileCount) ? mobileCount : 0);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, int>> GetRatingsSummaryAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset, CancellationToken ct = default)
        {
            DateTimeOffset startDate = GetStartDateTimeOffset(startDateLocal, timeOffset);
            DateTimeOffset endDate = GetEndDateTimeOffset(endDateLocal, timeOffset);
            return await ratingDelegate.GetRatingsSummaryAsync(startDate, endDate, ct);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<int, int>> GetAgeCountsAsync(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset, CancellationToken ct = default)
        {
            DateTimeOffset startDate = GetStartDateTimeOffset(startDateLocal, timeOffset);
            DateTimeOffset endDate = GetEndDateTimeOffset(endDateLocal, timeOffset);
            IDictionary<int, int> yearOfBirthCounts = await userProfileDelegate.GetLoggedInUserYearOfBirthCountsAsync(startDate, endDate, ct);
            int currentYear = DateTime.Today.Year;
            return new SortedDictionary<int, int>(yearOfBirthCounts.ToDictionary(kvp => currentYear - kvp.Key, kvp => kvp.Value));
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
