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
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle delegation.
    /// </summary>
    /// <param name="delegationService">The injected delegation service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class DelegationController(IDelegationService delegationService)
    {
        /// <summary>
        /// Retrieves delegation information for a person.
        /// </summary>
        /// <param name="phn">The phn to query on.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>Information about the person and their delegates.</returns>
        /// <response code="200">Returns the requested delegation information.</response>
        /// <response code="400">The request parameters did not pass validation.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">Patient could not be found.</response>
        /// <response code="502">Unable to get response from EMPI.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        public async Task<DelegationInfo> GetDelegationInformation([FromHeader] string phn, CancellationToken ct)
        {
            return await delegationService.GetDelegationInformationAsync(phn, ct).ConfigureAwait(true);
        }

        /// <summary>
        /// Retrieves information about a potential delegate.
        /// </summary>
        /// <param name="phn">The phn to query on.</param>
        /// <returns>Information about the potential delegate.</returns>
        /// <response code="200">Returns the requested delegation information.</response>
        /// <response code="400">The request parameters did not pass validation.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">Patient could not be found.</response>
        /// <response code="502">Unable to get response from EMPI.</response>
        [HttpGet]
        [Route("Delegate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        public async Task<DelegateInfo> GetDelegateInformation([FromHeader] string phn)
        {
            return await delegationService.GetDelegateInformationAsync(phn).ConfigureAwait(true);
        }

        /// <summary>
        /// Protects the dependent and if necessary creates the allowed delegation(s) and keeps the resource delegates
        /// synchronized.
        /// </summary>
        /// <param name="dependentHdid">The hdid of the dependent to protect.</param>
        /// <param name="request">The request object containing data used to protect a dependent.</param>
        /// <returns>The agent action entry created from the operation.</returns>
        /// <response code="200">The dependent is protected.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        [HttpPut]
        [Route("{dependentHdid}/ProtectDependent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<AgentAction> ProtectDependent(string dependentHdid, ProtectDependentRequest request)
        {
            return await delegationService.ProtectDependentAsync(dependentHdid, request.DelegateHdids, request.Reason).ConfigureAwait(true);
        }

        /// <summary>
        /// Unprotects the dependent and if necessary removes the allowed delegation(s) and keeps the resource delegates
        /// synchronized.
        /// </summary>
        /// <param name="dependentHdid">The hdid of the dependent to unprotect.</param>
        /// <param name="request">The request object containing data used to unprotect a dependent.</param>
        /// <returns>The agent action entry created from the operation.</returns>
        /// <response code="200">The dependent is unprotected.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        /// <response code="404">The dependent could not be found.</response>
        [HttpPut]
        [Route("{dependentHdid}/UnprotectDependent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<AgentAction> UnprotectDependent(string dependentHdid, UnprotectDependentRequest request)
        {
            return await delegationService.UnprotectDependentAsync(dependentHdid, request.Reason).ConfigureAwait(true);
        }
    }
}
