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
namespace HealthGateway.AccountDataAccess.Patient.Strategy
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The Context defines the interface of interest to clients.
    /// </summary>
    /// <param name="patientQueryStrategy">The implemented request to use.</param>
    internal class PatientQueryContext(PatientQueryStrategy patientQueryStrategy)
    {
        /// <summary>
        /// Returns patient from the specified source in the patient request.
        /// </summary>
        /// <param name="patientRequest">The patient request parameters to use.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The patient model.</returns>
        public async Task<PatientModel> GetPatientAsync(PatientRequest patientRequest, CancellationToken ct = default)
        {
            return await patientQueryStrategy.GetPatientAsync(patientRequest, ct);
        }
    }
}
