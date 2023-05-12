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
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Delegate that performs operations for models relating to AgentAudit.
    /// </summary>
    public interface IAgentAuditDelegate
    {
        /// <summary>
        /// Fetches the agent audit(s) by query options from the database.
        /// </summary>
        /// <param name="hdid">The hdid to search.</param>
        /// <param name="group">The group to search.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<AgentAudit>> GetAgentAuditsAsync(string hdid, AuditGroup? group = null);
    }
}
