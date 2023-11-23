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
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models.PHSA;
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

        private static ActivitySource Source { get; } = new(nameof(RestVaccineStatusDelegate));

        /// <inheritdoc/>
        public async Task<PhsaResult<VaccineStatusResult>> GetVaccineStatusWithRetries(string phn, DateTime dateOfBirth, string accessToken)
        {
            using Activity? activity = Source.StartActivity();
            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = phn,
                DateOfBirth = dateOfBirth,
            };

            PhsaResult<VaccineStatusResult> response;

            int attemptCount = 0;
            bool refreshInProgress;
            do
            {
                response = await immunizationAdminApi.GetVaccineStatus(query, accessToken).ConfigureAwait(true);

                refreshInProgress = response.LoadState.RefreshInProgress;

                attemptCount++;
                if (refreshInProgress && attemptCount <= this.phsaConfig.MaxRetries)
                {
                    logger.LogDebug("Refresh in progress, trying again....");
                    await Task.Delay(Math.Max(response.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds)).ConfigureAwait(true);
                }
            }
            while (refreshInProgress && attemptCount <= this.phsaConfig.MaxRetries);

            if (refreshInProgress)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.MaximumRetryAttemptsReached, HttpStatusCode.BadRequest, nameof(RestVaccineStatusDelegate)));
            }

            return response;
        }
    }
}
