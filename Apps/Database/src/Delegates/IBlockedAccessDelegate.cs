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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Delegate that performs operations relating to the BlackedAccess model.
    /// </summary>
    public interface IBlockedAccessDelegate
    {
        /// <summary>
        /// Deletes the blocked access object including agent audit to the DB.
        /// </summary>
        /// <param name="blockedAccess">The blocked access object to delete.</param>
        /// <param name="agentAudit">The agent audit to create.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteBlockedAccessAsync(BlockedAccess blockedAccess, AgentAudit agentAudit);

        /// <summary>
        /// Fetches the blocked access by hdid from the database.
        /// </summary>
        /// <param name="hdid">The hdid to search by.</param>
        /// <returns>The blocked access or null if not found.</returns>
        Task<BlockedAccess?> GetBlockedAccessAsync(string hdid);

        /// <summary>
        /// Fetches the blocked access's data sources from the database.
        /// </summary>
        /// <param name="hdid">The hdid to search by.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<DataSource>> GetDataSourcesAsync(string hdid);

        /// <summary>
        /// Adds or updates the blocked access object including agent audit to the DB.
        /// </summary>
        /// <param name="blockedAccess">The blocked access object to add or update.</param>
        /// <param name="agentAudit">The agent audit to create.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateBlockedAccessAsync(BlockedAccess blockedAccess, AgentAudit agentAudit);

        /// <summary>
        /// Retrieves all blocked access records.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A collection of records of user HDIDs with the data sources currently blocked.</returns>
        Task<IList<BlockedAccess>> GetAllAsync(CancellationToken ct);
    }
}
