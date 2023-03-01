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
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.ViewModels;
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

        private readonly IMapper autoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        /// <param name="patientService">The patient data service.</param>
        /// <param name="patientServiceV2">The V2 patient data service.</param>
        public PatientController(IPatientService patientService, Services.IPatientService patientServiceV2, IMapper autoMapper)
        {
            this.service = patientService;
            this.serviceV2 = patientServiceV2;
            this.autoMapper = autoMapper;
        }

        /// <summary>
        /// Gets a json of patient record.
        /// </summary>
        /// <returns>The patient record.</returns>
        /// <param name="hdid">The patient hdid.</param>
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
        public async Task<RequestResult<PatientModelV1>> GetPatient(string hdid)
        {
            RequestResult<PatientModel> patientRequestResult = await this.service.GetPatient(hdid).ConfigureAwait(true);

            // Map PatientModel to PatientModelV1
            RequestResult<PatientModelV1> v1RequestResult = this.autoMapper.Map<RequestResult<PatientModel>, RequestResult<PatientModelV1>>(patientRequestResult);
            v1RequestResult.ResourcePayload = this.autoMapper.Map<PatientModel, PatientModelV1>(patientRequestResult.ResourcePayload);
            return v1RequestResult;
        }

        /// <summary>
        /// Gets a json of patient record.
        /// </summary>
        /// <param name="hdid">The patient hdid.</param>
        /// <returns>The patient record.</returns>
        /// <response code="200">Returns the patient record.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="404">The patient could not be found.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="502">Unable to get response from client registry.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<PatientModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ApiVersion("2.0")]
        [Route("{hdid}")]
        [Authorize(Policy = PatientPolicy.Read)]
        public async Task<IActionResult> GetPatientV2(string hdid)
        {
            return this.Ok(await this.serviceV2.GetPatient(hdid).ConfigureAwait(true));
        }
    }
}
