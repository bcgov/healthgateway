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
    using FluentValidation;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Strategy implementation for EMPI phn patient data source with or without cache.
    /// </summary>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="cacheProvider">The injected cache provider.</param>
    /// <param name="clientRegistriesDelegate">The injected client registries delegate.</param>
    /// <param name="logger">The injected logger.</param>
    internal class PhnEmpiStrategy(
        IConfiguration configuration,
        ICacheProvider cacheProvider,
        IClientRegistriesDelegate clientRegistriesDelegate,
        ILogger<PhnEmpiStrategy> logger) : PatientQueryStrategy(configuration, cacheProvider, logger)
    {
        /// <inheritdoc/>
        public override async Task<PatientModel> GetPatientAsync(PatientRequest request, CancellationToken ct = default)
        {
            await new PhnValidator(ErrorMessages.PhnInvalid).ValidateAndThrowAsync(request.Identifier, ct);

            PatientModel patient = (request.UseCache ? await this.GetFromCacheAsync(request.Identifier, PatientIdentifierType.Phn, ct) : null) ??
                                   await clientRegistriesDelegate.GetDemographicsAsync(OidType.Phn, request.Identifier, request.DisabledValidation, ct);

            await this.CachePatientAsync(patient, request.DisabledValidation, ct);
            return patient;
        }
    }
}
