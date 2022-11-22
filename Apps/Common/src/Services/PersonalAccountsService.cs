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
namespace HealthGateway.Common.Services
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Server to fetch Personal Account information from PHSA.
    /// </summary>
    public class PersonalAccountsService : IPersonalAccountsService
    {
        /// <summary>
        /// The generic cache domain to store the Personal Account.
        /// </summary>
        private const string CacheDomain = "PersonalAccount";
        private readonly ICacheProvider cacheProvider;

        private readonly ILogger<PersonalAccountsService> logger;
        private readonly IPersonalAccountsApi personalAccountsApi;
        private readonly PhsaConfigV2 phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalAccountsService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="cacheProvider">The injected cache provider.</param>
        /// <param name="personalAccountsApi">The personal accounts api to use.</param>
        public PersonalAccountsService(
            ILogger<PersonalAccountsService> logger,
            IConfiguration configuration,
            ICacheProvider cacheProvider,
            IPersonalAccountsApi personalAccountsApi)
        {
            this.logger = logger;
            this.cacheProvider = cacheProvider;
            this.personalAccountsApi = personalAccountsApi;

            this.phsaConfig = new PhsaConfigV2();
            configuration.Bind(PhsaConfigV2.ConfigurationSectionKey, this.phsaConfig); // Initializes ClientId, ClientSecret, GrantType and Scope.
        }

        private static ActivitySource Source { get; } = new(nameof(PersonalAccountsService));

        /// <inheritdoc/>
        public async Task<RequestResult<PersonalAccount?>> GetPatientAccountAsync(string hdid)
        {
            RequestResult<PersonalAccount?> requestResult = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = 0,
            };
            PersonalAccount? account = this.GetFromCache(hdid);
            if (account is null)
            {
                try
                {
                    PersonalAccount response = await this.personalAccountsApi.AccountLookupByHdidAsync(hdid).ConfigureAwait(true);
                    account = response;
                    this.PutCache(hdid, account);
                }
                catch (Exception e) when (e is ApiException or HttpRequestException)
                {
                    this.logger.LogCritical("Request Exception {Error}", e.ToString());
                    requestResult.ResultError = new()
                    {
                        ResultMessage = "Error with request for Personal Accounts",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                }
            }

            if (account != null)
            {
                requestResult.ResultStatus = ResultType.Success;
                requestResult.ResourcePayload = account;
                requestResult.TotalResultCount = 1;
            }

            return requestResult;
        }

        /// <inheritdoc/>
        public RequestResult<PersonalAccount?> GetPatientAccount(string hdid)
        {
            return this.GetPatientAccountAsync(hdid).Result;
        }

        /// <summary>
        /// Caches the Personal Account if enabled.
        /// </summary>
        /// <param name="hdid">The hdid to use to retrieve the Personal Account.</param>
        /// <param name="personalAccount">The Personal Account to be cached.</param>
        private void PutCache(string hdid, PersonalAccount personalAccount)
        {
            using Activity? activity = Source.StartActivity();
            string cacheKey = $"{CacheDomain}:HDID:{hdid}";
            if (this.phsaConfig.PersonalAccountsCacheTtl > 0)
            {
                this.logger.LogDebug("Attempting to cache Personal Account for cache key: {Key}", cacheKey);
                this.cacheProvider.AddItem(cacheKey, personalAccount, TimeSpan.FromMinutes(this.phsaConfig.PersonalAccountsCacheTtl));
            }
            else
            {
                this.logger.LogDebug("PersonalAccount caching is disabled; will not cache for cache key: {Key}", cacheKey);
            }

            activity?.Stop();
        }

        /// <summary>
        /// Attempts to get the Personal Account from the cache.
        /// </summary>
        /// <param name="hdid">The hdid used to create the key to retrieve.</param>
        /// <returns>The Personal Account or null.</returns>
        private PersonalAccount? GetFromCache(string hdid)
        {
            using Activity? activity = Source.StartActivity();
            PersonalAccount? cacheItem = null;
            if (this.phsaConfig.PersonalAccountsCacheTtl > 0)
            {
                string cacheKey = $"{CacheDomain}:HDID:{hdid}";
                cacheItem = this.cacheProvider.GetItem<PersonalAccount>(cacheKey);
                this.logger.LogDebug("Cache key: {CacheKey} was {Found} found in cache.", cacheKey, cacheItem == null ? "not" : string.Empty);
            }

            activity?.Stop();
            return cacheItem;
        }
    }
}
