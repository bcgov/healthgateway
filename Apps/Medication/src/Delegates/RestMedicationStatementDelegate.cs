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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
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
    public class RestMedicationStatementDelegate : IMedicationStatementDelegate
    {
        private const string ProtectiveWordCacheDomain = "ProtectiveWord";
        private readonly ICacheProvider cacheProvider;
        private readonly IHashDelegate hashDelegate;
        private readonly IOdrApi odrApi;
        private readonly ILogger<RestMedicationStatementDelegate> logger;
        private readonly OdrConfig odrConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationStatementDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="odrApi">The injected medication statement api.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="cacheProvider">The provider responsible for caching.</param>
        /// <param name="hashDelegate">The delegate responsible for hashing.</param>
        public RestMedicationStatementDelegate(
            ILogger<RestMedicationStatementDelegate> logger,
            IOdrApi odrApi,
            IConfiguration configuration,
            ICacheProvider cacheProvider,
            IHashDelegate hashDelegate)
        {
            this.logger = logger;
            this.odrApi = odrApi;
            this.cacheProvider = cacheProvider;
            this.hashDelegate = hashDelegate;
            this.odrConfig = configuration.GetSection(OdrConfig.OdrConfigSectionKey).Get<OdrConfig>() ?? new();
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(RestMedicationStatementDelegate).FullName);

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catch all required")]
        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "Team decision")]
        public async Task<RequestResult<MedicationHistoryResponse>> GetMedicationStatementsAsync(
            OdrHistoryQuery query,
            string? protectiveWord,
            string hdid,
            string ipAddress,
            CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            RequestResult<MedicationHistoryResponse> retVal = new();

            try
            {
                bool validProtectiveWord = await this.ValidateProtectiveWordAsync(query.Phn, protectiveWord, hdid, ipAddress, ct);
                if (validProtectiveWord)
                {
                    using Activity? requestActivity = ActivitySource.StartActivity("GetMedicationStatementsAsyncRequest");

                    MedicationHistory request = new()
                    {
                        Id = Guid.NewGuid(),
                        RequestorHdid = hdid,
                        RequestorIp = ipAddress,
                        Query = query,
                    };

                    this.logger.LogDebug("Retrieving medication statements");
                    MedicationHistory medicationHistory = await this.odrApi.GetMedicationHistoryAsync(request, ct);

                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = medicationHistory.Response;
                }
                else
                {
                    this.logger.LogDebug("Protective word did not match");

                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired(ErrorMessages.ProtectiveWordErrorMessage, ActionType.Protected);
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving medication statements");

                retVal.ResultStatus = ResultType.Error;
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "An error occured with the Medication History Provider",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.OdrRecords),
                };
            }

            return retVal;
        }

        /// <summary>
        /// Validates the protective word.
        /// </summary>
        /// <param name="phn">The PHN to query.</param>
        /// <param name="protectiveWord">The protective word to validate.</param>
        /// <param name="hdid">The HDID of the user querying.</param>
        /// <param name="ipAddress">The IP of the user querying.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>True if the supplied protective word matches the user's stored protective word.</returns>
        private async Task<bool> ValidateProtectiveWordAsync(string phn, string? protectiveWord, string hdid, string ipAddress, CancellationToken ct = default)
        {
            IHash? hash = null;

            if (this.odrConfig.CacheTtl > 0)
            {
                this.logger.LogDebug("Accessing protective word cache");
                hash = await this.cacheProvider.GetItemAsync<HmacHash>($"{ProtectiveWordCacheDomain}:{hdid}", ct);
                this.logger.LogDebug("Protective word cache access outcome: {CacheResult}", hash == null ? "miss" : "hit");
            }

            if (hash == null)
            {
                hash = await this.GetProtectiveWordAsync(phn, hdid, ipAddress, ct);

                if (this.odrConfig.CacheTtl > 0)
                {
                    this.logger.LogDebug("Storing protective word in cache");
                    await this.cacheProvider.AddItemAsync($"{ProtectiveWordCacheDomain}:{hdid}", hash, TimeSpan.FromMinutes(this.odrConfig.CacheTtl), ct);
                }
            }

            return this.hashDelegate.Compare(protectiveWord, hash);
        }

        /// <summary>
        /// Returns the hashed protective word.
        /// </summary>
        /// <param name="phn">The PHN to query.</param>
        /// <param name="hdid">The HDID of the user querying.</param>
        /// <param name="ipAddress">The IP of the user querying.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The hash of the protective word response or null if not set.</returns>
        private async Task<IHash> GetProtectiveWordAsync(string phn, string hdid, string ipAddress, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

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

            this.logger.LogDebug("Retrieving protective word");
            ProtectiveWord protectiveWord = await this.odrApi.GetProtectiveWordAsync(request, ct);

            return this.hashDelegate.Hash(protectiveWord.QueryResponse.Value);
        }
    }
}
