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
namespace HealthGateway.Immunization.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Mechanism to pull the Vaccine Proof.
    /// </summary>
    public interface IVaccineProofService
    {
        /// <summary>
        /// Gets the Vaccine proof document for the given vaccination result and template.
        /// </summary>
        /// <param name="userIdentifier">A unique identifier for this user.</param>
        /// <param name="vaccineProofRequest">The vaccine proof request to process.</param>
        /// <param name="proofTemplate">The template to use.</param>
        /// <returns>The report model wrapped in a request result object.</returns>
        Task<RequestResult<ReportModel>> GetVaccineProof(string userIdentifier, VaccineProofRequest vaccineProofRequest, VaccineProofTemplate proofTemplate);
    }
}
