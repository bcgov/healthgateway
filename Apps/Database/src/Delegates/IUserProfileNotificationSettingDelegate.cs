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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Operations to be performed in the DB for the User Profile Notification Setting.
    /// </summary>
    public interface IUserProfileNotificationSettingDelegate
    {
        /// <summary>
        /// Fetches the user profile notification settings by hdid from the database.
        /// </summary>
        /// <param name="hdid">The hdid to search by.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>
        /// A read-only list of user profile notification settings for the specified hdid.
        /// Returns an empty collection if none are found.
        /// </returns>
        Task<IReadOnlyList<UserProfileNotificationSetting>> GetAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Adds or updates the user profile notification object to the DB.
        /// </summary>
        /// <param name="notificationSetting">The user profile notification setting object to add or update.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateAsync(UserProfileNotificationSetting notificationSetting, bool commit = true, CancellationToken ct = default);
    }
}
