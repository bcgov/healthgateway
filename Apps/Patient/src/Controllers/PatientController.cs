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
    using System;
    using System.Threading.Tasks;

    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Patient controller.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    //[Authorize]
    public class PatientController : ControllerBase
    {
        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger<PatientController> logger;

        /// <summary>
        /// Gets or sets the patient data service.
        /// </summary>
        private readonly IPatientService service;

        /// <summary>
        /// Gets or sets the http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="patientService">The patient data service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        public PatientController(ILogger<PatientController> logger, IHttpContextAccessor httpContextAccessor, IPatientService patientService)
        {
            this.logger = logger;
            this.service = patientService;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets a json of patient record.
        /// </summary>
        /// <returns>The patient record.</returns>
        /// <param name="hdid">The patient hdid.</param>
        /// <response code="200">Returns the patient record.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        //[Authorize(Policy = PatientPolicy.Read)]
        public async Task<IActionResult> GetPatient(string hdid)
        {
            RequestResult<PatientModel> result = await this.service.GetPatient(hdid).ConfigureAwait(true);
            return new JsonResult(result);
        }

        /// <summary>
        /// Searches for the patient given the an identifier.
        /// </summary>
        /// <returns>The patient record if found.</returns>
        /// <param name="identifier">The search identifier.</param>
        /// <response code="200">Returns the patient record.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Produces("application/json")]
        //[Authorize(Policy = PatientPolicy.Read)]
        public async Task<IActionResult> SearchPatientByIdentifier([FromQuery] string identifier)
        {
            try
            {
                ResourceIdentifier patientIdentifier = ResourceIdentifier.FromSearchString(identifier);
                RequestResult<PatientModel> result = await this.service.SearchPatientByIdentifier(patientIdentifier).ConfigureAwait(true);
                return new JsonResult(result);
            }
            catch (FormatException e)
            {
                this.logger.LogError($"Error extracting patient identifier {e.ToString()}");
                return new BadRequestResult();
            }

        }
    }
}