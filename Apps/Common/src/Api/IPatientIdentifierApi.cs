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
namespace HealthGateway.Common.Api
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models.PHSA;
    using Refit;

    /// <summary>
    /// Interface to retrieve patient identifiers.
    /// </summary>
    public interface IPatientIdentifierApi
    {
        /// <summary>
        /// Retrieves the patient identifier (PID) by HDID.
        /// Since we are only interested in the PID, other properties from the response are excluded.
        /// The IPatientIdentityApi in AccountDataAccess retrieves the full response.
        /// </summary>
        /// <param name="hdid">The HDID to lookup.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A response containing the patient identifier matching the HDID.</returns>
        [Get("/patient-identity/hdid/{hdid}")]
        Task<PatientIdentifierResponse> GetPatientIdentifierByHdidAsync(string hdid, CancellationToken ct = default);
    }
}
