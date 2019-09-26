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
namespace HealthGateway.MedicationService.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.MedicationService.Models;
    using HealthGateway.MedicationService.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Medication controller.
    /// </summary>
    // [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class MedicationController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the medication data service.
        /// </summary>
        private readonly IMedicationService service;

        /// <summary>
        /// The http context provider.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationController"/> class.
        /// </summary>
        /// <param name="svc">The injected medication data service.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        public MedicationController(IMedicationService svc, IHttpContextAccessor httpAccessor)
        {
            this.service = svc;
            this.httpContextAccessor = httpAccessor;
        }

        /// <summary>
        /// Gets a json of medication record.
        /// </summary>
        /// <returns>The medication statement records.</returns>
        /// <param name="hdid">The patient hdid.</param>
        /// <response code="200">Returns the medication statement bundle.</response>
        /// <response code="401">The client is not authorized to retrieve the record.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        public async Task<List<MedicationStatement>> GetMedicationStatements(string hdid)
        {
            // Uses hardcoded phn until we have the token setup.
            // string phn = this.GetPatientPHN(hdid);
            string phn = "0009735353315";

            // Uses hardcoded userId until we have the token setup.
            // string phn = this.GetPatientPHN(hdid);
            string userId = "1001";
            string ipAddress = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            return await this.service.GetMedicationStatementsAsync(phn, userId, ipAddress).ConfigureAwait(true);
        }
    }
}