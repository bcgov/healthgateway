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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed in the DB for the Profile.
    /// </summary>
    public interface IUserProfileDelegate
    {
        /// <summary>
        /// Creates a UserProfile object in the database.
        /// </summary>
        /// <param name="profile">The profile to create.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        Task<DbResult<UserProfile>> InsertUserProfileAsync(UserProfile profile, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Updates select attributes of the UserProfile object in the DB.
        /// This method re-reads the UserProfile to ensure all other attributes are not overwritten.
        /// Version must be set or a Concurrency exception will occur.
        /// UpdatedDateTime will overridden by our framework.
        /// </summary>
        /// <param name="profile">The profile to update.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        Task<DbResult<UserProfile>> UpdateAsync(UserProfile profile, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Fetches the UserProfile from the database.
        /// </summary>
        /// <param name="hdId">The unique profile key to find.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DbResult<UserProfile> GetUserProfile(string hdId);

        /// <summary>
        /// Fetches a UserProfile from the database by HDID.
        /// </summary>
        /// <param name="hdid">The unique profile key to find.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The matching UserProfile, or null if not found.</returns>
        Task<UserProfile?> GetUserProfileAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Fetches UserProfiles from the database.
        /// </summary>
        /// <param name="hdIds">The unique profile keys to find.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        Task<IList<UserProfile>> GetUserProfilesAsync(IList<string> hdIds, CancellationToken ct = default);

        /// <summary>
        /// Fetches UserProfile from the database.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        Task<IList<UserProfile>> GetUserProfilesAsync(UserQueryType queryType, string queryString);

        /// <summary>
        /// Returns the list of all UserProfiles who have an email address and have
        /// logged in before the lastLoggedIn date.
        /// </summary>
        /// <param name="filterDateTime">The profiles must have logged in prior to this date.</param>
        /// <param name="page">The page to request, defaults to 0.</param>
        /// <param name="pageSize">The amount of records to retrieve in 1 request, defaults to 500.</param>
        /// <returns>A list of matching UserProfiles wrapped in a DBResult.</returns>
        DbResult<List<UserProfile>> GetAllUserProfilesAfter(DateTime filterDateTime, int page = 0, int pageSize = 500);

        /// <summary>
        /// Returns the list of all UserProfiles who have a closed date earlier than the supplied filter datetime.
        /// </summary>
        /// <param name="filterDateTime">The profiles must be closed and earlier than this date.</param>
        /// <param name="page">The page to request, defaults to 0.</param>
        /// <param name="pageSize">The amount of records to retrieve in 1 request, defaults to 500.</param>
        /// <returns>A list of matching UserProfiles wrapped in a DBResult.</returns>
        DbResult<List<UserProfile>> GetClosedProfiles(DateTime filterDateTime, int page = 0, int pageSize = 500);

        /// <summary>
        /// Retrieves daily counts of user registrations over a date range.
        /// </summary>
        /// <param name="startDateTimeOffset">The start datetime offset to query.</param>
        /// <param name="endDateTimeOffset">The end datetime offset to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The daily counts of user registrations by date.</returns>
        Task<IDictionary<DateOnly, int>> GetDailyUserRegistrationCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default);

        /// <summary>
        /// Retrieves daily counts of unique user logins over a date range.
        /// </summary>
        /// <param name="startDateTimeOffset">The start datetime offset to query.</param>
        /// <param name="endDateTimeOffset">The end datetime offset to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The daily counts of unique user logins by date.</returns>
        Task<IDictionary<DateOnly, int>> GetDailyUniqueLoginCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default);

        /// <summary>
        /// Returns the list of all UserProfiles sorted by CreatedDateTime in ascending order.
        /// </summary>
        /// <param name="page">The page to request.</param>
        /// <param name="pageSize">The amount of records to retrieve in 1 request.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of UserProfiles.</returns>
        Task<IList<UserProfile>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a count of recurring users over a date range.
        /// </summary>
        /// <param name="dayCount">Minimum number of days users must have logged in within the period to count as recurring.</param>
        /// <param name="startDateTimeOffset">The start datetime offset to query.</param>
        /// <param name="endDateTimeOffset">The end datetime offset to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A count of recurring users.</returns>
        Task<int> GetRecurringUserCountAsync(int dayCount, DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default);

        /// <summary>
        /// Retrieves unique login counts over a date range grouped by client type.
        /// </summary>
        /// <param name="startDateTimeOffset">The start datetime offset to query.</param>
        /// <param name="endDateTimeOffset">The end datetime offset to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A dictionary containing login counts grouped by client type.</returns>
        Task<IDictionary<UserLoginClientType, int>> GetLoginClientCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default);

        /// <summary>
        /// Returns the list of UserProfileHistory for a particular hdid and x number of records to return.
        /// </summary>
        /// <param name="hdid">The unique profile key to find.</param>
        /// <param name="limit">The number of rows to return.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of matching UserProfileHistory wrapped in a DBResult.</returns>
        Task<IList<UserProfileHistory>> GetUserProfileHistoryListAsync(string hdid, int limit, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a count of user profiles.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The number of user profiles.</returns>
        Task<int> GetUserProfileCount(CancellationToken ct = default);

        /// <summary>
        /// Retrieves the number of user profiles that have been closed and not re-opened.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The number of user profiles that have been closed and not re-opened</returns>
        Task<int> GetClosedUserProfileCount(CancellationToken ct = default);

        /// <summary>
        /// Returns the list of logged in user year of birth counts over a date range.
        /// </summary>
        /// <param name="startDateTimeOffset">The start datetime offset to query.</param>
        /// <param name="endDateTimeOffset">The end datetime offset to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The counts of logged in users by year of birth.</returns>
        Task<IDictionary<int, int>> GetLoggedInUserYearOfBirthCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default);
    }
}
