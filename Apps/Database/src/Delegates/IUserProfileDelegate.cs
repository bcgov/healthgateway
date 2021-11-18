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
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<UserProfile> InsertUserProfile(UserProfile profile);

        /// <summary>
        /// Updates the UserProfile object in the DB.
        /// Version must be set or a Concurrency exception will occur.
        /// UpdatedDateTime will overridden by our framework.
        /// </summary>
        /// <param name="profile">The profile to update.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<UserProfile> Update(UserProfile profile, bool commit = true);

        /// <summary>
        /// Fetches the UserProfile from the database.
        /// </summary>
        /// <param name="hdId">The unique profile key to find.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<UserProfile> GetUserProfile(string hdId);

        /// <summary>
        /// Returns the list of all UserProfiles who have an email address and have
        /// logged in before the lastLoggedIn date.
        /// </summary>
        /// <param name="filterDateTime">The profiles must have logged in prior to this date.</param>
        /// <param name="page">The page to request, defaults to 0.</param>
        /// <param name="pagesize">The amount of records to retrieve in 1 request, defaults to 500.</param>
        /// <returns>A list of matching UserProfiles wrapped in a DBResult.</returns>
        DBResult<List<UserProfile>> GetAllUserProfilesAfter(DateTime filterDateTime, int page = 0, int pagesize = 500);

        /// <summary>
        /// Returns the list of all UserProfiles who have a closed date earlier than the supplied filter datetime.
        /// </summary>
        /// <param name="filterDateTime">The profiles must be closed and earlier than this date.</param>
        /// <param name="page">The page to request, defaults to 0.</param>
        /// <param name="pagesize">The amount of records to retrieve in 1 request, defaults to 500.</param>
        /// <returns>A list of matching UserProfiles wrapped in a DBResult.</returns>
        DBResult<List<UserProfile>> GetClosedProfiles(DateTime filterDateTime, int page = 0, int pagesize = 500);

        /// <summary>
        /// Returns the daily count of registered users from the database.
        /// </summary>
        /// <param name="offset">The clients offset to get to UTC.</param>
        /// <returns>The count of user profiles that accepted the terms of service.</returns>
        IDictionary<DateTime, int> GetDailyRegisteredUsersCount(TimeSpan offset);

        /// <summary>
        /// Returns the daily count of logged in users with the given offset .
        /// </summary>
        /// <param name="offset">The clients offset to get to UTC.</param>
        /// <returns>The daily count of logged in users.</returns>
        IDictionary<DateTime, int> GetDailyLoggedInUsersCount(TimeSpan offset);

        /// <summary>
        /// Returns the list of all UserProfiles sorted by CreatedDateTime in assending order.
        /// </summary>
        /// <param name="page">The page to request.</param>
        /// <param name="pageSize">The amount of records to retrieve in 1 request.</param>
        /// <returns>A list of UserProfiles wrapped in a DBResult.</returns>
        DBResult<IEnumerable<UserProfile>> GetAll(int page, int pageSize);

        /// <summary>
        /// Retrieves the count recurring users.
        /// </summary>
        /// <param name="dayCount">The number of unique days for evaluating a user.</param>
        /// <param name="startDate">The start date to evaluate the user in UTC.</param>
        /// <param name="endDate">The end date to evaluate the user in UTC</param>
        /// <returns>The count of recurrent users.</returns>
        int GetRecurrentUserCount(int dayCount, DateTime startDate, DateTime endDate);
    }
}
