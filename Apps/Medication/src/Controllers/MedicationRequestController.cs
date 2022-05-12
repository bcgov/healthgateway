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
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controller that handles requets for Medication Requests.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicationRequestController : ControllerBase
    {
        /// <summary>
        /// The medication request data service.
        /// </summary>
        private readonly IMedicationRequestService medicationRequestService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationRequestController"/> class.
        /// </summary>
        /// <param name="medicationRequestService">The injected medication request data service.</param>
        public MedicationRequestController(IMedicationRequestService medicationRequestService)
        {
            this.medicationRequestService = medicationRequestService;
        }

        /// <summary>
        /// Gets a json of medication request.
        /// </summary>
        /// <returns>The medication statement records.</returns>
        /// <param name="hdid">The patient hdid.</param>
        /// <response code="200">Returns the medication statement bundle.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        [Authorize(Policy = MedicationPolicy.MedicationRequestRead)]
        public async Task<RequestResult<IList<MedicationRequest>>> GetMedicationRequests(string hdid)
        {
            return await this.medicationRequestService.GetMedicationRequests(hdid).ConfigureAwait(true);
        }
    }
}
