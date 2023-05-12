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
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Strategy implementation for patient data source HdidEmpi.
    /// </summary>
    internal class HdidEmpiStrategy : PatientQueryStrategy
    {
        private readonly IClientRegistriesDelegate clientRegistriesDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HdidEmpiStrategy"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="cacheProvider">The injected cache provider.</param>
        /// <param name="clientRegistriesDelegate">The injected client registries delegate.</param>
        /// <param name="logger">The injected logger.</param>
        public HdidEmpiStrategy(
            IConfiguration configuration,
            ICacheProvider cacheProvider,
            IClientRegistriesDelegate clientRegistriesDelegate,
            ILogger<HdidEmpiStrategy> logger)
            : base(configuration, cacheProvider, logger)
        {
            this.clientRegistriesDelegate = clientRegistriesDelegate;
        }

        /// <inheritdoc/>
        public override async Task<PatientModel?> GetPatientAsync(PatientRequest request)
        {
            PatientModel? patient = (request.UseCache ? this.GetFromCache(request.Identifier, PatientIdentifierType.Hdid) : null) ??
                                    await this.clientRegistriesDelegate.GetDemographicsAsync(OidType.Hdid, request.Identifier, request.DisabledValidation).ConfigureAwait(true);

            this.CachePatient(patient);
            return patient;
        }
    }
}
