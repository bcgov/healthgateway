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
namespace HealthGateway.Database.Delegates;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HealthGateway.Database.Models;
using HealthGateway.Database.Wrapper;

/// <summary>
/// Operations to be performed in the DB for the Admin Profile.
/// </summary>
public interface IAdminUserProfileDelegate
{
    /// <summary>
    /// Fetches the AdminUserProfile from the database.
    /// </summary>
    /// <param name="username">The unique username key to find.</param>
    /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
    /// <returns>A DB result which encapsulates the return object and status.</returns>
    Task<DbResult<AdminUserProfile>> GetAdminUserProfileAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Returns Active AdminUserProfile objects from the database.
    /// </summary>
    /// <param name="activeDays">Users active within the last X days".</param>
    /// <param name="timeOffset">The clients offset to get to UTC.</param>
    /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
    /// <returns>An IEnumerable of AdminUserProfile objects.</returns>
    Task<IList<AdminUserProfile>> GetActiveAdminUserProfilesAsync(int activeDays, TimeSpan timeOffset, CancellationToken ct = default);

    /// <summary>
    /// Returns Inactive AdminUserProfile objects from the database.
    /// </summary>
    /// <param name="inactiveDays">Users inactive for at least X days.</param>
    /// <param name="timeOffset">The clients offset to get to UTC.</param>
    /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
    /// <returns>An IEnumerable of AdminUserProfile objects.</returns>
    Task<IList<AdminUserProfile>> GetInactiveAdminUserProfilesAsync(int inactiveDays, TimeSpan timeOffset, CancellationToken ct = default);

    /// <summary>
    /// Creates an AdminUserProfile object in the database.
    /// </summary>
    /// <param name="profile">The profile to create.</param>
    /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
    /// <returns>A DB result which encapsulates the return object and status.</returns>
    Task<DbResult<AdminUserProfile>> AddAsync(AdminUserProfile profile, CancellationToken ct = default);

    /// <summary>
    /// Updates the AdminUserProfile object in the DB.
    /// Version must be set or a Concurrency exception will occur.
    /// UpdatedDateTime will overridden by our framework.
    /// </summary>
    /// <param name="profile">The profile to update.</param>
    /// <param name="commit">if true the transaction is persisted immediately.</param>
    /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
    /// <returns>A DB result which encapsulates the return object and status.</returns>
    Task<DbResult<AdminUserProfile>> UpdateAsync(AdminUserProfile profile, bool commit = true, CancellationToken ct = default);
}
