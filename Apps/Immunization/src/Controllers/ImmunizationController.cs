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
    using HealthGateway.Common.Authorization;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Immunization controller.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class ImmunizationController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the immunization data service.
        /// </summary>
        private readonly IImmsService service;

        /// <summary>
        /// Gets or sets the http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Gets or sets the authorization service.
        /// </summary>
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationController"/> class.
        /// </summary>
        /// <param name="svc">The immunization data service.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        /// <param name="authorizationService">The IAuthorizationService.</param>
        public ImmunizationController(IImmsService svc, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
        {
            this.service = svc;
            this.httpContextAccessor = httpContextAccessor;
            this.authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets a json list of immunization records.
        /// </summary>
        /// <param name="hdid">The health directed id for the subject.</param>
        /// <returns>a list of immunization records.</returns>
        /// <response code="200">Returns the List of Immunization records.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>

        [HttpGet]
        [Produces("application/json")]
        [Route("items")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> GetImmunizations(string hdid)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            var isAuthorized = await this.authorizationService.AuthorizeAsync(user, hdid, PolicyNameConstants.UserIsPatient).ConfigureAwait(true);
            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }

            return new JsonResult(this.service.GetImmunizations(hdid));
        }
    }
}