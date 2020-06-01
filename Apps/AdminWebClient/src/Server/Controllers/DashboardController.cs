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
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models;
    using HealthGateway.Admin.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user feedback review.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class DashboardController
    {
        private readonly IDashboardService dashboardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="dashboardService">The injected dashboard service.</param>
        public DashboardController(
            IDashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }

        /// <summary>
        /// Retrieves the count of registered users.
        /// </summary>
        /// <returns>The count of registered users.</returns>
        /// <response code="200">Returns the count of registered users.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("RegisteredCount")]
        public IActionResult GetRegisteredUserCount()
        {
            return new JsonResult(this.dashboardService.GetRegisteredUserCount());
        }

        /// <summary>
        /// Retrieves the count of unregistered users that received an invite.
        /// </summary>
        /// <returns>The count of unregistered users that received an invite.</returns>
        /// <response code="200">Returns the count of unregistered users.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("UnregisteredInvitedCount")]
        public IActionResult GetUnregisteredInvitedUserCount()
        {
            return new JsonResult(this.dashboardService.GetUnregisteredInvitedUserCount());
        }

        /// <summary>
        /// Retrieves the count of logged in user in the last day.
        /// </summary>
        /// <param name="offset">The offset from the client browser to UTC.</param>
        /// <returns>The count of logged in users in the current day.</returns>
        /// <response code="200">Returns the list of user feedbacks.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("LoggedInCount")]
        public IActionResult GetTodayLoggedinUsersCount(int offset)
        {
            return new JsonResult(this.dashboardService.GetTodayLoggedInUsersCount(offset));
        }

        /// <summary>
        /// Retrieves the count of waitlisted users.
        /// </summary>
        /// <returns>The count of waitlisted users.</returns>
        /// <response code="200">Returns the count of waitlisted users.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("WaitlistCount")]
        public IActionResult GetWaitlistUserCount()
        {
            return new JsonResult(this.dashboardService.GetWaitlistUserCount());
        }
    }
}
