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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Models.Immunization;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">Injected Logger Provider.</param>
    /// <param name="immunizationAdminApi">The injected client for immunization admin api calls.</param>
    /// <param name="configuration">The injected configuration provider.</param>
    /// <param name="mappingService">The injected mapping service.</param>
    public class RestImmunizationAdminDelegate(
        ILogger<RestImmunizationAdminDelegate> logger,
        IImmunizationAdminApi immunizationAdminApi,
        IConfiguration configuration,
        IAdminServerMappingService mappingService) : IImmunizationAdminDelegate
    {
        private readonly PhsaConfig phsaConfig = configuration.GetSection(PhsaConfig.ConfigurationSectionKey).Get<PhsaConfig>() ?? new();

        private static ActivitySource Source { get; } = new(nameof(RestImmunizationAdminDelegate));

        /// <inheritdoc/>
        public async Task<VaccineDetails> GetVaccineDetailsWithRetriesAsync(string phn, string accessToken, bool refresh = false, CancellationToken ct = default)
        {
            logger.LogDebug("Getting vaccine details with retries...");
            using Activity? activity = Source.StartActivity();

            CovidImmunizationsRequest request = new()
            {
                PersonalHealthNumber = phn,
                IgnoreCache = refresh,
            };

            PhsaResult<VaccineDetailsResponse> response;
            int retryCount = 0;
            bool refreshInProgress;
            do
            {
                response = await immunizationAdminApi.GetVaccineDetailsAsync(request, accessToken, ct);

                refreshInProgress = response.LoadState.RefreshInProgress;

                if (refreshInProgress)
                {
                    logger.LogDebug("Refresh in progress, trying again....");
                    await Task.Delay(Math.Max(response.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds), ct);
                }
            }
            while (refreshInProgress && retryCount++ < this.phsaConfig.MaxRetries);

            if (refreshInProgress)
            {
                throw new UpstreamServiceException(ErrorMessages.MaximumRetryAttemptsReached, ProblemType.MaxRetriesReached);
            }

            return new()
            {
                Blocked = response.Result?.Blocked ?? false,
                ContainsInvalidDoses = response.Result?.ContainsInvalidDoses ?? false,
                Doses = response.Result?.Doses?.Select(mappingService.MapToVaccineDose).ToArray() ?? [],
                VaccineStatusResult = response.Result?.VaccineStatusResult,
            };
        }
    }
}
