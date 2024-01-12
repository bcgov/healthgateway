//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed for AdminTag.
    /// </summary>
    public interface IAdminTagDelegate
    {
        /// <summary>
        /// Add the given admin tag.
        /// </summary>
        /// <param name="tag">The admin tag to be added to the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An admin tag wrapped in a DBResult.</returns>
        Task<DbResult<AdminTag>> AddAsync(AdminTag tag, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Delete the given admin tag.
        /// </summary>
        /// <param name="tag">The admin tag to be deleted in the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An admin tag wrapped in a DBResult.</returns>
        Task<DbResult<AdminTag>> DeleteAsync(AdminTag tag, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of admin tags ordered by the name ascending.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An IEnumerable of AdminTag wrapped in a DBResult.</returns>
        Task<IEnumerable<AdminTag>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets a list of admin tags by ids.
        /// </summary>
        /// <param name="adminTagIds">The admin tag ids to search on.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An IEnumerable of AdminTag wrapped in a DBResult.</returns>
        Task<DbResult<IEnumerable<AdminTag>>> GetAdminTagsAsync(ICollection<Guid> adminTagIds, CancellationToken ct = default);
    }
}
