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
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Strategy implementation for patient data source HdidAll.
    /// </summary>
    internal class HdidAllStrategy : PatientQueryStrategy
    {
        private readonly IClientRegistriesDelegate clientRegistriesDelegate;
        private readonly IPatientIdentityApi patientIdentityApi;
        private readonly ILogger<HdidAllStrategy> logger;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HdidAllStrategy"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="cacheProvider">The injected cache provider.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="clientRegistriesDelegate">The injected client registries delegate.</param>
        /// <param name="patientIdentityApi">The injected patient identity api.</param>
        /// <param name="mapper">The injected mapper.</param>
        public HdidAllStrategy(
            IConfiguration configuration,
            ICacheProvider cacheProvider,
            IClientRegistriesDelegate clientRegistriesDelegate,
            IPatientIdentityApi patientIdentityApi,
            ILogger<HdidAllStrategy> logger,
            IMapper mapper)
            : base(configuration, cacheProvider, logger)
        {
            this.clientRegistriesDelegate = clientRegistriesDelegate;
            this.patientIdentityApi = patientIdentityApi;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public override async Task<PatientModel?> GetPatientAsync(PatientRequest request)
        {
            PatientModel? patient = (request.UseCache ? this.GetFromCache(request.Identifier, PatientIdentifierType.Hdid) : null) ??
                                    await this.clientRegistriesDelegate.GetDemographicsAsync(OidType.Hdid, request.Identifier, request.DisabledValidation).ConfigureAwait(true);

            if (patient == null)
            {
                try
                {
                    PatientIdentity result = await this.patientIdentityApi.GetPatientIdentityAsync(request.Identifier).ConfigureAwait(true);
                    patient = this.mapper.Map<PatientModel>(result);
                }
                catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogInformation("PHSA could not find patient identity for {Hdid}", request.Identifier);
                }
            }

            this.CachePatient(patient, request.DisabledValidation);
            return patient;
        }
    }
}
