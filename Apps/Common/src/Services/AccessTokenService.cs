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
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class AccessTokenService : IAccessTokenService
    {
        /// <summary>
        /// The generic cache domain to store access token against.
        /// </summary>
        private const string TokenSwapCacheDomain = "TokenSwap";

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

            this.phsaConfigV2 = new PhsaConfigV2();
            configuration.Bind(PhsaConfigV2.ConfigurationSectionKey, this.phsaConfigV2); // Initializes ClientId, ClientSecret, GrantType and Scope.
        }

        private static ActivitySource Source { get; } = new(nameof(AccessTokenService));

        /// <inheritdoc/>
        public async Task<RequestResult<TokenSwapResponse>> GetPhsaAccessToken()
        {
            using Activity? activity = Source.StartActivity();
            string? hdid = this.authenticationDelegate.FetchAuthenticatedUserHdid();
            RequestResult<TokenSwapResponse> requestResult = new();
            TokenSwapResponse? cachedAccessToken = this.GetFromCache(hdid);

            if (cachedAccessToken == null)
            {
                string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

                if (accessToken != null)
                {
                    requestResult = await this.SwapToken(hdid, accessToken).ConfigureAwait(true);
                }
                else
                {
                    this.logger.LogError("Unable to get authenticated user token from context for hdid: {Hdid}", hdid);
                    requestResult.ResultStatus = ResultType.Error;
                    requestResult.ResultError = new()
                    {
                        ResultMessage = $"Internal Error: Unable to get authenticated user token from context for hdid: {hdid}",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    };
                }
            }
            else
            {
                this.logger.LogDebug("Access token fetched from Cache");
                requestResult.ResourcePayload = cachedAccessToken;
                requestResult.ResultStatus = ResultType.Success;
            }

            return requestResult;
        }

        /// <summary>
        /// Caches the Access Token if enabled.
        /// </summary>
        /// <param name="hdid">The hdid associated with the token swap response to be cached.</param>
        /// <param name="tokenSwapResponse">The token swap response to be cached.</param>
        private void CacheAccessToken(string hdid, TokenSwapResponse tokenSwapResponse)
        {
            using Activity? activity = Source.StartActivity();
            string cacheKey = $"{TokenSwapCacheDomain}:HDID:{hdid}";
            if (this.phsaConfigV2.TokenCacheEnabled)
            {
                this.logger.LogDebug("Attempting to cache access token for cache key: {Key}", cacheKey);
                TimeSpan? expires = tokenSwapResponse.ExpiresIn > 0 ? TimeSpan.FromSeconds(tokenSwapResponse.ExpiresIn) : null;
                this.cacheProvider.AddItem(cacheKey, tokenSwapResponse, expires);
            }
            else
            {
                this.logger.LogDebug("Access token caching is disabled; will not cache for cache key: {Key}", cacheKey);
            }

            activity?.Stop();
        }

        /// <summary>
        /// Attempts to get the token response from cache.
        /// </summary>
        /// <param name="hdid">The hdid used to create the key to retrieve the token response from cache.</param>
        /// <returns>The found token response or null.</returns>
        private TokenSwapResponse? GetFromCache(string hdid)
        {
            using Activity? activity = Source.StartActivity();
            TokenSwapResponse? tokenResponse = null;
            if (this.phsaConfigV2.TokenCacheEnabled)
            {
                string cacheKey = $"{TokenSwapCacheDomain}:HDID:{hdid}";
                tokenResponse = this.cacheProvider.GetItem<TokenSwapResponse>(cacheKey);
                this.logger.LogDebug("Cache key: {CacheKey} was {Found} found in cache.", cacheKey, tokenResponse == null ? "not" : string.Empty);
            }

            activity?.Stop();
            return tokenResponse;
        }

        /// <summary>
        /// Attempts to get the token response from cache.
        /// </summary>
        /// <param name="hdid">The hdid used to create the key to use to cache the token response.</param>
        /// <param name="accessToken">The access token to swap for a new token.</param>
        /// <returns>The token response wrapped in a request result.</returns>
        private async Task<RequestResult<TokenSwapResponse>> SwapToken(string hdid, string accessToken)
        {
            RequestResult<TokenSwapResponse> requestResult = await this.tokenSwapDelegate.SwapToken(accessToken).ConfigureAwait(true);

            if (requestResult.ResultStatus == ResultType.Success)
            {
                this.CacheAccessToken(hdid, requestResult.ResourcePayload);
            }

            return requestResult;
        }
    }
}
