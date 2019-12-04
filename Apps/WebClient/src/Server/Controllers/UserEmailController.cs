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
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.Authorization;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user email interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class UserEmailController
    {
        private readonly IUserEmailService userEmailService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailController"/> class.
        /// </summary>
        /// <param name="userEmailService">The injected user email service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="authorizationService">The injected authorization service.</param>
        public UserEmailController(
            IUserEmailService userEmailService,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            this.userEmailService = userEmailService;
            this.httpContextAccessor = httpContextAccessor;
            this.authorizationService = authorizationService;
        }

        /// <summary>
        /// Validates an email invite.
        /// </summary>
        /// <returns>The an empty response.</returns>
        /// <param name="inviteKey">The email invite key.</param>
        /// <response code="200">The email was validated.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The invite key was not found.</response>
        [HttpGet]
        [Route("Validate/{inviteKey}")]
        public IActionResult ValidateEmail(Guid inviteKey)
        {
            string hdid = this.httpContextAccessor.HttpContext.User.FindFirst("hdid").Value;
            if (this.userEmailService.ValidateEmail(hdid, inviteKey))
            {
                return new OkResult();
            }
            else
            {
                return new NotFoundResult();
            }
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
        [Route("{hdid}")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> GetUserEmailInvite(string hdid)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
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
            }

            EmailInvite result = this.userEmailService.RetrieveLastInvite(hdid);

            return new JsonResult(result);
        }
    }
}
