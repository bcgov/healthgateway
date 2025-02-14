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
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Strategy implementation for PHSA phn patient data source with or without cache.
    /// </summary>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="cacheProvider">The injected cache provider.</param>
    /// <param name="patientIdentityApi">The injected patient identity api. </param>
    /// <param name="logger">The injected logger.</param>
    /// <param name="mapper">The injected mapper.</param>
    internal class HdidPhsaStrategy(
        IConfiguration configuration,
        ICacheProvider cacheProvider,
        IPatientIdentityApi patientIdentityApi,
        ILogger<HdidPhsaStrategy> logger,
        IMapper mapper) : PatientQueryStrategy(configuration, cacheProvider, logger)
    {
        /// <inheritdoc/>
        public override async Task<PatientModel> GetPatientAsync(PatientRequest request, CancellationToken ct = default)
        {
            PatientModel? patient = request.UseCache ? await this.GetFromCacheAsync(request.Identifier, PatientIdentifierType.Hdid, ct) : null;

            if (patient == null)
            {
                try
                {
                    this.GetLogger().LogDebug("Retrieving patient from PHSA");
                    PatientIdentity result = await patientIdentityApi.GetPatientIdentityAsync(request.Identifier, ct);
                    patient = mapper.Map<PatientModel>(result);
                }
                catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new NotFoundException(ErrorMessages.PatientIdentityNotFound);
                }
            }

            await this.CachePatientAsync(patient, request.DisabledValidation, ct);
            return patient;
        }
    }
}
