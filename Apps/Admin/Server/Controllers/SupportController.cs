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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.Constants;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user support requests.
    /// </summary>
    /// <param name="supportService">The injected support service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer,SupportUser")]
    [SuppressMessage("Major Code Smell", "S6960:This controller has multiple responsibilities and could be split into 2 smaller controllers", Justification = "Team decision")]
    public class SupportController(ISupportService supportService) : ControllerBase
    {
        /// <summary>
        /// Retrieves the collection of patients that match the query.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>The collection of patient support details that match the query.</returns>
        /// <response code="200">Returns the collection of patient support details matching the query.</response>
        /// <response code="400">The request parameters did not pass validation.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from EMPI.</response>
        [HttpGet]
        [Route("Users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        public async Task<IEnumerable<PatientSupportResult>> GetPatients([FromQuery] PatientQueryType queryType, [FromQuery] string queryString, CancellationToken ct)
        {
            return await supportService.GetPatientsAsync(queryType, queryString, ct);
        }

        /// <summary>
        /// Retrieves patient support details, which includes messaging verifications, agent changes and blocked data sources
        /// matching the query.
        /// </summary>
        /// <param name="queryType">The type of query to be performed when searching for patient support details.</param>
        /// <param name="queryString">The string value associated with the query type when searching for patient support details.</param>
        /// <param name="refreshVaccineDetails">
        /// Whether the call should force cached vaccine validation details data to be
        /// refreshed.
        /// </param>
        /// <param name="includeApiRegistration">Indicates whether the response should include the Api Registration status.</param>
        /// <param name="includeImagingRefresh">Indicates whether the response should include imaging refresh data.</param>
        /// <param name="includeLabsRefresh">Indicates whether the response should include labs refresh data.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>Patient support details matching the query.</returns>
        /// <response code="200">Returns the patient support details matching the query.</response>
        /// <response code="400">The Personal Health Number (PHN) in the request is not valid..</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="500">The patient data may be returned but not valid for this request.</response>
        [HttpGet]
        [Route("PatientSupportDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<PatientSupportDetails> GetPatientSupportDetails(
            [FromQuery] ClientRegistryType queryType,
            [FromQuery] string queryString,
            [FromQuery] bool refreshVaccineDetails,
            [FromQuery] bool includeApiRegistration,
            [FromQuery] bool includeImagingRefresh,
            [FromQuery] bool includeLabsRefresh,
            CancellationToken ct)
        {
            ClaimsPrincipal user = this.HttpContext.User;
            bool userIsAdmin = user.IsInRole("AdminUser");
            bool userIsReviewer = user.IsInRole("AdminReviewer");
            bool userIsSupport = user.IsInRole("SupportUser");

            return await supportService.GetPatientSupportDetailsAsync(
                new PatientSupportDetailsQuery
                {
                    QueryType = queryType,
                    QueryParameter = queryString,
                    IncludeMessagingVerifications = userIsAdmin || userIsReviewer,
                    IncludeBlockedDataSources = userIsAdmin || userIsReviewer,
                    IncludeAgentActions = userIsAdmin,
                    IncludeDependents = userIsAdmin || userIsReviewer,
                    IncludeCovidDetails = userIsAdmin || userIsSupport,
                    IncludeApiRegistration = includeApiRegistration,
                    IncludeImagingRefresh = includeImagingRefresh,
                    IncludeLabsRefresh = includeLabsRefresh,
                    RefreshVaccineDetails = refreshVaccineDetails,
                },
                ct);
        }

        /// <summary>
        /// Blocks access to data source(s) for a given hdid.
        /// </summary>
        /// <param name="hdid">The hdid belonging to the data sources to block.</param>
        /// <param name="request">The request object containing data sources to block.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <response code="200">Data source access has been updated.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        [HttpPut]
        [Route("{hdid}/BlockAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "AdminUser")]
        public async Task BlockAccess(string hdid, BlockAccessRequest request, CancellationToken ct)
        {
            await supportService.BlockAccessAsync(hdid, request.DataSources, request.Reason, ct);
        }

        /// <summary>
        /// Requests a refresh of cached health data for a specified personal health number (PHN) from a given system source.
        /// </summary>
        /// <param name="request">
        /// Contains the PHN and the system source for which the health data should be refreshed.
        /// </param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The encoded immunization document.</returns>
        /// <response code="200">The request to retrieve the encoded immunization document was successful.</response>
        /// <response code="400">The request could not be submitted successfully.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="500">The vaccine result could not be retrieved due to an internal server error.</response>
        [HttpPost("Patient/RefreshHealthData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task RequestHealthDataRefresh([FromBody] HealthDataStatusRequest request, CancellationToken ct)
        {
            await supportService.RequestHealthDataRefreshAsync(request, ct);
        }
    }
}
