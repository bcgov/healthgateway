﻿// -------------------------------------------------------------------------
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
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed in the DB for the Profile.
    /// </summary>
    public interface IProfileDelegate
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
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<UserProfile> UpdateUserProfile(UserProfile profile);

        /// <summary>
        /// Fetches the UserProfile from the database.
        /// </summary>
        /// <param name="hdId">The unique profile key to find.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<UserProfile> GetUserProfile(string hdId);
    }
}
