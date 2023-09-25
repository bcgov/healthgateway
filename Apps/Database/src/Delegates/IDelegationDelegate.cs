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
    /// Delegate that performs operations for models relating to Dependent.
    /// </summary>
    public interface IDelegationDelegate
    {
        /// <summary>
        /// Fetches the Dependent by hdid from the database.
        /// </summary>
        /// <param name="hdid">The dependent hdid to query on.</param>
        /// <param name="includeAllowedDelegation">
        /// Indicates whether allowed delegation should be included in the returned
        /// dependent.
        /// </param>
        /// <returns>The dependent or null if not found.</returns>
        Task<Dependent?> GetDependentAsync(string hdid, bool includeAllowedDelegation = false);

        /// <summary>
        /// Adds or updates the dependent object including allowed delegation associations as well as resource delegates and agent
        /// audit to the DB.
        /// </summary>
        /// <param name="dependent">The dependent to update.</param>
        /// <param name="resourceDelegatesToRemove">The resource delegates to remove.</param>
        /// <param name="agentAudit">The agent audit to create.</param>
        /// <param name="commit">Should commit, default to true.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateDelegationAsync(Dependent dependent, IEnumerable<ResourceDelegate> resourceDelegatesToRemove, AgentAudit agentAudit, bool commit = true);

        /// <summary>
        /// Retrieve all user HDIDs of protected dependents from the database.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of HDID strings.</returns>
        Task<IEnumerable<string>> GetProtectedDependentHdidsAsync(CancellationToken ct);
    }
}
