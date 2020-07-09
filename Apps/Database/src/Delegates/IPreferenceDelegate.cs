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
    /// Delegate that performs operations for the UserPreference model.
    /// </summary>
    public interface IUserPreferenceDelegate
    {
        /// <summary>
        /// Creates a UserPreference object in the database.
        /// </summary>
        /// <param name="preference">The preference to create.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<UserPreference> InsertUserPreference(UserPreference preference);

        /// <summary>
        /// Fetches the UserPreference from the database.
        /// </summary>
        /// <param name="hdid">The unique user profile key to find.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<UserPreference> GetUserPreference(string hdid);
    }
}
