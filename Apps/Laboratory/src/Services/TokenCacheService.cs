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
namespace HealthGateway.Laboratory.Services
{
    using System;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Utility to lookup system tokens and cache them.
    /// </summary>
    public class TokenCacheService : ITokenCacheService
    {
        private const string AuthConfigSectionName = "ClientAuthentication";
        private const string TokenCacheKey = "TokenCacheKey";

        private readonly IConfiguration configuration;
        private readonly ILogger<TokenCacheService> logger;
        private readonly IMemoryCache memoryCache;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly Uri tokenUri;
        private readonly int tokenCacheMinutes;
        private readonly ClientCredentialsTokenRequest tokenRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCacheService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to lookup values.</param>
        /// <param name="logger">The callers logger.</param>
        /// <param name="memoryCache">The cache to use to reduce lookups.</param>
        /// <param name="authenticationDelegate">The authorization delegate.</param>
        public TokenCacheService(
            IConfiguration configuration,
            ILogger<TokenCacheService> logger,
            IMemoryCache memoryCache,
            IAuthenticationDelegate authenticationDelegate)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.memoryCache = memoryCache;
            this.authenticationDelegate = authenticationDelegate;

            IConfigurationSection? configSection = configuration?.GetSection(AuthConfigSectionName);
            this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");
            this.tokenCacheMinutes = configSection.GetValue<int>("TokenCacheExpireMinutes", 20);
            this.tokenRequest = new();
            configSection.Bind(this.tokenRequest); // Client ID, Client Secret, Audience, Username, Password
        }

        /// <summary>
        /// Utility to fetch an access token from a memory cache.
        /// </summary>
        /// <returns>The access token if found/authenticated.</returns>
        public string? RetrieveAccessToken()
        {
            this.memoryCache.TryGetValue(TokenCacheKey, out string? accessToken);
            if (accessToken == null)
            {
                this.logger.LogInformation("Access token not found in cache");
                accessToken = this.authenticationDelegate.AuthenticateAsUser(this.tokenUri, this.tokenRequest).AccessToken;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    this.logger.LogInformation("Attempting to store Access token in cache");
                    MemoryCacheEntryOptions cacheEntryOptions = new();
                    cacheEntryOptions.AbsoluteExpiration =
                        new DateTimeOffset(DateTime.Now.AddMinutes(this.tokenCacheMinutes));
                    this.memoryCache.Set(TokenCacheKey, accessToken, cacheEntryOptions);
                }
            }

            return accessToken;
        }
    }
}
