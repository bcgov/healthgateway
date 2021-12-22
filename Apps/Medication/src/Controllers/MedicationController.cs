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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Medication controller.
    /// </summary>
    [EnableCors("allowAny")]
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
        public RequestResult<MedicationInformation> GetMedication(string drugIdentifier)
        {
            // The database requires the dins to be the same size and padded with zeroes on the left
            string paddedDin = drugIdentifier!.PadLeft(8, '0');
            IDictionary<string, MedicationInformation> medications = this.medicationService.GetMedications(new List<string>() { paddedDin });

            RequestResult<MedicationInformation> result = new RequestResult<MedicationInformation>()
            {
                ResultStatus = ResultType.Success,
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
        public RequestResult<IDictionary<string, MedicationInformation>> GetMedications([FromQuery]IList<string> drugIdentifiers)
        {
            // The database requires the dins to be the same size and padded with zeroes on the left
            IList<string> paddedDinList = drugIdentifiers.Select(x => x.PadLeft(8, '0')).ToList();
            IDictionary<string, MedicationInformation> medications = this.medicationService.GetMedications(paddedDinList);

            RequestResult<IDictionary<string, MedicationInformation>> result = new RequestResult<IDictionary<string, MedicationInformation>>()
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
