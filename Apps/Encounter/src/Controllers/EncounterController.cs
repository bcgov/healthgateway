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
namespace HealthGateway.Encounter.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Filters;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Encounter controller.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Route("[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class EncounterController : ControllerBase
    {
        private readonly ILogger logger;

        /// <summary>
        /// Gets or sets the Encounter data service.
        /// </summary>
        private readonly IEncounterService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="service">The Encounter data service.</param>
        public EncounterController(
            ILogger<EncounterController> logger,
            IEncounterService service)
        {
            this.logger = logger;
            this.service = service;
        }

        /// <summary>
        /// Gets a json list of encounter records.
        /// </summary>
        /// <param name="hdid">The hdid patient id.</param>
        /// <returns>a list of Encounter records.</returns>
        /// <response code="200">Returns the List of Encounter records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        [Authorize(Policy = EncounterPolicy.Read)]
        public async Task<RequestResult<IEnumerable<EncounterModel>>> GetEncounters(string hdid)
        {
            this.logger.LogDebug($"Getting claims from controller... {hdid}");
            RequestResult<IEnumerable<EncounterModel>> result = await this.service.GetEncounters(hdid).ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting claims from controller... {hdid}");
            return result;
        }
    }
}
