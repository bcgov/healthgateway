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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
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
    [Route("[controller]/[action]")]
    [ApiController]
    [SuppressMessage("Major Code Smell", "S6960:Controllers should not have mixed responsibilities", Justification = "Team decision")]
    public class DataAccessController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessController"/> class.
        /// </summary>
        public DataAccessController()
        {
        }

        /// <summary>
        /// Gets all datasets blocked for the specified user.
        /// </summary>
        /// <returns>The list of datasets blocked for the identified user.</returns>
        /// <param name="hdid">The users hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns the list of blockedDatasets for the identified user.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [AllowAnonymous]
        //[Authorize(Policy = SystemDelegatedPatientPolicy.Read)]
        [Route("{hdid}")]
        public async Task<ActionResult> BlockedDatasets(string hdid, CancellationToken ct)
        {
            DataSource[] blockedDatasets = [DataSource.HealthVisit, DataSource.SpecialAuthorityRequest];
            var response = new
            {
                blcockedDatasets = blockedDatasets,
            };
            return await Task.FromResult(this.Ok(response));
        }

        /// <summary>
        /// Confirms if the supplied user is protected.
        /// </summary>
        /// <returns>An HTTP Status code representing a true (200)/false(404) depending if the user is found to be protected or not.</returns>
        /// <param name="hdid">The subjects hdid.</param>
        /// <param name="delegatehdid">The delegate hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The subject may be added may be added to the delegate user.</response>
        /// <response code="451">The subject must not be added as they are protected.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [AllowAnonymous]
        //[Authorize(Policy = SystemDelegatedPatientPolicy.Read)]
        [Route("{hdid}/{delgatehdid}")]
        public async Task<ActionResult> Protected(string hdid, string delegatehdid, CancellationToken ct)
        {
            return await Task.FromResult(this.Ok(true));
        }
    }
}
