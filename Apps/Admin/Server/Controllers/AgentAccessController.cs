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
namespace HealthGateway.Admin.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to manage agent access to the admin website.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class AgentAccessController : ControllerBase
    {
        private readonly IAgentAccessService agentAccessService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAccessController"/> class.
        /// </summary>
        /// <param name="agentAccessService">The injected agent access service.</param>
        public AgentAccessController(IAgentAccessService agentAccessService)
        {
            this.agentAccessService = agentAccessService;
        }

        /// <summary>
        /// Provisions agent access to the admin website.
        /// </summary>
        /// <returns>The created agent.</returns>
        /// <param name="agent">The agent model.</param>
        /// <response code="200">Returns the created agent.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from Keycloak.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminAgent))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> ProvisionAgentAccess(AdminAgent agent)
        {
            AdminAgent result = await this.agentAccessService.ProvisionAgentAccessAsync(agent).ConfigureAwait(true);
            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves agents with access to the admin website that match the query.
        /// </summary>
        /// <param name="query">The query string to match agents against.</param>
        /// <returns>The collection of matching agents.</returns>
        /// <response code="200">Returns the collection of matching agents.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from Keycloak.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdminAgent>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAgents(string query)
        {
            IEnumerable<AdminAgent> result = await this.agentAccessService.GetAgentsAsync(query).ConfigureAwait(true);
            return this.Ok(result);
        }

        /// <summary>
        /// Updates agent access to the admin website.
        /// </summary>
        /// <param name="agent">The agent model.</param>
        /// <returns>The updated agent.</returns>
        /// <response code="200">Returns the updated agent.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from Keycloak.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminAgent))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> UpdateAgentAccess(AdminAgent agent)
        {
            AdminAgent result = await this.agentAccessService.UpdateAgentAccessAsync(agent).ConfigureAwait(true);
            return this.Ok(result);
        }

        /// <summary>
        /// Removes an agent's access to the admin website.
        /// </summary>
        /// <param name="id">The unique identifier of the agent whose access should be terminated.</param>
        /// <returns>An empty result.</returns>
        /// <response code="200">The agent no longer has access to the admin website.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from Keycloak.</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> RemoveAgentAccess(Guid id)
        {
            await this.agentAccessService.RemoveAgentAccessAsync(id).ConfigureAwait(true);
            return this.Ok();
        }
    }
}
