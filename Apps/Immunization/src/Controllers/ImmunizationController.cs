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
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Immunization.Models;
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
    public class ImmunizationController : ControllerBase
    {
        private readonly ILogger logger;

        /// <summary>
        /// Gets or sets the immunization data service.
        /// </summary>
        private readonly IImmunizationService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="svc">The immunization data service.</param>
        public ImmunizationController(
            ILogger<ImmunizationController> logger,
            IImmunizationService svc)
        {
            this.logger = logger;
            this.service = svc;
        }

        /// <summary>
        /// Gets an immunization record for the given id.
        /// </summary>
        /// <param name="hdid">The hdid patient id.</param>
        /// <param name="immunizationId">The immunization id.</param>
        /// <returns>The immunization record with the given id.</returns>
        /// <response code="200">Returns the List of Immunization records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{immunizationId}")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        public async Task<RequestResult<ImmunizationEvent>> GetImmunization([FromQuery] string hdid, string immunizationId)
        {
            this.logger.LogDebug($"Getting immunization {immunizationId} for user {hdid}");
            RequestResult<ImmunizationEvent> result = await this.service.GetImmunization(immunizationId).ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting immunization {immunizationId} for user {hdid}");
            return result;
        }

        /// <summary>
        /// Gets a json list of immunization records.
        /// </summary>
        /// <param name="hdid">The hdid patient id.</param>
        /// <returns>A list of immunization records for the given patient identifier.</returns>
        /// <response code="200">Returns the List of Immunization records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        public async Task<RequestResult<ImmunizationResult>> GetImmunizations([FromQuery] string hdid)
        {
            this.logger.LogDebug($"Getting immunizations for user {hdid}");
            RequestResult<ImmunizationResult> result = await this.service.GetImmunizations().ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting immunizations for user {hdid}");
            return result;
        }
    }
}
