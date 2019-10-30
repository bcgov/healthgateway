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
    using HealthGateway.Common.Models;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Authorization;
    
    using Microsoft.AspNetCore.Mvc;

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
        /// Initializes a new instance of the <see cref="MedicationStatementController"/> class.
        /// </summary>
        /// <param name="medicationStatementService">The injected medication data service.</param>
        public MedicationStatementController(IMedicationStatementService medicationStatementService)
        {
            this.medicationStatementService = medicationStatementService;
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
        public async Task<RequestResult<List<MedicationStatement>>> GetMedicationStatements(string hdid)
        {
            return await this.GetMedicationStatements(hdid, null);
        }

        /// <summary>
        /// Gets a json of medication record.
        /// </summary>
        /// <returns>The medication statement records.</returns>
        /// <param name="hdid">The patient hdid.</param>
        /// <param name="protectiveWord">The clients protective word for Pharmanet.</param>
        /// <response code="200">Returns the medication statement bundle.</response>
        /// <response code="401">The client is not authorized to retrieve the record.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        public async Task<RequestResult<List<MedicationStatement>>> GetMedicationStatements(string hdid, string protectiveWord)
        {
            HNMessage<List<MedicationStatement>> medicationStatements = await this.medicationStatementService.GetMedicationStatements(hdid, protectiveWord).ConfigureAwait(true);

            if (medicationStatements.IsError)
            {
                RequestResult<List<MedicationStatement>> result = new RequestResult<List<MedicationStatement>>()
                {
                    ErrorMessage = medicationStatements.Error,
                };

                return result;
            }
            else
            {
                RequestResult<List<MedicationStatement>> result = new RequestResult<List<MedicationStatement>>()
                {
                    ResourcePayload = medicationStatements.Message,
                    PageIndex = 0,
                    PageSize = medicationStatements.Message.Count,
                    TotalResultCount = medicationStatements.Message.Count,
                };

                return result;
            }
        }
    }
}