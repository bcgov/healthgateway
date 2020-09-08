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
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
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
        /// Gets or sets the http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="svc">The immunization data service.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        public ImmunizationController(
            ILogger<ImmunizationController> logger,
            IImmunizationService svc,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.service = svc;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets a json list of immunization records.
        /// </summary>
        /// <param name="hdid">The hdid patient id.</param>
        /// <returns>a list of immunization records.</returns>
        /// <response code="200">Returns the List of Immunization records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        public async Task<IActionResult> GetImmunizations(string hdid)
        {
            this.logger.LogDebug($"Getting immunizations from controller... {hdid}");
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string accessToken = await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true);
            RequestResult<IEnumerable<ImmunizationModel>> result = await this.service.GetImmunizations(accessToken).ConfigureAwait(true);

            this.logger.LogDebug($"Finished getting immunizations from controller... {hdid}");
            return new JsonResult(result);
        }
    }
}