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
    using Asp.Versioning;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API for the Admin dashboard.
    /// </summary>
    /// <param name="dashboardService">The injected dashboard service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer,AdminAnalyst")]
    public class DashboardController(IDashboardService dashboardService)
    {
        /// <summary>
        /// Retrieves all-time counts.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A model containing the all-time counts.</returns>
        /// <response code="200">Returns a model containing the all-time counts.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("AllTimeCounts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<AllTimeCounts> GetAllTimeCounts(CancellationToken ct)
        {
            return await dashboardService.GetAllTimeCountsAsync(ct);
        }

        /// <summary>
        /// Retrieves daily usage counts over a date range.
        /// </summary>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A model containing daily usage counts.</returns>
        /// <response code="200">Returns a model containing daily usage counts.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("DailyUsageCounts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<DailyUsageCounts> GetDailyUsageCounts(
            [FromQuery] DateOnly startDateLocal,
            [FromQuery] DateOnly endDateLocal,
            CancellationToken ct)
        {
            return await dashboardService.GetDailyUsageCountsAsync(startDateLocal, endDateLocal, ct);
        }

        /// <summary>
        /// Retrieves a count of recurring users over a date range.
        /// </summary>
        /// <param name="days">Minimum number of days users must have logged in within the period to count as recurring.</param>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A count of recurring users.</returns>
        /// <response code="200">Returns a count of recurring users.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Route("RecurringUserCount")]
        public async Task<int> GetRecurringUserCount(
            [FromQuery] int days,
            [FromQuery] DateOnly startDateLocal,
            [FromQuery] DateOnly endDateLocal,
            CancellationToken ct)
        {
            return await dashboardService.GetRecurringUserCountAsync(days, startDateLocal, endDateLocal, ct);
        }

        /// <summary>
        /// Retrieves unique app login counts over a date range.
        /// </summary>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The login counts for Health Gateway applications.</returns>
        /// <response code="200">Returns the login counts for Health Gateway applications.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("AppLoginCounts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<AppLoginCounts> GetAppLoginCounts(
            [FromQuery] DateOnly startDateLocal,
            [FromQuery] DateOnly endDateLocal,
            CancellationToken ct)
        {
            return await dashboardService.GetAppLoginCountsAsync(startDateLocal, endDateLocal, ct);
        }

        /// <summary>
        /// Retrieves the ratings summary.
        /// </summary>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A dictionary pairing the ratings with the counts.</returns>
        /// <response code="200">Returns the ratings summary.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("Ratings/Summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IDictionary<string, int>> GetRatingsSummary(
            [FromQuery] DateOnly startDateLocal,
            [FromQuery] DateOnly endDateLocal,
            CancellationToken ct)
        {
            return await dashboardService.GetRatingsSummaryAsync(startDateLocal, endDateLocal, ct);
        }

        /// <summary>
        /// Retrieves age counts for users that have logged in between two dates.
        /// </summary>
        /// <param name="startDateLocal">The local start date to query.</param>
        /// <param name="endDateLocal">The local end date to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A dictionary mapping ages to user counts.</returns>
        /// <response code="200">Returns a dictionary mapping birth years to user counts.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("AgeCounts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IDictionary<int, int>> GetAgeCounts(
            [FromQuery] DateOnly startDateLocal,
            [FromQuery] DateOnly endDateLocal,
            CancellationToken ct)
        {
            return await dashboardService.GetAgeCountsAsync(startDateLocal, endDateLocal, ct);
        }
    }
}
