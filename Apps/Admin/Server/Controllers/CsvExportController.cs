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
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Utils;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Web API to export data from Health Gateway and return CSV files.
    /// </summary>
    /// <param name="dataExportService">The injected data export service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
    public class CsvExportController(ICsvExportService dataExportService)
    {
        /// <summary>
        /// Retrieves a list of User Profiles created inclusively between UTC dates if provided.
        /// </summary>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <returns>A CSV of the raw data. email.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("GetUserProfiles")]
        [Produces("text/csv")]
        public IActionResult GetUserProfiles(DateTime? startDate = null, DateTime? endDate = null)
        {
            return SendContentResponse("UserProfiles", dataExportService.GetUserProfiles(startDate, endDate));
        }

        /// <summary>
        /// Retrieves a list of inactive users created exclusive of the days inactive.
        /// </summary>
        /// <param name="inactiveDays">The days inactive to filter the users last login.</param>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        /// <returns>A CSV of inactive users.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("GetInactiveUsers")]
        [Produces("text/csv")]
        public async Task<IActionResult> GetInactiveAdminUser(int inactiveDays, int timeOffset)
        {
            return SendContentResponse("InactiveUsers", await dataExportService.GetInactiveUsers(inactiveDays, timeOffset).ConfigureAwait(true));
        }

        /// <summary>
        /// Retrieves a list of Comments inclusively between UTC dates if provided.
        /// </summary>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <returns>The invite email.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("GetComments")]
        [Produces("text/csv")]
        public IActionResult GetComments(DateTime? startDate = null, DateTime? endDate = null)
        {
            return SendContentResponse("Comments", dataExportService.GetComments(startDate, endDate));
        }

        /// <summary>
        /// Retrieves a list of Notes inclusively between UTC dates if provided.
        /// </summary>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <returns>The invite email.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("GetNotes")]
        [Produces("text/csv")]
        public IActionResult GetNotes(DateTime? startDate = null, DateTime? endDate = null)
        {
            return SendContentResponse("Notes", dataExportService.GetNotes(startDate, endDate));
        }

        /// <summary>
        /// Retrieves a list of Ratings inclusively between UTC dates if provided.
        /// </summary>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <returns>A CSV of Ratings.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("GetRatings")]
        [Produces("text/csv")]
        public IActionResult GetRatings(DateTime? startDate = null, DateTime? endDate = null)
        {
            return SendContentResponse("Ratings", dataExportService.GetRatings(startDate, endDate));
        }

        /// <summary>
        /// Retrieves a list of User Feedback.
        /// </summary>
        /// <returns>A CSV of User Feedback.</returns>
        /// <response code="200">Returns the list of user feedback.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("GetUserFeedback")]
        [Produces("text/csv")]
        public IActionResult GetUserFeedback()
        {
            return SendContentResponse("UserFeedback", dataExportService.GetUserFeedback());
        }

        /// <summary>
        /// Retrieves a list of year of birth counts inclusively between UTC dates.
        /// </summary>
        /// <param name="startDateLocal">The starting date to get the user counts in the client's Local time.</param>
        /// <param name="endDateLocal">The ending date for the query in the client's local time.</param>
        /// <param name="timeOffset">The current timezone offset from the client browser to UTC.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A CSV of year of birth counts.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("GetYearOfBirthCounts")]
        [Produces("text/csv")]
        public async Task<FileStreamResult> GetYearOfBirthCounts([FromQuery] DateOnly startDateLocal, [FromQuery] DateOnly endDateLocal, [FromQuery] int timeOffset, CancellationToken ct)
        {
            Stream stream = await dataExportService.GetYearOfBirthCountsAsync(startDateLocal, endDateLocal, timeOffset, ct);
            return SendContentResponse("YearOfBirthCounts", stream);
        }

        private static FileStreamResult SendContentResponse(string name, Stream csvStream)
        {
            csvStream.Seek(0, SeekOrigin.Begin);
            MediaTypeHeaderValue mimeType = new("text/csv");
            string filename = $"{name}_export_{DateTimeFormatter.FormatDate(DateTime.Now)}.csv";
            FileStreamResult result = new(csvStream, mimeType)
            {
                FileDownloadName = filename,
            };
            return result;
        }
    }
}
