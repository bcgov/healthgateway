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
    using System.Threading.Tasks;

    /// <summary>
    /// The Context defines the interface of interest to clients.
    /// </summary>
    internal class PatientQueryContext
    {
        // The Context maintains a reference to one of the Strategy objects. The
        // Context does not know the concrete class of a request. It should
        // work with all strategies via the Strategy interface.
        private IPatientQuery patientQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientQueryContext"/> class.
        /// </summary>
        /// <param name="patientQuery">The implemented request to use.</param>
        public PatientQueryContext(IPatientQuery patientQuery)
        {
            this.patientQuery = patientQuery;
        }

        /// <summary>
        /// Sets implemented request to use.
        /// </summary>
        /// <param name="strategy">The implemented request to set.</param>
        public void SetStrategy(IPatientQuery strategy)
        {
            this.patientQuery = strategy;
        }

        /// <summary>
        /// Returns patient from the database.
        /// </summary>
        /// <param name="patientRequest">The patient request parameters to use.</param>
        /// <returns>The patient model.</returns>
        public async Task<PatientModel?> GetPatientAsync(PatientRequest patientRequest)
        {
            return await this.patientQuery.GetPatientAsync(patientRequest).ConfigureAwait(true);
        }
    }
}
