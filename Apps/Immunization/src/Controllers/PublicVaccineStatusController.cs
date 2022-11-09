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
namespace HealthGateway.Immunization.Controllers
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Filters;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The public vaccine status controller.
    /// </summary>
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class PublicVaccineStatusController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly IVaccineStatusService vaccineStatusService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicVaccineStatusController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="vaccineStatusService">The injected vaccine status service.</param>
        public PublicVaccineStatusController(
            ILogger<PublicVaccineStatusController> logger,
            IVaccineStatusService vaccineStatusService)
        {
            this.logger = logger;
            this.vaccineStatusService = vaccineStatusService;
        }

        /// <summary>
        /// Requests the vaccine status for the supplied PHN, date of birth, and date of vaccine.
        /// </summary>
        /// <param name="phn">The personal health number to query.</param>
        /// <param name="dateOfBirth">The date of birth (yyyy-MM-dd) for the supplied PHN.</param>
        /// <param name="dateOfVaccine">The date of one of the vaccine doses (yyyy-MM-dd) for the supplied PHN.</param>
        /// <returns>The wrapped vaccine status.</returns>
        /// <response code="200">Returns the Vaccine status.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        public async Task<RequestResult<VaccineStatus>> GetVaccineStatus([FromHeader] string phn, [FromHeader] string dateOfBirth, [FromHeader] string dateOfVaccine)
        {
            this.logger.LogTrace("Getting vaccine status for PHN: {Phn}, DOB: {DateOfBirth}, and DOV: {DateOfVaccine}", phn, dateOfBirth, dateOfVaccine);
            RequestResult<VaccineStatus> result = await this.vaccineStatusService.GetPublicVaccineStatus(phn, dateOfBirth, dateOfVaccine).ConfigureAwait(true);
            this.logger.LogTrace("Finished getting vaccine status for PHN: {Phn}, DOB: {DateOfBirth}, and DOV: {DateOfVaccine}", phn, dateOfBirth, dateOfVaccine);

            return result;
        }

        /// <summary>
        /// Requests the vaccine proof for the supplied PHN, date of birth, and date of vaccine.
        /// </summary>
        /// <param name="phn">The personal health number to query.</param>
        /// <param name="dateOfBirth">The date of birth (yyyy-MM-dd) for the supplied PHN.</param>
        /// <param name="dateOfVaccine">The date of one of the vaccine doses (yyyy-MM-dd) for the supplied PHN.</param>
        /// <returns>The wrapped vaccine proof.</returns>
        /// <response code="200">Returns the vaccine proof.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Route("pdf")]
        [Produces("application/json")]
        public async Task<RequestResult<VaccineProofDocument>> GetVaccineProof([FromHeader] string phn, [FromHeader] string dateOfBirth, [FromHeader] string dateOfVaccine)
        {
            this.logger.LogDebug("Getting Vaccine Proof for PHN: {Phn}, DOB: {DateOfBirth}, and DOV: {DateOfVaccine}", phn, dateOfBirth, dateOfVaccine);
            RequestResult<VaccineProofDocument> result = await this.vaccineStatusService.GetPublicVaccineProof(phn, dateOfBirth, dateOfVaccine).ConfigureAwait(true);
            this.logger.LogDebug("Finished getting Vaccine Proof for PHN: {Phn}, DOB: {DateOfBirth}, and DOV: {DateOfVaccine}", phn, dateOfBirth, dateOfVaccine);

            return result;
        }
    }
}
