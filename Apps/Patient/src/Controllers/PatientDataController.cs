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
namespace HealthGateway.Patient.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using FluentValidation;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Patient.Constants;
    using HealthGateway.Patient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Endpoint to query and manage patient related data.
    /// </summary>
    [ApiController]
    [ApiVersion("2.0")]
    [Route("[controller]")]
    [Authorize]
    public class PatientDataController : ControllerBase
    {
        private readonly IPatientDataService patientDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDataController"/> class.
        /// </summary>
        /// <param name="patientDataService">DI service.</param>
        public PatientDataController(IPatientDataService patientDataService)
        {
            this.patientDataService = patientDataService;
        }

        /// <summary>
        /// Queries patient data for a specific patient and one or more data types.
        /// </summary>
        /// <param name="hdid">The patient hdid.</param>
        /// <param name="patientDataTypes">array of data types to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>object with an array of patient data information.</returns>
        [HttpGet("{hdid}")]
        [Authorize(policy: PatientPolicy.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<PatientDataResponse>> Get(string hdid, [FromQuery] PatientDataType[] patientDataTypes, CancellationToken ct)
        {
            ValidateGetRequest(hdid, patientDataTypes);
            PatientDataQuery query = new(hdid, patientDataTypes);
            return await this.patientDataService.QueryAsync(query, ct);
        }

        /// <summary>
        /// Gets a patient's file by id.
        /// </summary>
        /// <param name="hdid">The patient hdid.</param>
        /// <param name="fileId">The file id.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The patient file.</returns>
        [HttpGet("{hdid}/file/{fileId}")]
        [Authorize(policy: PatientPolicy.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<PatientFileResponse>> GetFile(string hdid, string fileId, CancellationToken ct)
        {
            ValidateGetFileRequest(hdid, fileId);
            PatientFileQuery query = new(hdid, fileId);
            return await this.patientDataService.QueryAsync(query, ct) ??
                   throw new NotFoundException($"file {fileId} not found");
        }

        private static void ValidateGetRequest(string hdid, PatientDataType[] patientDataTypes)
        {
            if (string.IsNullOrEmpty(hdid))
            {
                throw new ValidationException("Hdid is missing");
            }

            if (patientDataTypes == null || patientDataTypes.Length == 0)
            {
                throw new ValidationException("Must have at least one data type");
            }
        }

        private static void ValidateGetFileRequest(string hdid, string fileId)
        {
            if (string.IsNullOrEmpty(hdid))
            {
                throw new ValidationException("Hdid is missing");
            }

            if (string.IsNullOrEmpty(fileId))
            {
                throw new ValidationException("File id is missing");
            }
        }
    }
}
