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
namespace HealthGateway.Admin.Delegates
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization information.
    /// </summary>
    public class RestVaccineStatusDelegate : IVaccineStatusDelegate
    {
        private readonly ILogger logger;
        private readonly IImmunizationAdminApi immunizationAdminApi;
        private readonly PhsaConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestVaccineStatusDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="immunizationAdminApi">The injected Refit API for accessing vaccine statuses for admin users.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestVaccineStatusDelegate(
            ILogger<RestVaccineStatusDelegate> logger,
            IImmunizationAdminApi immunizationAdminApi,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.immunizationAdminApi = immunizationAdminApi;

            this.phsaConfig = new PhsaConfig();
            configuration.Bind(PhsaConfig.ConfigurationSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(RestVaccineStatusDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<VaccineStatusResult>>> GetVaccineStatusWithRetries(string phn, DateTime dateOfBirth, string accessToken)
        {
            using Activity? activity = Source.StartActivity();
            RequestResult<PhsaResult<VaccineStatusResult>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> response;
            int attemptCount = 0;
            bool refreshInProgress;
            do
            {
                response = await this.GetVaccineStatus(phn, dateOfBirth, accessToken).ConfigureAwait(true);

                refreshInProgress = response.ResultStatus == ResultType.Success &&
                                    response.ResourcePayload != null &&
                                    response.ResourcePayload.LoadState.RefreshInProgress;

                attemptCount++;
                if (refreshInProgress && attemptCount <= this.phsaConfig.MaxRetries)
                {
                    this.logger.LogDebug("Refresh in progress, trying again....");
                    await Task.Delay(Math.Max(response.ResourcePayload!.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds)).ConfigureAwait(true);
                }
            }
            while (refreshInProgress && attemptCount <= this.phsaConfig.MaxRetries);

            if (refreshInProgress)
            {
                this.logger.LogDebug("Maximum retry attempts reached.");
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Refresh in progress",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }
            else if (response.ResultStatus == ResultType.Success)
            {
                retVal = response;
            }
            else
            {
                retVal.ResultError = response.ResultError;
            }

            return retVal;
        }

        private async Task<RequestResult<PhsaResult<VaccineStatusResult>>> GetVaccineStatus(string phn, DateTime dateOfBirth, string accessToken)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting vaccine status for PHN {PersonalHealthNumber}, DoB {DateOfBirth}.", phn, dateOfBirth);

            RequestResult<PhsaResult<VaccineStatusResult>> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                VaccineStatusQuery query = new()
                {
                    PersonalHealthNumber = phn,
                    DateOfBirth = dateOfBirth,
                };

                PhsaResult<VaccineStatusResult> phsaResult = await this.immunizationAdminApi.GetVaccineStatus(query, accessToken).ConfigureAwait(true);

                if (phsaResult.Result != null)
                {
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = phsaResult;
                    retVal.TotalResultCount = 1;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogError("Unexpected exception in GetVaccineStatus {Exception}", e);
            }

            if (retVal.ResourcePayload == null)
            {
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error getting vaccine status",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            this.logger.LogDebug("Finished getting vaccine status for PHN {PersonalHealthNumber}", phn);
            return retVal;
        }
    }
}
