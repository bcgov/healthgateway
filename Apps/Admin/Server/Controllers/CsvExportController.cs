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
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Web API to export data from Health Gateway and return CSV files.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class CsvExportController
    {
        private readonly ICsvExportService dataExportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExportController"/> class.
        /// </summary>
        /// <param name="dataExportService">The injected data export service.</param>
        public CsvExportController(ICsvExportService dataExportService)
        {
            this.dataExportService = dataExportService;
        }

        /// <summary>
        /// Retrieves a list of User Profiles created inclusively between UTC dates if provided.
        /// </summary>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <returns>A CSV of the raw data. email.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("GetUserProfiles")]
        [Produces("text/csv")]
        public IActionResult GetUserProfiles(DateTime? startDate = null, DateTime? endDate = null)
        {
            return SendContentResponse("UserProfiles", this.dataExportService.GetUserProfiles(startDate, endDate));
        }

        /// <summary>
        /// Retrieves a list of Comments inclusively between UTC dates if provided.
        /// </summary>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <returns>The invite email.</returns>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("GetComments")]
        [Produces("text/csv")]
        public IActionResult GetComments(DateTime? startDate = null, DateTime? endDate = null)
        {
            return SendContentResponse("Comments", this.dataExportService.GetComments(startDate, endDate));
        }

        /// <summary>
        /// Retrieves a list of Notes inclusively between UTC dates if provided.
        /// </summary>
        /// <returns>The invite email.</returns>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("GetNotes")]
        [Produces("text/csv")]
        public IActionResult GetNotes(DateTime? startDate = null, DateTime? endDate = null)
        {
            return SendContentResponse("Notes", this.dataExportService.GetNotes(startDate, endDate));
        }

        /// <summary>
        /// Retrieves a list of Ratings inclusively between UTC dates if provided.
        /// </summary>
        /// <returns>A CSV of Ratings.</returns>
        /// <param name="startDate">The optional start date for the data.</param>
        /// <param name="endDate">The optional end date for the data.</param>
        /// <response code="200">Returns the list of beta requests.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("GetRatings")]
        [Produces("text/csv")]
        public IActionResult GetRatings(DateTime? startDate = null, DateTime? endDate = null)
        {
            return SendContentResponse("Ratings", this.dataExportService.GetRatings(startDate, endDate));
        }

        private static FileStreamResult SendContentResponse(string name, Stream csvStream)
        {
            csvStream.Seek(0, SeekOrigin.Begin);
            MediaTypeHeaderValue mimeType = new MediaTypeHeaderValue("text/csv");
            string filename = $"{name}_export_{DateTimeFormatter.FormatDate(DateTime.Now)}.csv";
            FileStreamResult result = new FileStreamResult(csvStream, mimeType)
            {
                FileDownloadName = filename,
            };
            return result;
        }
    }
}
