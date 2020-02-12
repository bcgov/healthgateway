﻿//-------------------------------------------------------------------------
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
namespace HealthGateway.PatientService.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.Authorization;
    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Patient controller.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the patient data service.
        /// </summary>
        private readonly IPatientService service;

        /// <summary>
        /// The authorization service.
        /// </summary>
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Gets or sets the http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientController"/> class.
        /// </summary>
        /// <param name="svc">The patient data service.</param>
        /// <param name="authorizationService">The injected authorization service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        public PatientController(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor, IPatientService svc)
        {
            this.service = svc;
            this.httpContextAccessor = httpContextAccessor;
            this.authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets a json of patient record.
        /// </summary>
        /// <returns>The patient record.</returns>
        /// <param name="hdid">The patient hdid.</param>
        /// <response code="200">Returns the patient record.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> GetPatient(string hdid)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            var isAuthorized = await this.authorizationService.AuthorizeAsync(user, hdid, PolicyNameConstants.UserIsPatient).ConfigureAwait(true);
            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }

            Patient result = await this.service.GetPatient(hdid).ConfigureAwait(true);
            return new JsonResult(result);
        }
    }
}