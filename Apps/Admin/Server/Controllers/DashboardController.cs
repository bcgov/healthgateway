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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Services;
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
    [Authorize(Roles = "AdminUser,AdminReviewer,AdminAnalyst")]
    public class DashboardController
    {
        private readonly IDashboardService dashboardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="dashboardService">The injected dashboard service.</param>
        public DashboardController(IDashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }

        /// <summary>
        /// Retrieves the count of registered users.
        /// </summary>
        /// <param name="timeOffset">The current timezone offset from the client browser to UTC.</param>
        /// <returns>The count of registered users.</returns>
        /// <response code="200">Returns the count of registered users.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("RegisteredCount")]
        public IActionResult GetRegisteredUserCount(int timeOffset)
        {
            return new JsonResult(this.dashboardService.GetDailyRegisteredUsersCount(timeOffset));
        }

        /// <summary>
        /// Retrieves the count of logged in user in the last day.
        /// </summary>
        /// <param name="startDateLocal">The starting date to get the user counts in the client's Local time.</param>
        /// <param name="endDateLocal">The ending date for the query in the client's local time.</param>
        /// <param name="timeOffset">The current timezone offset from the client browser to UTC in minutes.</param>
        /// <returns>The count of logged in users in the current day.</returns>
        /// <response code="200">Returns the list of user feedbacks.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("LoggedInCount")]
        public IActionResult GetLoggedInUsersCount(DateOnly startDateLocal, DateOnly endDateLocal, int timeOffset)
        {
            return new JsonResult(this.dashboardService.GetDailyLoggedInUsersCount(startDateLocal, endDateLocal, timeOffset));
        }

        /// <summary>
        /// Retrieves the count of dependents.
        /// </summary>
        /// <param name="timeOffset">The current timezone offset from the client browser to UTC.</param>
        /// <returns>The count of logged in users in the current day.</returns>
        /// <response code="200">Returns the list of user feedbacks.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("DependentCount")]
        public IActionResult GetDependentCount(int timeOffset)
        {
            return new JsonResult(this.dashboardService.GetDailyDependentCount(timeOffset));
        }

        /// <summary>
        /// Retrieves recurring user counts.
        /// </summary>
        /// <param name="days">The number of unique days for evaluating a user.</param>
        /// <param name="startPeriod">The period start over which to evaluate the user.</param>
        /// <param name="endPeriod">The period end over which to evaluate the user.</param>
        /// <param name="timeOffset">The current timezone offset from the client browser to UTC.</param>
        /// <returns>The recurrent user counts.</returns>
        /// <response code="200">Returns the list of user feedbacks.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("RecurringUserCounts")]
        public IActionResult GetRecurringUserCounts(int days, string startPeriod, string endPeriod, int timeOffset)
        {
            return new JsonResult(this.dashboardService.GetRecurrentUserCounts(days, startPeriod, endPeriod, timeOffset));
        }

        /// <summary>
        /// Retrieves the ratings summary.
        /// </summary>
        /// <param name="startPeriod">The period start to calculate the summary.</param>
        /// <param name="endPeriod">The period end to calculate the summary.</param>
        /// <param name="timeOffset">The current timezone offset from the client browser to UTC.</param>
        /// <returns>A dictionary pairing the ratings with the counts.</returns>
        /// <response code="200">Returns the ratings summary.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("Ratings/Summary")]
        public IActionResult GetRatingsSummary([FromQuery] string startPeriod, [FromQuery] string endPeriod, [FromQuery] int timeOffset)
        {
            return new JsonResult(this.dashboardService.GetRatingSummary(startPeriod, endPeriod, timeOffset));
        }

        /// <summary>
        /// Retrieves year of birth counts for users that have logged in between two dates.
        /// </summary>
        /// <returns>A dictionary mapping birth years to user counts.</returns>
        /// <param name="startPeriod">The start period for the data.</param>
        /// <param name="endPeriod">The end period for the data.</param>
        /// <param name="timeOffset">The current timezone offset from the client browser to UTC.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns a dictionary mapping birth years to user counts.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("YearOfBirthCounts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IDictionary<string, int>> GetYearOfBirthCounts([FromQuery] string startPeriod, [FromQuery] string endPeriod, [FromQuery] int timeOffset, CancellationToken ct)
        {
            return await this.dashboardService.GetYearOfBirthCountsAsync(startPeriod, endPeriod, timeOffset, ct);
        }
    }
}
