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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using HealthGateway.Admin.Server.Services;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAdminController"/> class.
        /// </summary>
        /// <param name="logger">The injected ILogger.</param>
        /// <param name="emailAdminService">The injected user email admin service.</param>
        /// <param name="emailQueueService">The injected user email queue service.</param>
        public EmailAdminController(
            ILogger<EmailAdminController> logger,
            IEmailAdminService emailAdminService,
            IEmailQueueService emailQueueService)
        {
            this.logger = logger;
            this.emailAdminService = emailAdminService;
            this.emailQueueService = emailQueueService;
        }

        /// <summary>
        /// Retrieves a list of emails from the system.
        /// </summary>
        /// <returns>A list of email.</returns>
        /// <response code="200">Returns the list of emails.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        public IActionResult GetEmails()
        {
            return new JsonResult(this.emailAdminService.GetEmails());
        }

        /// <summary>
        /// Creates a clone of the emails and requeues it for sending.
        /// </summary>
        /// <returns>An ok result unless the email id is not found.</returns>
        /// <param name="emailIds">The email Ids to resend.</param>
        /// <response code="200">Successfully queued all emails.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        public IActionResult ResendEmail(Collection<Guid> emailIds)
        {
            if (emailIds == null)
            {
                return new BadRequestResult();
            }

            try
            {
                foreach (var id in emailIds)
                {
                    this.emailQueueService.CloneAndQueue(id);
                }
            }
            catch (ArgumentException e)
            {
                this.logger.LogError($"Error cloning and sending emails with exception {e.ToString()}");
                return new BadRequestResult();
            }

            return new OkResult();
        }
    }
}
