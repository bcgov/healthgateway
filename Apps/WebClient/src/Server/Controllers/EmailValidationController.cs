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
    using System.Diagnostics.Contracts;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.Authorization;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user profile interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class EmailValidationController
    {
        private readonly IEmailValidationService emailValidationService;

        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailValidationController"/> class.
        /// </summary>
        /// <param name="emailValidationService">The injected email validation service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        public EmailValidationController(
            IEmailValidationService emailValidationService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.emailValidationService = emailValidationService;
            this.httpContextAccessor = httpContextAccessor;
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
        [Route("{inviteKey}")]
        public IActionResult ValidateEmail(Guid inviteKey)
        {
            string hdid = this.httpContextAccessor.HttpContext.User.FindFirst("hdid").Value;
            if (this.emailValidationService.ValidateEmail(hdid, inviteKey))
            {
                return new OkResult();
            }
            else
            {
                return new NotFoundResult();
            }
        }
    }
}
