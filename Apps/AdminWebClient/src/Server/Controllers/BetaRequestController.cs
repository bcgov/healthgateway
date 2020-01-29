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
    using System.Diagnostics.Contracts;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;
    using HealthGateway.Admin.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    /// <summary>
    /// Web API to handle user email interactions.
    /// </summary>
    //[Authorize]
    //[ApiVersion("1.0")]
    [Route("v1/api/[controller]")]
    [ApiController]
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
        /// Validates an email invite.
        /// </summary>
        /// <returns>The invite email.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user email invite json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        public async Task<IActionResult> GetBetaRequests()
        {
            /*ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;

            // Validate that the query parameter matches the user claims
            if (!hdid.Equals(userHdid, StringComparison.CurrentCultureIgnoreCase))
            {
                return new BadRequestResult();
            }

            var isAuthorized = await this.authorizationService
                .AuthorizeAsync(user, userHdid, PolicyNameConstants.UserIsPatient)
                .ConfigureAwait(true);

            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }*/

            return new JsonResult(this.betaRequestService.GetPendingBetaRequests());
        }

        /// <summary>
        /// Validates an email invite.
        /// </summary>
        /// <returns>The invite email.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the user email invite json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPatch]
        //[Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> SendBetaRequestsInvites(List<string> betaRequestIds)
        {
            /*ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;

            // Validate that the query parameter matches the user claims
            if (!hdid.Equals(userHdid, StringComparison.CurrentCultureIgnoreCase))
            {
                return new BadRequestResult();
            }

            var isAuthorized = await this.authorizationService
                .AuthorizeAsync(user, userHdid, PolicyNameConstants.UserIsPatient)
                .ConfigureAwait(true);

            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }*/

            string referer = this.httpContextAccessor.HttpContext.Request
                .GetTypedHeaders()
                .Referer?
                .GetLeftPart(UriPartial.Authority);

            return new JsonResult(this.betaRequestService.SendInvites(betaRequestIds, referer));
        }
    }
}
