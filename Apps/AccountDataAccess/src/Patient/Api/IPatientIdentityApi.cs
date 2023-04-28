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
namespace HealthGateway.AccountDataAccess.Patient.Api
{
    using System.Threading.Tasks;
    using Refit;

    /// <summary>
    /// Interface to interact with PHSA Patient Identity API.
    /// </summary>
    internal interface IPatientIdentityApi
    {
        /// <summary>
        /// Retrieves the patient by PID.
        /// </summary>
        /// <param name="pid">The PID to lookup.</param>
        /// <returns>The Patient matching the id.</returns>
        [Get("/patient/{pid}/patient-identity")]
        Task<PatientResult?> PatientLookupByHdidAsync(string pid);
    }

    internal record PatientResult(PatientMetadata Metadata, PatientIdentity Data);

    internal record PatientMetadata;
}
