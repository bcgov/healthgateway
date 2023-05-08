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
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
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
        /// Fetches the agent audit(s) by query options from the database.
        /// </summary>
        /// <param name="query">The query criteria.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<AgentAuditQueryResult> GetAgentAuditsAsync(AgentAuditQuery query);

        /// <summary>
        /// Adds or updates the dependent object including allowed delegation associations as well as resource delegates and agent
        /// audit to the DB.
        /// </summary>
        /// <param name="dependent">The dependent to update.</param>
        /// <param name="resourceDelegatesToRemove">The resource delegates to remove.</param>
        /// <param name="agentAudit">The agent audit to create.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateDelegationAsync(Dependent dependent, IEnumerable<ResourceDelegate> resourceDelegatesToRemove, AgentAudit agentAudit);
    }

    /// <summary>
    /// Agent audit query options to determine the agent audit results to return.
    /// </summary>
    public record AgentAuditQuery
    {
        /// <summary>
        /// Gets the audit group to search by.
        /// </summary>
        public AuditGroup? GroupCode { get; init; }

        /// <summary>
        /// Gets the hdid to search by.
        /// </summary>
        public string Hdid { get; init; } = string.Empty;
    }

    /// <summary>
    /// Returns agent audit query results.
    /// </summary>
    public record AgentAuditQueryResult
    {
        /// <summary>
        /// gets or sets the found items.
        /// </summary>
        public IEnumerable<AgentAudit> Items { get; set; } = Array.Empty<AgentAudit>();
    }
}
