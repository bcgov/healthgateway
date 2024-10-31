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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Service to fetch personal account information from PHSA.
    /// </summary>
    public class PersonalAccountsService : IPersonalAccountsService
    {
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
            this.phsaConfig = configuration.GetSection(PhsaConfigV2.ConfigurationSectionKey).Get<PhsaConfigV2>() ?? new();
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(PersonalAccountsService).FullName);

        /// <inheritdoc/>
        public async Task<PersonalAccount> GetPersonalAccountAsync(string hdid, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();
            activity?.AddBaggage("PatientAccountHdid", hdid);

            PersonalAccount? account = await this.GetFromCacheAsync(hdid, ct);
            if (account is not null)
            {
                return account;
            }

            this.logger.LogDebug("Retrieving personal account");
            account = await this.personalAccountsApi.AccountLookupByHdidAsync(hdid, ct);
            await this.PutCacheAsync(hdid, account, ct);

            return account;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PersonalAccount>> GetPersonalAccountResultAsync(string hdid, CancellationToken ct = default)
        {
            RequestResult<PersonalAccount> requestResult = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = 0,
            };

            try
            {
                requestResult.ResourcePayload = await this.GetPersonalAccountAsync(hdid, ct);
                requestResult.ResultStatus = ResultType.Success;
                requestResult.TotalResultCount = 1;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving personal account");
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with request for Personal Accounts",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return requestResult;
        }

        /// <summary>
        /// Caches a personal account (when cache is enabled).
        /// </summary>
        /// <param name="hdid">The HDID associated with the personal account.</param>
        /// <param name="personalAccount">The personal account to be cached.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task PutCacheAsync(string hdid, PersonalAccount personalAccount, CancellationToken ct)
        {
            if (this.phsaConfig.PersonalAccountsCacheTtl == 0)
            {
                return;
            }

            using Activity? activity = ActivitySource.StartActivity();
            this.logger.LogDebug("Storing personal account in cache");

            TimeSpan expiry = TimeSpan.FromMinutes(this.phsaConfig.PersonalAccountsCacheTtl);
            await this.cacheProvider.AddItemAsync($"{CacheDomain}:HDID:{hdid}", personalAccount, expiry, ct);
        }

        /// <summary>
        /// Attempts to retrieve a personal account from the cache.
        /// </summary>
        /// <param name="hdid">The associated HDID used to create the cache key.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The personal account, if it is found in the cache, or null, if it is not.</returns>
        private async Task<PersonalAccount?> GetFromCacheAsync(string hdid, CancellationToken ct)
        {
            if (this.phsaConfig.PersonalAccountsCacheTtl == 0)
            {
                return null;
            }

            using Activity? activity = ActivitySource.StartActivity();
            this.logger.LogDebug("Accessing personal account cache");

            PersonalAccount? cacheItem = await this.cacheProvider.GetItemAsync<PersonalAccount>($"{CacheDomain}:HDID:{hdid}", ct);

            this.logger.LogDebug("Personal account cache access outcome: {CacheResult}", cacheItem == null ? "miss" : "hit");
            return cacheItem;
        }
    }
}
