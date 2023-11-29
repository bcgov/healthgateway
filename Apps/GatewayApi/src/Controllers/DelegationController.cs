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
namespace HealthGateway.GatewayApi.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle delegate interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class DelegationController
    {
        private readonly IDelegationService delegationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationController"/> class.
        /// </summary>
        /// <param name="delegationService">The injected delegate service.</param>
        public DelegationController(IDelegationService delegationService)
        {
            this.delegationService = delegationService;
        }

        /// <summary>
        /// Associates user's delegation to a profile.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="hdid">The delegate's hdid.</param>
        /// <param name="encryptedDelegationId">The encrypted delegation id.</param>
        /// <param name="ct">cancellation token.</param>
        /// <response code="200">Returns the sharing code.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Route("{hdid}/Associate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task AssociateDelegation(string hdid, [FromQuery] string encryptedDelegationId, CancellationToken ct)
        {
            await this.delegationService.AssociateDelegationAsync(hdid, encryptedDelegationId, ct);
        }

        /// <summary>
        /// Creates a delegation for the given delegator's hdid.
        /// </summary>
        /// <returns>The sharing code</returns>
        /// <param name="hdid">The delegator hdid.</param>
        /// <param name="request">The create delegation request model.</param>
        /// <param name="ct">cancellation token.</param>
        /// <response code="200">Returns the sharing code.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Route("{hdid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<string> CreateDelegation(string hdid, [FromBody] CreateDelegationRequest request, CancellationToken ct)
        {
            return await this.delegationService.CreateDelegationAsync(hdid, request, ct);
        }
    }
}
