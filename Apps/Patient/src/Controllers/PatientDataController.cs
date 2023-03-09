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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthGateway.Common.AccessManagement.Authorization.Policy;
using HealthGateway.Common.Data.ErrorHandling;
using HealthGateway.Patient.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthGateway.Patient.Controllers
{
    /// <summary>
    /// Endpoint to query and manage patient related data
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class PatientDataController : ControllerBase
    {
        private readonly IPatientDataService patientDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDataController"/> class.
        /// </summary>
        /// <param name="patientDataService">DI service</param>
        public PatientDataController(IPatientDataService patientDataService)
        {
            this.patientDataService = patientDataService;
        }

        /// <summary>
        /// Queries patient data for a specific patient and one or more data types
        /// </summary>
        /// <param name="hdid">The patient hdid</param>
        /// <param name="patientDataTypes">array of data types to query</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>object with an array of patient data information</returns>
        [HttpGet("{hdid}")]
        [Authorize(policy: PatientPolicy.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<PatientDataResponse>> Get(string hdid, [FromQuery] PatientDataType[] patientDataTypes, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(hdid))
                throw new ProblemDetailsException(ExceptionUtility.CreateValidationError(nameof(hdid), "Hdid is missing"));
            if (patientDataTypes == null || !patientDataTypes.Any())
                throw new ProblemDetailsException(ExceptionUtility.CreateValidationError(nameof(patientDataTypes), "Must have at least one data type"));

            var response = await patientDataService.Query(new PatientDataQuery(hdid, patientDataTypes), ct);
            return response;
        }

        /// <summary>
        /// Gets a patient's file by id
        /// </summary>
        /// <param name="hdid">The patient hdid</param>
        /// <param name="fileId">The file id</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>File</returns>
        [HttpGet("{hdid}/file/{fileId}")]
        [Authorize(policy: PatientPolicy.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<PatientFileResponse>> GetFile(string hdid, string fileId, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(hdid))
                throw new ProblemDetailsException(ExceptionUtility.CreateValidationError(nameof(hdid), "Hdid is missing"));
            if (string.IsNullOrEmpty(fileId))
                throw new ProblemDetailsException(ExceptionUtility.CreateValidationError(nameof(fileId), "File id is missing"));

            return await patientDataService.Query(new PatientFileQuery(hdid, fileId), ct) ??
                   throw new ProblemDetailsException(ExceptionUtility.CreateNotFoundError($"file {fileId} not found"));
        }
    }
}
