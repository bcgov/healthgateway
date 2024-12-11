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
namespace HealthGateway.Immunization.Delegates
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Api;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization information.
    /// </summary>
    public class RestVaccineStatusDelegate : IVaccineStatusDelegate
    {
        private readonly ILogger<RestVaccineStatusDelegate> logger;
        private readonly IImmunizationApi immunizationApi;
        private readonly IImmunizationPublicApi immunizationPublicApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestVaccineStatusDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="immunizationApi">The injected Refit API for accessing vaccine statuses for authenticated users.</param>
        /// <param name="immunizationPublicApi">The injected Refit API for accessing vaccine statuses for public users.</param>
        public RestVaccineStatusDelegate(ILogger<RestVaccineStatusDelegate> logger, IImmunizationApi immunizationApi, IImmunizationPublicApi immunizationPublicApi)
        {
            this.logger = logger;
            this.immunizationApi = immunizationApi;
            this.immunizationPublicApi = immunizationPublicApi;
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(RestVaccineStatusDelegate).FullName);

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<VaccineStatusResult>>> GetVaccineStatusAsync(string hdid, bool includeFederalPvc, string accessToken, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();
            activity?.AddBaggage("VaccineStatusHdid", hdid);
            activity?.AddBaggage("VaccineStatusIncludeFederalPvc", includeFederalPvc.ToString());

            RequestResult<PhsaResult<VaccineStatusResult>> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                this.logger.LogDebug("Retrieving vaccine status (authenticated)");
                PhsaResult<VaccineStatusResult> phsaResult = await this.immunizationApi.GetVaccineStatusAsync(hdid, includeFederalPvc, accessToken, ct);

                if (phsaResult.Result != null)
                {
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = phsaResult;
                    retVal.TotalResultCount = 1;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving vaccine status (authenticated)");
            }

            if (retVal.ResourcePayload == null)
            {
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error getting vaccine status",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<VaccineStatusResult>>> GetVaccineStatusPublicAsync(VaccineStatusQuery query, string accessToken, string clientIp, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();
            activity?.AddBaggage("VaccineStatusPhn", query.PersonalHealthNumber);
            activity?.AddBaggage("VaccineStatusIncludeFederalVaccineProof", query.IncludeFederalVaccineProof.ToString());

            RequestResult<PhsaResult<VaccineStatusResult>> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                this.logger.LogDebug("Retrieving vaccine status (public)");
                PhsaResult<VaccineStatusResult> phsaResult = await this.immunizationPublicApi.GetVaccineStatusAsync(query, accessToken, clientIp, ct);

                if (phsaResult.Result != null)
                {
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = phsaResult;
                    retVal.TotalResultCount = 1;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving vaccine status (public)");
            }

            if (retVal.ResourcePayload == null)
            {
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error getting vaccine status",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return retVal;
        }
    }
}
