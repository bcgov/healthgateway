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
namespace HealthGateway.Admin.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// Service to manage agent access to the admin website.
    /// </summary>
    public interface IAgentAccessService
    {
        /// <summary>
        /// Provisions agent access to the admin website.
        /// </summary>
        /// <param name="agent">The agent model.</param>
        /// <returns>The created agent.</returns>
        Task<AdminAgent> ProvisionAgentAccessAsync(AdminAgent agent);

        /// <summary>
        /// Retrieves agents with access to the admin website that match the query.
        /// </summary>
        /// <param name="searchString">The query string to match agents against.</param>
        /// <param name="resultLimit">The maximum number of results to return.</param>
        /// <returns>The collection of matching agents.</returns>
        Task<IEnumerable<AdminAgent>> GetAgentsAsync(string searchString, int? resultLimit = 25);

        /// <summary>
        /// Updates agent access to the admin website.
        /// </summary>
        /// <param name="agent">The agent model.</param>
        /// <returns>The updated agent.</returns>
        Task<AdminAgent> UpdateAgentAccessAsync(AdminAgent agent);

        /// <summary>
        /// Removes an agent's access to the admin website.
        /// </summary>
        /// <param name="agentId">The unique identifier of the agent whose access should be terminated.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RemoveAgentAccessAsync(Guid agentId);
    }
}
