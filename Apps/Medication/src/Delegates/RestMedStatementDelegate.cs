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
namespace HealthGateway.Medication.Delegates
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.Cacheable;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Medication.Api;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// ODR Implementation for Rest Medication Statements.
    /// </summary>
    public class RestMedStatementDelegate : IMedStatementDelegate
    {
        private const string ProtectiveWordCacheDomain = "ProtectiveWord";
        private readonly ICacheProvider cacheProvider;
        private readonly IHashDelegate hashDelegate;
        private readonly IOdrApi odrApi;

        private readonly ILogger logger;
        private readonly OdrConfig odrConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedStatementDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="odrApi">The injected medication statement api.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="cacheProvider">The provider responsible for caching.</param>
        /// <param name="hashDelegate">The delegate responsible for hashing.</param>
        public RestMedStatementDelegate(
            ILogger<RestMedStatementDelegate> logger,
            IOdrApi odrApi,
            IConfiguration configuration,
            ICacheProvider cacheProvider,
            IHashDelegate hashDelegate)
        {
            this.logger = logger;
            this.odrApi = odrApi;
            this.cacheProvider = cacheProvider;
            this.hashDelegate = hashDelegate;
            this.odrConfig = new OdrConfig();
            configuration.Bind(OdrConfig.OdrConfigSectionKey, this.odrConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(RestMedStatementDelegate));

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catch all required")]
        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "Team decision")]
        public async Task<RequestResult<MedicationHistoryResponse>> GetMedicationStatementsAsync(OdrHistoryQuery query, string? protectiveWord, string hdid, string ipAddress)
        {
            using (Source.StartActivity())
            {
                RequestResult<MedicationHistoryResponse> retVal = new();
                try
                {
                    bool validProtectiveWord = await this.ValidateProtectiveWordAsync(query.Phn, protectiveWord, hdid, ipAddress).ConfigureAwait(true);
                    if (validProtectiveWord)
                    {
                        using (Source.StartActivity("ODRQuery"))
                        {
                            this.logger.LogTrace("Getting medication statements... {Phn}", query.Phn.Substring(0, 3));
                            MedicationHistory request = new()
                            {
                                Id = Guid.NewGuid(),
                                RequestorHdid = hdid,
                                RequestorIp = ipAddress,
                                Query = query,
                            };

                            MedicationHistory medicationHistory = await this.odrApi.GetMedicationHistoryAsync(request).ConfigureAwait(true);
                            retVal.ResultStatus = ResultType.Success;
                            retVal.ResourcePayload = medicationHistory.Response;
                            this.logger.LogDebug("Finished getting medication statements");
                        }
                    }
                    else
                    {
                        this.logger.LogInformation("Invalid protected word");
                        retVal.ResultStatus = ResultType.ActionRequired;
                        retVal.ResultError = ErrorTranslator.ActionRequired(ErrorMessages.ProtectiveWordErrorMessage, ActionType.Protected);
                    }
                }
                catch (Exception e) when (e is ApiException or HttpRequestException)
                {
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = "An error occured with the Medication History Provider",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.OdrRecords),
                    };
                    this.logger.LogError("Error with Medication Service: {Exception}", e.ToString());
                }

                return retVal;
            }
        }

        /// <inheritdoc/>
        public Task<bool> SetProtectiveWordAsync(string phn, string newProtectiveWord, string protectiveWord, string hdid, string ipAddress)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> DeleteProtectiveWordAsync(string phn, string protectiveWord, string hdid, string ipAddress)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "Team decision")]
        public async Task<bool> ValidateProtectiveWordAsync(string phn, string? protectiveWord, string hdid, string ipAddress)
        {
            using Activity? activity = Source.StartActivity();
            bool cachingEnabled = this.odrConfig.CacheTtl > 0;

            IHash? protectiveWordHash = null;
            if (cachingEnabled)
            {
                this.logger.LogDebug("Attempting to fetch Protective Word from Cache");
                using Activity? startActivity = Source.StartActivity("GetCacheObject");

                protectiveWordHash = await this.cacheProvider.GetOrSetAsync(
                        $"{ProtectiveWordCacheDomain}:{hdid}",
                        () => this.GetProtectiveWordHash(phn, hdid, ipAddress),
                        TimeSpan.FromMinutes(this.odrConfig.CacheTtl))
                    .ConfigureAwait(true);
            }
            else
            {
                protectiveWordHash = await this.GetProtectiveWordHash(phn, hdid, ipAddress).ConfigureAwait(true);
            }

            this.logger.LogDebug("Validating Protective Word");
            bool isValid = this.hashDelegate.Compare(protectiveWord, protectiveWordHash);
            return isValid;
        }

        /// <summary>
        /// Returns the hashed protective word.
        /// </summary>
        /// <param name="phn">The PHN to query.</param>
        /// <param name="hdid">The HDID of the user querying.</param>
        /// <param name="ipAddress">The IP of the user querying.</param>
        /// <returns>The hash of the protective word response or null if not set.</returns>
        private async Task<HmacHash> GetProtectiveWordHash(string phn, string hdid, string ipAddress)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogTrace("Getting Protective word for {Phn}", phn.Substring(0, 3));
            ProtectiveWord request = new()
            {
                Id = Guid.NewGuid(),
                RequestorHdid = hdid,
                RequestorIp = ipAddress,
                QueryResponse = new ProtectiveWordQueryResponse
                {
                    Phn = phn,
                    Operator = ProtectiveWordOperator.Get,
                },
            };

            ProtectiveWord protectiveWord = await this.odrApi.GetProtectiveWordAsync(request).ConfigureAwait(true);
            HmacHash retVal = (HmacHash)this.hashDelegate.Hash(protectiveWord.QueryResponse.Value);
            this.logger.LogDebug("Finished getting Protective Word {Phn}", phn.Substring(0, 3));
            return retVal;
        }
    }
}
