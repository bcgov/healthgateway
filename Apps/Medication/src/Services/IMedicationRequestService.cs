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
namespace HealthGateway.Medication.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// Defines the medication request service interface.
    /// </summary>
    public interface IMedicationRequestService
    {
        /// <summary>
        /// Gets the patient medication request.
        /// </summary>
        /// <param name="hdid">The hdid to retrieve records for.</param>
        /// <returns>A RequestResult with MedicationHistory models as payload.</returns>
        Task<RequestResult<IList<MedicationRequest>>> GetMedicationRequests(string hdid);
    }
}
