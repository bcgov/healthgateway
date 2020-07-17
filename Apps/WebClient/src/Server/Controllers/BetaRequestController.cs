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
namespace HealthGateway.WebClient.Controllers
{
    using System;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle requests for beta users.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class BetaRequestController
    {
        private readonly IBetaRequestService betaRequestService;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaRequestController"/> class.
        /// </summary>
        /// <param name="betaRequestService">The injected beta request service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        public BetaRequestController(
            IBetaRequestService betaRequestService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.betaRequestService = betaRequestService;
        }

        /// <summary>
        /// Posts a beta request json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="betaRequest">The beta request model.</param>
        /// <response code="200">The beta request record was saved.</response>
        /// <response code="400">The beta request was already inserted.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Authorize(Policy = UserPolicy.UserOnly)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult CreateBetaRequest([FromHeader] string authorization, [FromBody] BetaRequest betaRequest)
        {
            // this.logger.logTrace(authorization);
            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer?
                .GetLeftPart(UriPartial.Authority) !;

            RequestResult<BetaRequest> result = this.betaRequestService.PutBetaRequest(betaRequest, referer);
            return new JsonResult(result);
        }

        /// <summary>
        /// Retrieves the latest user queued email.
        /// </summary>
        /// <returns>The email for the suer queued.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the email for the queued user.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}")]
        [Authorize(Policy = UserPolicy.Read)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult GetBetaRequest([FromHeader] string authorization, string hdid)
        {
            BetaRequest result = this.betaRequestService.GetBetaRequest(hdid);
            return new JsonResult(result);
        }
    }
}
