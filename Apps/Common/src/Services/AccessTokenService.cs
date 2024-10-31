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
namespace HealthGateway.Common.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class AccessTokenService : IAccessTokenService
    {
        private const string TokenSwapCacheDomain = "TokenSwap";
        private const int EarlyExpiry = 45;

        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly ICacheProvider cacheProvider;
        private readonly ILogger<AccessTokenService> logger;
        private readonly PhsaConfigV2 phsaConfigV2;
        private readonly ITokenSwapDelegate tokenSwapDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenService"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="tokenSwapDelegate">The delegate responsible for swapping tokens with PHSA.</param>
        /// <param name="cacheProvider">The cache provider responsible for caching.</param>
        /// <param name="authenticationDelegate">The delegate responsible for fetching tokens.</param>
        /// <param name="configuration">The configuration to use.</param>
        public AccessTokenService(
            ILogger<AccessTokenService> logger,
            ITokenSwapDelegate tokenSwapDelegate,
            ICacheProvider cacheProvider,
            IAuthenticationDelegate authenticationDelegate,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.tokenSwapDelegate = tokenSwapDelegate;
            this.cacheProvider = cacheProvider;
            this.authenticationDelegate = authenticationDelegate;
            this.phsaConfigV2 = configuration.GetSection(PhsaConfigV2.ConfigurationSectionKey).Get<PhsaConfigV2>() ?? new();
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(AccessTokenService).FullName);

        /// <inheritdoc/>
        public async Task<RequestResult<TokenSwapResponse>> GetPhsaAccessTokenAsync(CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            this.logger.LogDebug("Retrieving PHSA access token");

            RequestResult<TokenSwapResponse> requestResult = new();

            string? userId = this.authenticationDelegate.FetchAuthenticatedUserId();
            string cacheKey = $"{TokenSwapCacheDomain}:{userId}";
            TokenSwapResponse? cachedAccessToken = await this.GetFromCacheAsync(cacheKey, ct);

            if (cachedAccessToken == null)
            {
                string? accessToken = await this.authenticationDelegate.FetchAuthenticatedUserTokenAsync(ct);
                if (accessToken != null)
                {
                    requestResult = await this.SwapTokenAsync(cacheKey, accessToken, ct);
                }
                else
                {
                    this.logger.LogError("Unable to get authenticated user token from context");
                    requestResult.ResultStatus = ResultType.Error;
                    requestResult.ResultError = new()
                    {
                        ResultMessage = $"Internal Error: Unable to get authenticated user token from context for: {cacheKey}",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    };
                }
            }
            else
            {
                requestResult.ResourcePayload = cachedAccessToken;
                requestResult.ResultStatus = ResultType.Success;
            }

            return requestResult;
        }

        /// <summary>
        /// Caches the Access Token if enabled.
        /// </summary>
        /// <param name="cacheKey">The key to retrieve the token response from cache.</param>
        /// <param name="tokenSwapResponse">The token swap response to be cached.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        private async Task CacheAccessTokenAsync(string cacheKey, TokenSwapResponse tokenSwapResponse, CancellationToken ct)
        {
            using Activity? activity = ActivitySource.StartActivity();

            if (this.phsaConfigV2.TokenCacheEnabled)
            {
                this.logger.LogDebug("Storing token swap response in cache");
                TimeSpan? expires = tokenSwapResponse.ExpiresIn > 0 ? TimeSpan.FromSeconds(tokenSwapResponse.ExpiresIn - EarlyExpiry) : null;
                await this.cacheProvider.AddItemAsync(cacheKey, tokenSwapResponse, expires, ct);
            }
        }

        /// <summary>
        /// Attempts to get the token response from cache.
        /// </summary>
        /// <param name="cacheKey">The key to retrieve the token response from cache.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The found token response or null.</returns>
        private async Task<TokenSwapResponse?> GetFromCacheAsync(string cacheKey, CancellationToken ct)
        {
            using Activity? activity = ActivitySource.StartActivity();

            TokenSwapResponse? tokenResponse = null;

            if (this.phsaConfigV2.TokenCacheEnabled)
            {
                this.logger.LogDebug("Accessing token swap cache");
                tokenResponse = await this.cacheProvider.GetItemAsync<TokenSwapResponse>(cacheKey, ct);
                this.logger.LogDebug("Token swap cache access outcome: {CacheResult}", tokenResponse == null ? "miss" : "hit");
            }

            return tokenResponse;
        }

        /// <summary>
        /// Attempts to get the token response from cache.
        /// </summary>
        /// <param name="cacheKey">The key to retrieve the token response from cache.</param>
        /// <param name="accessToken">The access token to swap for a new token.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The token response wrapped in a request result.</returns>
        private async Task<RequestResult<TokenSwapResponse>> SwapTokenAsync(string cacheKey, string accessToken, CancellationToken ct)
        {
            RequestResult<TokenSwapResponse> requestResult = await this.tokenSwapDelegate.SwapTokenAsync(accessToken, ct);

            if (requestResult.ResultStatus == ResultType.Success)
            {
                await this.CacheAccessTokenAsync(cacheKey, requestResult.ResourcePayload, ct);
            }

            return requestResult;
        }
    }
}
