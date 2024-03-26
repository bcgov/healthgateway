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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The medication controller.
    /// </summary>
    [EnableCors("allowAny")]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class MedicationController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the medication service.
        /// </summary>
        private readonly IMedicationService medicationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationController"/> class.
        /// </summary>
        /// <param name="medicationService">The injected medication service.</param>
        public MedicationController(IMedicationService medicationService)
        {
            this.medicationService = medicationService;
        }

        /// <summary>
        /// Gets medication information matching the requested drug identifier.
        /// The drug identifier must be either a Health Canada DIN or a BC PharmaNet PIN.
        /// </summary>
        /// <returns>Medication information wrapped in a RequestResult.</returns>
        /// <param name="drugIdentifier">The drug identifier to retrieve.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns medication information wrapped in a RequestResult.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{drugIdentifier}")]
        public async Task<RequestResult<MedicationInformation>> GetMedication(string drugIdentifier, CancellationToken ct)
        {
            // The database requires the dins to be the same size and padded with zeroes on the left
            string paddedDin = drugIdentifier.PadLeft(8, '0');
            IDictionary<string, MedicationInformation> medications = await this.medicationService.GetMedicationsAsync([paddedDin], ct);

            medications.TryGetValue(paddedDin, out MedicationInformation? medication);
            RequestResult<MedicationInformation> result = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = medication,
                TotalResultCount = medications.Count,
                PageIndex = 0,
                PageSize = medications.Count,
            };

            return result;
        }

        /// <summary>
        /// Gets medication information matching the requested drug identifiers.
        /// </summary>
        /// <returns>A dictionary mapping drug identifiers to medication information wrapped in a RequestResult.</returns>
        /// <param name="drugIdentifiers">The list of drug identifiers to retrieve.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Returns a list of medication information wrapped in a RequestResult.</response>
        [HttpGet("")]
        [Produces("application/json")]
        public async Task<RequestResult<IDictionary<string, MedicationInformation>>> GetMedications([FromQuery] IList<string> drugIdentifiers, CancellationToken ct)
        {
            // The database requires the dins to be the same size and padded with zeroes on the left
            IList<string> paddedDrugIdentifiers = drugIdentifiers.Select(x => x.PadLeft(8, '0')).ToList();
            IDictionary<string, MedicationInformation> medications = await this.medicationService.GetMedicationsAsync(paddedDrugIdentifiers, ct);

            RequestResult<IDictionary<string, MedicationInformation>> result = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = medications,
                TotalResultCount = medications.Count,
                PageIndex = 0,
                PageSize = medications.Count,
            };

            return result;
        }
    }
}
