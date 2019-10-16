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
    using System.Threading.Tasks;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Common.Models;
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
        /// The drug lookup delegate.
        /// </summary>
        private readonly IDrugLookupDelegate dinLookupDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationController"/> class.
        /// </summary>
        /// <param name="medicationService">The injected medication data service.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        public MedicationController(IMedicationService medicationService, IHttpContextAccessor httpAccessor, IDrugLookupDelegate dinLookupDelegate)
        {
            this.medicationService = medicationService;
            this.httpContextAccessor = httpAccessor;
            this.dinLookupDelegate = dinLookupDelegate;
        }

        /// <summary>
        /// Gets a list of medications that match the requested drug identifiers.
        /// </summary>
        /// <returns>The medication statement records.</returns>
        /// <param name="drugIdentifiers">The list of medication identifiers to retrieve.</param>
        /// <response code="200">Returns the medication statement bundle.</response>
        /// <response code="401">The client is not authorized to retrieve the record.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        public async Task<RequestResult<List<Medication>>> GetMedications(List<string> drugIdentifiers)
        {
            RequestResult<List<Medication>> result = await this.dinLookupDelegate.FindMedicationsByDIN(drugIdentifiers);
            return result;
        }
    }
}