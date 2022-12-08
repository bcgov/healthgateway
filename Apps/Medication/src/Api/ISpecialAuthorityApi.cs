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
    using HealthGateway.Medication.Models.Salesforce;
    using Refit;

    /// <summary>
    /// Special Authority API that connects to Salesforce backend.
    /// </summary>
    public interface ISpecialAuthorityApi
    {
        /// <summary>
        /// Retrieves the wrapped response of MedicationRequests.
        /// </summary>
        /// <param name="phn">The PHN to query.</param>
        /// <param name="token">The access token to be used for the authorize header.</param>
        /// <returns>A wrapped response of SpecialAuthority requests.</returns>
        [Get("")]
        Task<ResponseWrapper> GetSpecialAuthorityRequestsAsync([Header("phn")] string phn, [Authorize] string token);
    }
}
