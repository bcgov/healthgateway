//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Admin.Server.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class RestImmunizationAdminDelegate : IImmunizationAdminDelegate
    {
        private readonly IMapper autoMapper;
        private readonly IImmunizationAdminApi immunizationAdminApi;
        private readonly ILogger logger;
        private readonly PhsaConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestImmunizationAdminDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="immunizationAdminApi">The injected client for immunization admin api calls.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public RestImmunizationAdminDelegate(
            ILogger<RestImmunizationAdminDelegate> logger,
            IImmunizationAdminApi immunizationAdminApi,
            IConfiguration configuration,
            IMapper autoMapper)
        {
            this.logger = logger;
            this.immunizationAdminApi = immunizationAdminApi;
            this.autoMapper = autoMapper;
            this.phsaConfig = new PhsaConfig();
            configuration.Bind(PhsaConfig.ConfigurationSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(RestImmunizationAdminDelegate));

        /// <inheritdoc/>
        public async Task<VaccineDetails> GetVaccineDetailsWithRetries(PatientModel patient, string accessToken, bool refresh = false)
        {
            this.logger.LogDebug("Getting vaccine details with retries...");
            using Activity? activity = Source.StartActivity();

            CovidImmunizationsRequest request = new()
            {
                PersonalHealthNumber = patient.Phn,
                IgnoreCache = refresh,
            };

            PhsaResult<VaccineDetailsResponse> response;
            int retryCount = 0;
            bool refreshInProgress;
            do
            {
                response = await this.immunizationAdminApi.GetVaccineDetails(request, accessToken).ConfigureAwait(true);

                refreshInProgress = response.LoadState.RefreshInProgress;

                if (refreshInProgress)
                {
                    this.logger.LogDebug("Refresh in progress, trying again....");
                    await Task.Delay(Math.Max(response.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds)).ConfigureAwait(true);
                }
            }
            while (refreshInProgress && retryCount++ < this.phsaConfig.MaxRetries);

            return new VaccineDetails(
                this.autoMapper.Map<IEnumerable<VaccineDoseResponse>, IList<VaccineDose>>(response.Result?.Doses),
                response.Result?.VaccineStatusResult)
            {
                Blocked = response.Result?.Blocked ?? false,
                ContainsInvalidDoses = response.Result?.ContainsInvalidDoses ?? false,
            };
        }
    }
}
