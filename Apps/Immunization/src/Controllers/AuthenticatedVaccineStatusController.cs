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
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models.PHSA;
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
    [Route("[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class AuthenticatedVaccineStatusController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly IVaccineStatusService vaccineStatusService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedVaccineStatusController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="vaccineStatusService">The injected vaccine status service.</param>
        public AuthenticatedVaccineStatusController(
            ILogger<ImmunizationController> logger,
            IVaccineStatusService vaccineStatusService)
        {
            this.logger = logger;
            this.vaccineStatusService = vaccineStatusService;
        }

        /// <summary>
        /// Requests the vaccine status for the supplied HDID.
        /// </summary>
        /// <param name="hdid">The patient's HDID.</param>
        /// <returns>The wrapped vaccine status.</returns>
        /// <response code="200">Returns the Vaccine Status.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        public async Task<RequestResult<VaccineStatus>> GetVaccineStatus([FromQuery] string hdid)
        {
            this.logger.LogDebug($"Getting vaccine status for HDID {hdid}");
            RequestResult<VaccineStatus> result = await this.vaccineStatusService.GetAuthenticatedVaccineStatus(hdid).ConfigureAwait(true);
            this.logger.LogDebug($"Finished getting vaccine status for HDID {hdid}");

            return result;
        }

        /// <summary>
        /// Requests the COVID-19 Vaccine Proof for the supplied HDID if the user is the owner.
        /// </summary>
        /// <param name="hdid">The patient's HDID.</param>
        /// <returns>The PDF Vaccine Proof.</returns>
        /// <response code="200">Returns the Vaccine Proof.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("pdf")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        public async Task<RequestResult<VaccineProofDocument>> GetVaccineProof([FromQuery] string hdid)
        {
            this.logger.LogDebug($"Getting  Vaccine Proof for HDID {hdid}");
            RequestResult<VaccineProofDocument> result = await this.vaccineStatusService.GetAuthenticatedVaccineProof(hdid).ConfigureAwait(true);
            this.logger.LogDebug($"Finished getting Vaccine Proof for HDID {hdid}");

            return result;
        }
    }
}
