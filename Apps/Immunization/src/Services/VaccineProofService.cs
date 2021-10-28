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
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class VaccineProofService : IVaccineProofService
    {
        private const string BCMailPlusConfigSectionKey = "BCMailPlus";
        private readonly ILogger<VaccineProofService> logger;
        private readonly IVaccineProofDelegate vpDelegate;
        private readonly IVaccineProofRequestCacheDelegate cacheDelegate;
        private readonly BCMailPlusConfig bcmpConfig;
        private readonly bool cacheEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineProofService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="vaccineProofDelegate">The injected vaccine proof delegate.</param>
        /// <param name="cacheDelegate">The cache provider for vaccine proof requests.</param>
        public VaccineProofService(
                IConfiguration configuration,
                ILogger<VaccineProofService> logger,
                IVaccineProofDelegate vaccineProofDelegate,
                IVaccineProofRequestCacheDelegate cacheDelegate)
        {
            this.logger = logger;
            this.vpDelegate = vaccineProofDelegate;
            this.cacheDelegate = cacheDelegate;
            this.bcmpConfig = new();
            configuration.Bind(BCMailPlusConfigSectionKey, this.bcmpConfig);
            this.cacheEnabled = this.bcmpConfig.CacheTtl > 0;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> GetVaccineProof(string userIdentifier, VaccineProofRequest vaccineProofRequest, VaccineProofTemplate proofTemplate)
        {
            RequestResult<ReportModel> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            (RequestResult<VaccineProofResponse> proofGenerate, bool fromCache) = await this.CheckCache(userIdentifier, vaccineProofRequest, proofTemplate).ConfigureAwait(true);
            if (proofGenerate.ResultStatus == ResultType.Success && proofGenerate.ResourcePayload != null)
            {
                int retryCount = 0;
                RequestResult<ReportModel> assetResult = new()
                {
                    ResultStatus = ResultType.Error,
                };
                bool processing = true;
                while (processing && retryCount++ <= this.bcmpConfig.MaxRetries)
                {
                    // Skip delay on the first iteration if we fetched from cache.
                    if (!fromCache || (fromCache && retryCount > 1))
                    {
                        this.logger.LogInformation("Waiting to fetch Vaccine Proof Asset...");
                        await Task.Delay(this.bcmpConfig.BackOffMilliseconds).ConfigureAwait(true);
                    }

                    assetResult = await this.vpDelegate.GetAssetAsync(proofGenerate.ResourcePayload.AssetUri).ConfigureAwait(true);
                    processing = assetResult.ResultStatus == ResultType.ActionRequired;
                }

                if (assetResult.ResultStatus == ResultType.Success && assetResult.ResourcePayload != null)
                {
                    retVal.ResourcePayload = assetResult.ResourcePayload;
                    retVal.ResultStatus = ResultType.Success;
                }
                else
                {
                    retVal.ResultError = assetResult.ResultError ?? new RequestResultError() { ResultMessage = "Unable to obtain Vaccine Proof PDF", ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.BCMP) };
                }
            }
            else
            {
                retVal.ResultError = proofGenerate.ResultError;
            }

            return retVal;
        }

        /// <summary>
        /// Hashes a base64 string.
        /// </summary>
        /// <param name="base64String">The base64 string to hash.</param>
        /// <returns>A base64 encoded SHA512 Hash.</returns>
        private static string HashShcImage(string base64String)
        {
            using SHA512 shaM = new SHA512Managed();
            byte[] hash = shaM.ComputeHash(Convert.FromBase64String(base64String));
            string encodedHash = Convert.ToBase64String(hash);
            return encodedHash;
        }

        private async Task<(RequestResult<VaccineProofResponse> ProofResponse, bool FromCache)> CheckCache(string userIdentifier, VaccineProofRequest vaccineProofRequest, VaccineProofTemplate proofTemplate)
        {
            bool foundInCache = false;
            RequestResult<VaccineProofResponse> retVal = new();
            if (this.cacheEnabled)
            {
                string hashShc = HashShcImage(vaccineProofRequest.SmartHealthCardQr);
                VaccineProofRequestCache? cacheItem = this.cacheDelegate.GetCacheItem(userIdentifier, proofTemplate, hashShc);
                if (cacheItem != null)
                {
                    VaccineProofResponse proofResponse = new()
                    {
                        Id = cacheItem.VaccineProofResponseId!,
                        Status = VaccineProofRequestStatus.Unknown,
                        AssetUri = new Uri(cacheItem.AssetEndpoint),
                    };
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = proofResponse;
                    foundInCache = true;
                }
            }

            if (!foundInCache)
            {
                retVal = await this.vpDelegate.GenerateAsync(proofTemplate, vaccineProofRequest).ConfigureAwait(true);
                if (this.cacheEnabled && retVal.ResultStatus == ResultType.Success && retVal.ResourcePayload != null && retVal.ResourcePayload.AssetUri != null)
                {
                    VaccineProofRequestCache cacheItem = new()
                    {
                        PersonIdentifier = userIdentifier,
                        ProofTemplate = proofTemplate,
                        ShcImageHash = HashShcImage(vaccineProofRequest.SmartHealthCardQr),
                        ExpiryDateTime = DateTime.UtcNow.AddMinutes(this.bcmpConfig.CacheTtl),
                        VaccineProofResponseId = retVal.ResourcePayload.Id,
                        AssetEndpoint = retVal.ResourcePayload.AssetUri.ToString(),
                    };

                    this.cacheDelegate.AddCacheItem(cacheItem);
                }
            }

            return (retVal, foundInCache);
        }
    }
}
