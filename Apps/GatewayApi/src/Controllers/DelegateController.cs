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
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle delegate interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class DelegateController
    {
        private readonly IDelegateService delegateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateController"/> class.
        /// </summary>
        /// <param name="delegateService">The injected delegate service.</param>
        public DelegateController(
            IDelegateService delegateService)
        {
            this.delegateService = delegateService;
        }

        /// <summary>
        /// Creates a delegate invitation for the given delegator's hdid.
        /// </summary>
        /// <returns>The delegate invitation</returns>
        /// <param name="hdid">The delegator hdid.</param>
        /// <param name="request">The delegate invitation request model.</param>
        /// <response code="200">Returns the delegate invitation.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [Route("{hdid}/Invitations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<DelegateInvitation> CreateDelegateInvitation(string hdid, [FromBody] DelegateInvitationRequest request)
        {
            return await this.delegateService.CreateDelegateInvitationAsync(hdid, request);
        }
    }
}
