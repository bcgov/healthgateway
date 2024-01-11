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
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user support requests.
    /// </summary>
    /// <param name="covidSupportService">The injected covid support service.</param>
    /// <param name="supportService">The injected support service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer,SupportUser")]
    public class SupportController(ICovidSupportService covidSupportService, ISupportService supportService) : ControllerBase
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
            return await supportService.GetPatientsAsync(queryType, queryString, ct).ConfigureAwait(true);
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
        /// <param name="ct">A cancellation token.</param>
        /// <returns>Patient support details matching the query.</returns>
        /// <response code="200">Returns the patient support details matching the query.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("PatientSupportDetails")]
        public async Task<PatientSupportDetails> GetPatientSupportDetails(
            [FromQuery] ClientRegistryType queryType,
            [FromQuery] string queryString,
            [FromQuery] bool refreshVaccineDetails,
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
                    RefreshVaccineDetails = refreshVaccineDetails,
                },
                ct);
        }

        /// <summary>
        /// Blocks access to data source(s) for a given hdid.
        /// </summary>
        /// <param name="hdid">The hdid belonging to the data sources to block.</param>
        /// <param name="request">The request object containing data sources to block.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <response code="200">Data source access has been updated.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        [HttpPut]
        [Route("{hdid}/BlockAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "AdminUser")]
        public async Task BlockAccess(string hdid, BlockAccessRequest request)
        {
            await supportService.BlockAccessAsync(hdid, request.DataSources, request.Reason).ConfigureAwait(true);
        }

        /// <summary>
        /// Triggers the process to physically mail the Vaccine Card document.
        /// </summary>
        /// <param name="request">The mail document request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">The vaccine proof request could be submitted successfully.</response>
        /// <response code="400">The vaccine proof request could not be submitted successfully.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="404">The patient could not be found for the personal health number..</response>
        [HttpPost]
        [Route("Patient/Document")]
        public async Task MailVaccineCard([FromBody] MailDocumentRequest request)
        {
            await covidSupportService.MailVaccineCardAsync(request).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the COVID-19 Vaccine Record document that includes the Vaccine Card and Vaccination History.
        /// </summary>
        /// <param name="phn">The personal health number that matches the document to retrieve.</param>
        /// <returns>The encoded immunization document.</returns>
        /// <response code="200">The request to retrieve the encoded immunization document was successful.</response>
        /// <response code="400">The request could not be submitted successfully.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("Patient/Document")]
        public async Task<ReportModel> RetrieveVaccineRecord([FromQuery] string phn)
        {
            return await covidSupportService.RetrieveVaccineRecordAsync(phn).ConfigureAwait(true);
        }

        /// <summary>
        /// Submitting a completed anti viral screening form.
        /// </summary>
        /// <param name="request">The covid therapy assessment request to use for submission.</param>
        /// <returns>A covid therapy assessment response.</returns>
        /// <response code="200">Returns a covid therapy assessment response.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpPost]
        [Produces("application/json")]
        [Route("CovidAssessment")]
        [Authorize(Roles = "SupportUser,AdminUser")]
        public async Task<CovidAssessmentResponse> SubmitCovidAssessment([FromBody] CovidAssessmentRequest request)
        {
            return await covidSupportService.SubmitCovidAssessmentAsync(request).ConfigureAwait(true);
        }
    }
}
