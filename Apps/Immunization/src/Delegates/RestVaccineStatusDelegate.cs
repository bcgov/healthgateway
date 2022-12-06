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
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
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
        private readonly ILogger logger;
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

        private static ActivitySource Source { get; } = new(nameof(RestVaccineStatusDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<VaccineStatusResult>>> GetVaccineStatus(string hdid, bool includeFederalPvc, string accessToken)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting vaccine status for HDID {Hdid} with includeFederalPvc = {IncludeFederalPvc}.", hdid, includeFederalPvc);

            RequestResult<PhsaResult<VaccineStatusResult>> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                PhsaResult<VaccineStatusResult> phsaResult =
                    await this.immunizationApi.GetVaccineStatusAsync(hdid, includeFederalPvc, accessToken).ConfigureAwait(true);

                if (phsaResult.Result != null)
                {
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = phsaResult;
                    retVal.TotalResultCount = 1;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogError("Unexpected exception in GetVaccineStatusAsync {Exception}", e);
            }

            if (retVal.ResourcePayload == null)
            {
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error getting vaccine status",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            this.logger.LogDebug("Finished getting vaccine status for HDID {Hdid}", hdid);
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<VaccineStatusResult>>> GetVaccineStatusPublic(VaccineStatusQuery query, string accessToken, string clientIp)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug(
                "Getting vaccine status for PHN {PersonalHealthNumber}, DoB {DateOfBirth}, DoV {DateOfVaccine} with IncludeFederalVaccineProof = {IncludeFederalVaccineProof}.",
                query.PersonalHealthNumber,
                query.DateOfBirth,
                query.DateOfVaccine,
                query.IncludeFederalVaccineProof);

            RequestResult<PhsaResult<VaccineStatusResult>> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                PhsaResult<VaccineStatusResult> phsaResult = await this.immunizationPublicApi.GetVaccineStatusAsync(query, accessToken, clientIp).ConfigureAwait(true);

                if (phsaResult.Result != null)
                {
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = phsaResult;
                    retVal.TotalResultCount = 1;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogError("Unexpected exception in GetVaccineStatusPublic {Exception}", e);
            }

            if (retVal.ResourcePayload == null)
            {
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error getting vaccine status",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            this.logger.LogDebug("Finished getting vaccine status for PHN {PersonalHealthNumber}", query.PersonalHealthNumber);
            return retVal;
        }
    }
}
