// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Medication.Api
{
    using System.Threading.Tasks;
    using HealthGateway.Medication.Models.ODR;
    using Refit;

    /// <summary>
    /// API to interact with ODR for Medication History and ProtectiveWord.
    /// </summary>
    public interface IOdrApi
    {
        /// <summary>
        /// Gets the Medication History for the supplied request.
        /// </summary>
        /// <param name="request">The request to query.</param>
        /// <returns>The MedicationHistory.</returns>
        [Post("/patientProfile")]
        Task<MedicationHistory> GetMedicationHistoryAsync(MedicationHistory request);

        /// <summary>
        /// Gets the Protective Word.
        /// </summary>
        /// <param name="request">The protective word request.</param>
        /// <returns>The ProtectiveWord.</returns>
        [Post("/maintainProtectiveWord")]
        Task<ProtectiveWord> GetProtectiveWordAsync(ProtectiveWord request);
    }
}
