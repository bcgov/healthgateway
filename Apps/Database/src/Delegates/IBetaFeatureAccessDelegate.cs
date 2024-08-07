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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Delegate that performs operations relating to the BetaFeatureAccess model.
    /// </summary>
    public interface IBetaFeatureAccessDelegate
    {
        /// <summary>
        /// Adds the list of beta feature associations to the DB.
        /// </summary>
        /// <param name="betaFeatureAccessAssociations">The list of beta feature access associations to add.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task AddRangeAsync(IEnumerable<BetaFeatureAccess> betaFeatureAccessAssociations, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Deletes the list of beta feature associations from the DB.
        /// </summary>
        /// <param name="betaFeatureAccessAssociations">The list of beta feature access associations to delete.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteRangeAsync(IEnumerable<BetaFeatureAccess> betaFeatureAccessAssociations, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of beta feature associations for the provided hdids from the DB.
        /// </summary>
        /// <param name="hdids">A list of hdids to query on.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IList<BetaFeatureAccess>> GetAsync(IEnumerable<string> hdids, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of all beta feature associations from the DB, using pagination.
        /// </summary>
        /// <param name="pageIndex">Current page index, starting from 0.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<PaginatedResult<IGrouping<string, BetaFeatureAccess>>> GetAllAsync(int pageIndex, int pageSize, CancellationToken ct = default);
    }
}
