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
namespace HealthGateway.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Services;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user email interactions.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class BetaRequestController
    {
        private readonly IBetaRequestService betaRequestService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaRequestController"/> class.
        /// </summary>
        /// <param name="betaRequestService">The injected user email service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="authorizationService">The injected authorization service.</param>
        public BetaRequestController(
            IBetaRequestService betaRequestService,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            this.betaRequestService = betaRequestService;
            this.httpContextAccessor = httpContextAccessor;
            this.authorizationService = authorizationService;
        }

        /// <summary>
        /// Retrieves a list of pending beta requests.
        /// </summary>
        /// <returns>The invite email.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        public IActionResult GetBetaRequests()
        {
            return new JsonResult(this.betaRequestService.GetPendingBetaRequests());
        }

        /// <summary>
        /// Sends email invites to the beta requets with the given ids.
        /// </summary>
        /// <returns>A list of ids of the beta requests that where successfully processed.</returns>
        /// <param name="betaRequestIds">List of beta requests to send invites.</param>
        /// <response code="200">Returns the beta requests ids that where invited.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPatch]
        public IActionResult SendBetaRequestsInvites(List<string> betaRequestIds)
        {
            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer
                .GetLeftPart(UriPartial.Authority);

            return new JsonResult(this.betaRequestService.SendInvites(betaRequestIds, referer));
        }
    }
}
