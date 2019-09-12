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
    using HealthGateway.MedicationService.Models;
    using Microsoft.AspNetCore.Authorization;
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
        /// Initializes a new instance of the <see cref="MedicationController"/> class.
        /// </summary>
        /// <param name="svc">The medication data service.</param>
        public MedicationController(IPatientService svc)
        {
            this.service = svc;
        }

        /// <summary>
        /// Gets a json of medication record.
        /// </summary>
        /// <returns>The medication statement records.</returns>
        /// <param name="hdid">The patient hdid.</param>
        /// <response code="200">Returns the medication record.</response>
        /// <response code="401">The client is not authorzied to retrieve the record.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        public async System.Threading.Tasks.Task<Patient> GetMedicationStatements(string hdid)
        {
            return await this.service.GetMedicationStatements(hdid).ConfigureAwait(true);
        }
    }
}