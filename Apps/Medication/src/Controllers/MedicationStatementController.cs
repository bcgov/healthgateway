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
namespace HealthGateway.Medication.Controllers
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Medication controller.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicationStatementController : ControllerBase
    {
        /// <summary>
        /// The medication statement data service.
        /// </summary>
        private readonly IMedicationStatementService medicationStatementService;

        /// <summary>
        /// The authorization service.
        /// </summary>
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// The httpContextAccessor injected.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// The Configuration injected.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationStatementController"/> class.
        /// </summary>
        /// <param name="authorizationService">The injected authorization service.</param>
        /// <param name="medicationStatementService">The injected medication data service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public MedicationStatementController(IAuthorizationService authorizationService, IMedicationStatementService medicationStatementService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.medicationStatementService = medicationStatementService;
            this.httpContextAccessor = httpContextAccessor;
            this.authorizationService = authorizationService;
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets a json of medication record.
        /// </summary>
        /// <returns>The medication statement records.</returns>
        /// <param name="hdid">The patient hdid.</param>
        /// <param name="protectiveWord">The clients protective word for Pharmanet.</param>
        /// <response code="200">Returns the medication statement bundle.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> GetMedicationStatements(string hdid, [FromHeader] string? protectiveWord = null)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            var isAuthorized = await this.authorizationService.AuthorizeAsync(user, hdid, PolicyNameConstants.UserIsPatient).ConfigureAwait(true);
            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }

            string medicationDataSource = this.configuration.GetSection("MedicationDataSource").Value;

            // Switch between both types of systems
            if (medicationDataSource == "PharmaNet")
            {
                return new JsonResult(await this.medicationStatementService.GetMedicationStatements(hdid, protectiveWord).ConfigureAwait(true));
            }
            else if (medicationDataSource == "ODR")
            {
                return new JsonResult(await this.medicationStatementService.GetMedicationStatementsHistory(hdid, protectiveWord).ConfigureAwait(true));
            }
            else
            {
                throw new KeyNotFoundException("No valid data source configured");
            }
        }
    }
}
