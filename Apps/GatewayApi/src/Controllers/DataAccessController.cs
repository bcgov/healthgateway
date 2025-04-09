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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
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
        /// <response code="403">The client is authenticated but does not have permission to access the resource.</response>
        [HttpGet]
        [AllowAnonymous]
        // [Authorize(Policy = SystemDelegatedPatientPolicy.Read)]
        [Route("BlockedDatasets/{hdid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DataSource>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<DataSource>>> BlockedDatasets([FromRoute] [Required] string hdid, CancellationToken ct)
        {
            IEnumerable<DataSource> datasets = await dataAccessService.GetBlockedDatasetsAsync(hdid, ct);
            return this.Ok(datasets);
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
        /// <response code="403">The client is authenticated but does not have permission to access the resource.</response>
        /// <response code="404">No contact information was found for the specified user.</response>
        [HttpGet]
        [AllowAnonymous]
        // [Authorize(Policy = SystemDelegatedPatientPolicy.Read)]
        [Route("ContactInfo/{hdid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContactInfo))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContactInfo>> ContactInfo([FromRoute] [Required] string hdid, CancellationToken ct)
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
        /// A boolean value indicating the protection status of the subject user:
        /// - <c>true</c> if the user is protected;
        /// - <c>false</c> if the user is not protected.
        /// Both outcomes are returned with an HTTP <c>200 OK</c> status unless access is restricted or the request is invalid.
        /// </returns>
        /// <response code="200">
        /// The request was successful. Returns <c>true</c> if the subject is protected, or <c>false</c> if not.
        /// </response>
        /// <response code="401">Authentication is required and was not provided or is invalid.</response>
        /// <response code="403">The client is authenticated but does not have permission to access this resource.</response>
        /// <response code="451">
        /// The subject is protected, and delegation is not permitted due to legal or policy restrictions. Returns <c>true</c>.
        /// </response>
        [HttpGet]
        [AllowAnonymous]
        // [Authorize(Policy = SystemDelegatedPatientPolicy.Read)]
        [Route("Protected/{hdid}/{delegateHdid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status451UnavailableForLegalReasons)]
        public async Task<ActionResult<bool>> Protected([FromRoute] [Required] string hdid, [FromRoute] [Required] string delegateHdid, CancellationToken ct)
        {
            return await dataAccessService.Protected(hdid, delegateHdid, ct);
        }
    }
}
