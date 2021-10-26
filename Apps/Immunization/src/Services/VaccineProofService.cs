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
namespace HealthGateway.Immunization.Services
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class VaccineProofService : IVaccineProofService
    {
        private const string BCMailPlusConfigSectionKey = "BCMailPlus";
        private readonly ILogger<VaccineProofService> logger;
        private readonly IVaccineProofDelegate vpDelegate;
        private readonly BCMailPlusConfig bcmpConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineProofService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="vaccineProofDelegate">The injected vaccine proof delegate.</param>
        public VaccineProofService(
                IConfiguration configuration,
                ILogger<VaccineProofService> logger,
                IVaccineProofDelegate vaccineProofDelegate)
        {
            this.logger = logger;
            this.vpDelegate = vaccineProofDelegate;
            this.bcmpConfig = new();
            configuration.Bind(BCMailPlusConfigSectionKey, this.bcmpConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> GetVaccineProof(string userIdentifier, VaccineProofRequest vaccineProofRequest, VaccineProofTemplate proofTemplate)
        {
            RequestResult<ReportModel> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            RequestResult<VaccineProofResponse> proofGenerate = await this.vpDelegate.GenerateAsync(proofTemplate, vaccineProofRequest).ConfigureAwait(true);
            if (proofGenerate.ResultStatus == ResultType.Success && proofGenerate.ResourcePayload != null)
            {
                RequestResult<VaccineProofResponse> proofStatus;
                bool processing;
                int retryCount = 0;
                do
                {
                    proofStatus = await this.vpDelegate.GetStatusAsync(proofGenerate.ResourcePayload.Id).ConfigureAwait(true);

                    processing = proofStatus.ResultStatus == ResultType.Success &&
                                 proofStatus.ResourcePayload != null &&
                                 proofStatus.ResourcePayload.Status == VaccineProofRequestStatus.Started;
                    if (processing)
                    {
                        this.logger.LogInformation("Waiting to poll Vaccine Proof Status again");
                        await Task.Delay(this.bcmpConfig.BackOffMilliseconds).ConfigureAwait(true);
                    }
                }
                while (processing && retryCount++ < this.bcmpConfig.MaxRetries);
                if (proofStatus.ResultStatus == ResultType.Success && proofStatus.ResourcePayload?.Status == VaccineProofRequestStatus.Completed)
                {
                    // Get the Asset
                    RequestResult<ReportModel> assetResult = await this.vpDelegate.GetAssetAsync(proofGenerate.ResourcePayload.Id).ConfigureAwait(true);
                    if (assetResult.ResultStatus == ResultType.Success && assetResult.ResourcePayload != null)
                    {
                        retVal.ResourcePayload = assetResult.ResourcePayload;
                        retVal.ResultStatus = ResultType.Success;
                    }
                    else
                    {
                        retVal.ResultError = assetResult.ResultError;
                    }
                }
                else
                {
                    retVal.ResultError = proofStatus.ResultError ?? new RequestResultError() { ResultMessage = "Unable to obtain Vaccine Proof PDF", ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.BCMP) };
                }
            }
            else
            {
                retVal.ResultError = proofGenerate.ResultError;
            }

            return retVal;
        }
    }
}
