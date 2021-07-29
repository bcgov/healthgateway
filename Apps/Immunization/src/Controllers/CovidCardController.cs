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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Immunization controller.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class CovidCardController : ControllerBase
    {
        private readonly ILogger logger;

        /// <summary>
        /// Gets or sets the immunization data service.
        /// </summary>
        private readonly IImmunizationService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidCardController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="svc">The immunization data service.</param>
        public CovidCardController(
            ILogger<ImmunizationController> logger,
            IImmunizationService svc)
        {
            this.logger = logger;
            this.service = svc;
        }

        /// <summary>
        /// Gets the covid card for the supplied patient identifier.
        /// </summary>
        /// <param name="identifier">The patient identifier to fetch the covid card for.</param>
        /// <param name="identifierType">Type type of identifier passed in.</param>
        /// <returns>TThe PDF Covid Card.</returns>
        /// <response code="200">Returns the List of Immunization records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Policy = "Super Secure new Policy")]
        [Route("Lookup")]
        public async Task<IActionResult> Get([FromHeader] string identifier, [FromHeader] PatientIdentifierType identifierType)
        {
            this.logger.LogDebug($"Getting Covid Card for Patient Identifier {identifier}");
            RequestResult<string> result = await this.service.GetCovidCard(identifier, identifierType).ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting Covid Card for Patient Identifier {identifier}");
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets the Covid card for the supplied hdid if the user is the owner or has delegated access.
        /// </summary>
        /// <param name="hdid">The hdid patient id.</param>
        /// <returns>The PDF Covid Card.</returns>
        /// <response code="200">Returns the List of Immunization records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        public async Task<IActionResult> Get([FromQuery] string hdid)
        {
            this.logger.LogDebug($"Getting Covid Card for HDID for {hdid}");
            RequestResult<string> result = await this.service.GetCovidCard(hdid, PatientIdentifierType.HDID).ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting Covid Card for HDID for {hdid}");
            return new JsonResult(result);
        }
    }
}
