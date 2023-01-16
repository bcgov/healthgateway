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
namespace HealthGateway.Admin.Client.Api
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using Refit;

    /// <summary>
    /// API for managing agent access to the admin website.
    /// </summary>
    public interface IAgentAccessApi
    {
        /// <summary>
        /// Provisions agent access to the admin website.
        /// </summary>
        /// <returns>The created agent.</returns>
        /// <param name="agent">The agent model.</param>
        [Post("/")]
        Task<AdminAgent> ProvisionAgentAccessAsync([Body] AdminAgent agent);

        /// <summary>
        /// Retrieves agents with access to the admin website that match the query.
        /// </summary>
        /// <param name="query">The query string to match agents against.</param>
        /// <returns>The collection of matching agents.</returns>
        [Get("/")]
        Task<IEnumerable<AdminAgent>> GetAgentsAsync(string query);

        /// <summary>
        /// Updates agent access to the admin website.
        /// </summary>
        /// <param name="agent">The agent model.</param>
        /// <returns>The updated agent.</returns>
        [Put("/")]
        Task<AdminAgent> UpdateAgentAccessAsync([Body] AdminAgent agent);

        /// <summary>
        /// Removes an agent's access to the admin website.
        /// </summary>
        /// <param name="id">The unique identifier of the agent whose access should be terminated.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Delete("/")]
        Task RemoveAgentAccessAsync(Guid id);
    }
}
