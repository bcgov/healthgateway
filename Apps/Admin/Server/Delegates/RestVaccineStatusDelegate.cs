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
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">Injected Logger Provider.</param>
    /// <param name="immunizationAdminApi">The injected Refit API for accessing vaccine statuses for admin users.</param>
    /// <param name="configuration">The injected configuration provider.</param>
    public class RestVaccineStatusDelegate(
        ILogger<RestVaccineStatusDelegate> logger,
        IImmunizationAdminApi immunizationAdminApi,
        IConfiguration configuration) : IVaccineStatusDelegate
    {
        private readonly PhsaConfig phsaConfig = configuration.GetSection(PhsaConfig.ConfigurationSectionKey).Get<PhsaConfig>() ?? new();

        private static ActivitySource ActivitySource { get; } = new(typeof(RestVaccineStatusDelegate).FullName);

        /// <inheritdoc/>
        [SuppressMessage("Style", "IDE0046:Simplify 'if' statement", Justification = "Team decision")]
        public async Task<PhsaResult<VaccineStatusResult>> GetVaccineStatusWithRetriesAsync(string phn, DateTime dateOfBirth, string accessToken, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = phn,
                DateOfBirth = dateOfBirth,
            };

            PhsaResult<VaccineStatusResult> response;

            int retryCount = 0;
            bool refreshInProgress;
            do
            {
                logger.LogDebug("Retrieving vaccine status");
                response = await immunizationAdminApi.GetVaccineStatusAsync(query, accessToken, ct);

                refreshInProgress = response.LoadState.RefreshInProgress;
                if (refreshInProgress)
                {
                    logger.LogDebug("Refresh is in progress");

                    retryCount++;
                    if (retryCount <= this.phsaConfig.MaxRetries)
                    {
                        await Task.Delay(Math.Max(response.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds), ct);
                    }
                }
            }
            while (refreshInProgress && retryCount <= this.phsaConfig.MaxRetries);

            if (refreshInProgress)
            {
                throw new UpstreamServiceException(ErrorMessages.MaximumRetryAttemptsReached) { ProblemType = ProblemType.MaxRetriesReached };
            }

            return response;
        }
    }
}
