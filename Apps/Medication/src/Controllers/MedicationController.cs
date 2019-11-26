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
    using System.Diagnostics.Contracts;
    using System.Linq;
    using HealthGateway.Common.Models;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Medication controller.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class MedicationController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the medication data service.
        /// </summary>
        private readonly IMedicationService medicationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationController"/> class.
        /// </summary>
        /// <param name="medicationService">The injected medication data service.</param>
        public MedicationController(IMedicationService medicationService)
        {
            this.medicationService = medicationService;
        }

        /// <summary>
        /// Gets a list of medications that match the requested drug identifier.
        /// The drug identifier must be either a Health Canada DIN or a BC Pharmanet PIN.
        /// </summary>
        /// <returns>The medication statement records.</returns>
        /// <param name="drugIdentifier">The medication identifier to retrieve.</param>
        /// <response code="200">Returns the medication statement bundle.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{drugIdentifier}")]
        public RequestResult<MedicationResult> GetMedication(string drugIdentifier)
        {
            Contract.Requires(drugIdentifier != null);

            // The database requires the dins to be the same size and padded with zeroes on the left
            string paddedDin = drugIdentifier.PadLeft(8, '0');
            Dictionary<string, MedicationResult> medications = this.medicationService.GetMedications(new List<string>() { paddedDin });

            RequestResult<MedicationResult> result = new RequestResult<MedicationResult>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = medications.ContainsKey(paddedDin) ? medications[paddedDin] : null,
                TotalResultCount = medications.Count,
                PageIndex = 0,
                PageSize = medications.Count,
            };

            return result;
        }

        /// <summary>
        /// Gets a list of medications that match the requested drug identifiers.
        /// </summary>
        /// <returns>The medication statement records.</returns>
        /// <param name="drugIdentifiers">The list of medication identifiers to retrieve.</param>
        /// <response code="200">Returns the medication statement bundle.</response>
        [HttpGet("")]
        [Produces("application/json")]
        public RequestResult<Dictionary<string, MedicationResult>> GetMedications([FromQuery]List<string> drugIdentifiers)
        {
            // The database requires the dins to be the same size and padded with zeroes on the left
            List<string> paddedDinList = drugIdentifiers.Select(x => x.PadLeft(8, '0')).ToList();
            Dictionary<string, MedicationResult> medications = this.medicationService.GetMedications(paddedDinList);

            RequestResult<Dictionary<string, MedicationResult>> result = new RequestResult<Dictionary<string, MedicationResult>>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = medications,
                TotalResultCount = medications.Count,
                PageIndex = 0,
                PageSize = medications.Count,
            };

            return result;
        }
    }
}