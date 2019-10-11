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
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Medication controller.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicationController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the medication data service.
        /// </summary>
        private readonly IMedicationService medicationService;

        /// <summary>
        /// The http context provider.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// The authorization service provider.
        /// </summary>
        private readonly ICustomAuthorizationService authorizationService;

        /// <summary>
        /// The patient service provider used to retrieve Personal Health Number for subject.
        /// </summary>
        private readonly IPatientService patientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationController"/> class.
        /// </summary>
        /// <param name="medicationService">The injected medication data service.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="authZService">The injected authService authorization provider.</param>
        /// <param name="patientService">The injected patientService patient registry provider.</param>
        public MedicationController(IMedicationService medicationService, IHttpContextAccessor httpAccessor, ICustomAuthorizationService authZService, IPatientService patientService)
        {
            this.medicationService = medicationService;
            this.httpContextAccessor = httpAccessor;
            this.authorizationService = authZService;
            this.patientService = patientService;
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
        public async Task<HNMessage<List<MedicationStatement>>> GetMedications(string hdid)
        {
            string jwtString = this.httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];
            string phn = "9735353315";//await this.patientService.GetPatientPHNAsync(hdid).ConfigureAwait(true);
            string userId = this.httpContextAccessor.HttpContext.User.Identity.Name;
            IPAddress address = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            string ipv4Address = address.MapToIPv4().ToString();

            return await this.medicationService.GetMedicationsAsync(phn, userId, ipv4Address).ConfigureAwait(true);
        }
    }
}