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
    using HealthGateway.Common.Constants;

    /// <summary>
    /// Strategy implementation for patient data source HdidEmpi.
    /// </summary>
    internal class PatientQueryHdidEmpi : IPatientQuery
    {
        /// <inheritdoc/>
        public async Task<PatientModel?> GetPatientAsync(PatientRequest request)
        {
            return await request.ClientRegistriesDelegate.GetDemographicsAsync(OidType.Hdid, request.Hdid, request.DisabledValidation).ConfigureAwait(true);
        }
    }
}
