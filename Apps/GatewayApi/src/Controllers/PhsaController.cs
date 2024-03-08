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

namespace HealthGateway.GatewayApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle dependent interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class PhsaController : ControllerBase
    {
        private readonly IDependentService dependentService;
        private readonly IPatientDetailsService patientDetailsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhsaController"/> class.
        /// </summary>
        /// <param name="dependentService">The injected dependent service.</param>
        /// <param name="patientDetailsService">The injected user patient details service.</param>
        public PhsaController(IDependentService dependentService, IPatientDetailsService patientDetailsService)
        {
            this.dependentService = dependentService;
            this.patientDetailsService = patientDetailsService;
        }

        /// <summary>
        /// Gets all dependents for the specified user.
        /// </summary>
        /// <returns>The list of dependent model wrapped in a request result.</returns>
        /// <param name="hdid">The owner hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Authorize(Policy = SystemDelegatedPatientPolicy.Read)]
        [Route("dependents/{hdid}")]
        public async Task<ActionResult<RequestResult<IEnumerable<DependentModel>>>> GetAll(string hdid, CancellationToken ct)
        {
            return await this.dependentService.GetDependentsAsync(hdid, ct: ct);
        }

        /// <summary>
        /// Gets all dependents for the specified date range.
        /// </summary>
        /// <returns>The list of dependents wrapped in a request result.</returns>
        /// <param name="fromDateUtc">The from date in Utc. Required.</param>
        /// <param name="toDateUtc">The to date in Utc.</param>
        /// <param name="page">The page number. Defaults to 0.</param>
        /// <param name="pageSize">The page size. Max 5000.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Authorize(Policy = SystemDelegatedPatientPolicy.Read)]
        [Route("dependents")]
        public async Task<ActionResult<RequestResult<IEnumerable<GetDependentResponse>>>> GetAll(
            [FromQuery] DateTime fromDateUtc,
            [FromQuery] DateTime? toDateUtc,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 5000,
            CancellationToken ct = default)
        {
            return await this.dependentService.GetDependentsAsync(fromDateUtc, toDateUtc, page, pageSize, ct);
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
        /// <response code="500">An internal error occured processing patient data.</response>
        /// <response code="502">Unable to get response from client registry.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [Route("patients/{hdid}")]
        [Authorize(Policy = SystemDelegatedPatientPolicy.Read)]
        public async Task<ActionResult<PatientDetails>> GetPatient(string hdid, CancellationToken ct)
        {
            return await this.patientDetailsService.GetPatientAsync(hdid, PatientIdentifierType.Hdid, false, ct);
        }
    }
}
