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
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Strategy implementation for patient data source PhsaCache.
    /// </summary>
    internal class PatientHdidPhsaCacheStrategy : IPatientQueryStrategy
    {
        /// <inheritdoc/>
        public async Task<PatientModel?> GetPatientAsync(PatientRequest request)
        {
            PatientModel? patient = request.CachedPatient;

            if (patient == null)
            {
                try
                {
                    PatientIdentity result = await request.PatientIdentityApi.GetPatientIdentityAsync(request.Identifier).ConfigureAwait(true);
                    patient = request.Mapper.Map<PatientModel>(result);
                }
                catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    request.Logger.LogInformation("PHSA could not find patient identity for {Hdid}", request.Identifier);
                }
            }

            return patient;
        }
    }
}
