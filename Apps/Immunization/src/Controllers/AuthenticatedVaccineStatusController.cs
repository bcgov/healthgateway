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
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Database.Constants;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The authenticated vaccine status controller.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class AuthenticatedVaccineStatusController : ControllerBase
    {
        private readonly ILogger logger;

        /// <summary>
        /// Gets or sets the immunization data service.
        /// </summary>
        private readonly IImmunizationService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedVaccineStatusController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="svc">The immunization data service.</param>
        public AuthenticatedVaccineStatusController(
            ILogger<ImmunizationController> logger,
            IImmunizationService svc)
        {
            this.logger = logger;
            this.service = svc;
        }

        /// <summary>
        /// Requests the vaccine status for the supplied HDID.
        /// </summary>
        /// <param name="hdid">The patient's HDID.</param>
        /// <returns>The wrapped vaccine status.</returns>
        /// <response code="200">Returns the Vaccine status.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        public async Task<IActionResult> GetVaccineStatus([FromQuery] string hdid)
        {
            this.logger.LogDebug($"Getting vaccine status for HDID {hdid}");
            RequestResult<VaccineStatus> result = await this.service.GetCovidVaccineStatus(hdid).ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting vaccine status for HDID {hdid}");
            return new JsonResult(result);
        }

        /// <summary>
        /// Requests the COVID-19 vaccine record for the supplied HDID if the user is the owner.
        /// </summary>
        /// <param name="hdid">The patient's HDID.</param>
        /// <param name="proofTemplate">The template to use for the generated vaccine proof.</param>
        /// <returns>The PDF Vaccine Record.</returns>
        /// <response code="200">Returns the vaccine record.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("pdf")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        public async Task<IActionResult> GetVaccineRecordPdf([FromQuery] string hdid, [FromQuery]VaccineProofTemplate proofTemplate)
        {
            this.logger.LogDebug($"Getting vaccine record for HDID {hdid}");
            RequestResult<CovidVaccineRecord> result = await this.service.GetCovidVaccineRecord(hdid, proofTemplate).ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting vaccine record for HDID {hdid}");
            return new JsonResult(result);
        }
    }
}
