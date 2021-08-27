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
    using System;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The COVID-19 Vaccine Record controller.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class CovidVaccineRecordController : ControllerBase
    {
        private readonly ILogger logger;

        /// <summary>
        /// Gets or sets the immunization data service.
        /// </summary>
        private readonly IImmunizationService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidVaccineRecordController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="svc">The immunization data service.</param>
        public CovidVaccineRecordController(
            ILogger<ImmunizationController> logger,
            IImmunizationService svc)
        {
            this.logger = logger;
            this.service = svc;
        }

        /// <summary>
        /// Gets the COVID-19 vaccine record for the supplied HDID if the user is the owner or has delegated access.
        /// </summary>
        /// <param name="hdid">The patient's HDID.</param>
        /// <returns>The PDF Vaccine Record.</returns>
        /// <response code="200">Returns the vaccine record.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        public async Task<IActionResult> Get([FromQuery] string hdid)
        {
            this.logger.LogDebug($"Getting vaccine record for HDID {hdid}");
            RequestResult<CovidVaccineRecord> result = await this.service.GetCovidVaccineRecord(hdid).ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting vaccine record for HDID {hdid}");
            return new JsonResult(result);
        }
    }
}
