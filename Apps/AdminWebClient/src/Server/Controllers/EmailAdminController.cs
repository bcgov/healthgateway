﻿// -------------------------------------------------------------------------
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
    using HealthGateway.Admin.Services;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Web API to handle user email interactions.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class EmailAdminController
    {
        private readonly ILogger<EmailAdminController> logger;
        private readonly IEmailAdminService emailAdminService;
        private readonly IEmailQueueService emailQueueService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAdminController"/> class.
        /// </summary>
        /// <param name="logger">The injected ILogger.</param>
        /// <param name="emailAdminService">The injected user email admin service.</param>
        /// <param name="emailQueueService">The injected user email queue service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="authorizationService">The injected authorization service.</param>
        public EmailAdminController(
            ILogger<EmailAdminController> logger,
            IEmailAdminService emailAdminService,
            IEmailQueueService emailQueueService,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            this.logger = logger;
            this.emailAdminService = emailAdminService;
            this.emailQueueService = emailQueueService;
            this.httpContextAccessor = httpContextAccessor;
            this.authorizationService = authorizationService;
        }

        /// <summary>
        /// Retrieves a list of emails from the system.
        /// </summary>
        /// <returns>Alist of email.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        public IActionResult GetEmails()
        {
            return new JsonResult(this.emailAdminService.GetEmails());
        }

        /// <summary>
        /// Creates a clone of the email id and requeues it for sending.
        /// </summary>
        /// <returns>An ok result unless the email id is not foun.</returns>
        /// <param name="emailId">The email Id to resend.</param>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Route("{emailId}")]
        public IActionResult ResendEmail(Guid emailId)
        {
            try
            {
                this.emailQueueService.CloneAndQueue(emailId);
            }
            catch (ArgumentException e)
            {
                this.logger.LogError($"Error cloning and sending emailId {emailId} with excpetion {e.ToString()}");
                return new BadRequestResult();
            }

            return new OkResult();
        }
    }
}
