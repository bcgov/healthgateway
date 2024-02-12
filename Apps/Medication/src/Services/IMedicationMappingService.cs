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
namespace HealthGateway.Medication.Services
{
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.ODR;
    using HealthGateway.Medication.Models.Salesforce;

    /// <summary>
    /// Service to map between models at different layers.
    /// </summary>
    public interface IMedicationMappingService
    {
        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        MedicationRequest MapToMedicationRequest(SpecialAuthorityRequest source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        MedicationStatement MapToMedicationStatement(MedicationResult source);
    }
}
