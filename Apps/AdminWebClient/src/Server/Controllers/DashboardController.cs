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
    using HealthGateway.Admin.Constants;
    using HealthGateway.Admin.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user feedback review.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
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
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        /// <returns>The count of registered users.</returns>
        /// <response code="200">Returns the count of registered users.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("RegisteredCount")]
        public IActionResult GetRegisteredUserCount(int timeOffset)
        {
            return new JsonResult(this.dashboardService.GetDailyRegisteredUsersCount(timeOffset));
        }

        /// <summary>
        /// Retrieves the count of logged in user in the last day.
        /// </summary>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        /// <returns>The count of logged in users in the current day.</returns>
        /// <response code="200">Returns the list of user feedbacks.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("LoggedInCount")]
        public IActionResult GetLoggedinUsersCount(int timeOffset)
        {
            return new JsonResult(this.dashboardService.GetDailyLoggedInUsersCount(timeOffset));
        }

        /// <summary>
        /// Retrieves the count of dependents.
        /// </summary>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        /// <returns>The count of logged in users in the current day.</returns>
        /// <response code="200">Returns the list of user feedbacks.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("DependentCount")]
        public IActionResult GetDependentCount(int timeOffset)
        {
            return new JsonResult(this.dashboardService.GetDailyDependentCount(timeOffset));
        }

        /// <summary>
        /// Retrieves the count recurring users.
        /// </summary>
        /// <param name="days">The number of unique days for evaluating a user.</param>
        /// <param name="startPeriod">The period start over which to evaluate the user.</param>
        /// <param name="endPeriod">The period end over which to evaluate the user.</param>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        /// <returns>The count of recurrent users.</returns>
        /// <response code="200">Returns the list of user feedbacks.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("RecurringUsers")]
        public IActionResult GetRecurringUsersCount(int days, string startPeriod, string endPeriod, int timeOffset)
        {
            return new JsonResult(this.dashboardService.GetRecurrentUserCount(days, startPeriod, endPeriod, timeOffset));
        }

        /// <summary>
        /// Retrieves a list Message Verifications matching the query.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <returns>A list of users matching the query.</returns>
        /// <response code="200">Returns the list of users matching the query.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("Users")]
        public IActionResult GetMessageVerifications(UserQueryType queryType, string queryString)
        {
            return new JsonResult(this.dashboardService.GetMessageVerifications(queryType, queryString));
        }
    }
}
