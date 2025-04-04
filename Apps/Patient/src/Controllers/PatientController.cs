//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Patient.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Patient.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Patient controller.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [SuppressMessage("SonarLint", "S6960:This controller has multiple responsibilities and could be split into 2 smaller controllers", Justification = "Team Decision")]
    public class PatientController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the patient data service.
        /// </summary>
        private readonly IPatientService service;

        /// <summary>
        /// Gets or sets the patient data service v2.
        /// </summary>
        private readonly Services.IPatientService serviceV2;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        /// <param name="patientService">The patient data service.</param>
        /// <param name="patientServiceV2">The V2 patient data service.</param>
        public PatientController(IPatientService patientService, Services.IPatientService patientServiceV2)
        {
            this.service = patientService;
            this.serviceV2 = patientServiceV2;
        }

        /// <summary>
        /// Gets a json of patient record.
        /// </summary>
        /// <returns>The patient record.</returns>
        /// <param name="hdid">The patient hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns the patient record.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Produces("application/json")]
        [ApiVersion("1.0")]
        [Route("{hdid}")]
        [Authorize(Policy = PatientPolicy.Read)]
        public async Task<RequestResult<PatientModel>> GetPatient(string hdid, CancellationToken ct)
        {
            return await this.service.GetPatientAsync(hdid, ct: ct);
        }

        /// <summary>
        /// Gets a json of patient record.
        /// </summary>
        /// <param name="hdid">The patient hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The patient record.</returns>
        /// <response code="200">Returns the patient record.</response>
        /// <response code="400">The request HDID did not result in a valid PHN internally.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The patient could not be found.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="500">Data is in a state that may not be returned.</response>
        /// <response code="502">Unable to get response from client registry.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        [ApiVersion("2.0")]
        [Route("{hdid}")]
        [Authorize(Policy = PatientPolicy.Read)]
        public async Task<ActionResult<PatientDetails>> GetPatientV2(string hdid, CancellationToken ct)
        {
            PatientDetails patientDetails = await this.serviceV2.GetPatientAsync(hdid, ct: ct);
            return this.Ok(patientDetails);
        }
    }
}
