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
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    ///  Special API to data access values for a given user.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class DataAccessController(IDataAccessService dataAccessService) : ControllerBase
    {
        /// <summary>
        /// Retrieves the list of datasets that are blocked for the specified user.
        /// </summary>
        /// <param name="hdid">The user's HDID.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of blocked <see cref="DataSource"/> values associated with the specified user.</returns>
        /// <response code="200">Successfully retrieved the list of blocked datasets.</response>
        /// <response code="400">The request is malformed or missing required information (e.g., HDID).</response>
        /// <response code="401">Authentication is required and was not provided or is invalid.</response>
        [HttpGet]
        [Authorize(Policy = SystemDelegatedLraDataAccessPolicy.Read)]
        [Route("BlockedDatasets/{hdid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlockedDatasets))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<BlockedDatasets>> BlockedDatasets(
            [FromRoute] [Required]
            string hdid,
            CancellationToken ct)
        {
            BlockedDatasets blockedDatasets = await dataAccessService.GetBlockedDatasetsAsync(hdid, ct);
            return this.Ok(blockedDatasets);
        }

        /// <summary>
        /// Retrieves the contact information associated with the specified user.
        /// </summary>
        /// <param name="hdid">The user's HDID (Health Data ID).</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="ContactInfo"/> object representing the user's contact information.</returns>
        /// <response code="200">Successfully retrieved the contact information.</response>
        /// <response code="400">The request is malformed or missing required information (e.g., HDID).</response>
        /// <response code="401">Authentication is required and was not provided or is invalid.</response>
        /// <response code="404">No contact information was found for the specified user.</response>
        [HttpGet]
        [Authorize(Policy = SystemDelegatedLraDataAccessPolicy.Read)]
        [Route("ContactInfo/{hdid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContactInfo))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContactInfo>> ContactInfo(
            [FromRoute] [Required]
            string hdid,
            CancellationToken ct)
        {
            ContactInfo contactInfo = await dataAccessService.GetContactInfoAsync(hdid, ct);
            return this.Ok(contactInfo);
        }

        /// <summary>
        /// Checks whether the specified subject user is marked as protected.
        /// </summary>
        /// <param name="hdid">The subject user's HDID.</param>
        /// <param name="delegateHdid">The delegate user's HDID.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="UserProtection"/> model containing the specified user's HDID and the protection status of the subject
        /// user.
        /// </returns>
        /// <response code="200">
        /// The request was successful. The response contains a <see cref="UserProtection"/> object with the protection status.
        /// </response>
        /// <response code="401">Authentication is required and was not provided or is invalid.</response>
        [HttpGet]
        [Authorize(Policy = SystemDelegatedLraDataAccessPolicy.Read)]
        [Route("Protected/{hdid}/{delegateHdid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProtection))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserProtection>> Protected(
            [FromRoute] [Required]
            string hdid,
            [FromRoute] [Required]
            string delegateHdid,
            CancellationToken ct)
        {
            return await dataAccessService.GetUserProtectionAsync(hdid, delegateHdid, ct);
        }
    }
}
