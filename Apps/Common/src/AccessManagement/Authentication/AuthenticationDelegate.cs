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
namespace HealthGateway.Common.AccessManagement.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Authorization service.
    /// </summary>
    public class AuthenticationDelegate : IAuthenticationDelegate
    {
        private const string AuthConfigSectionName = "AuthCache";

        private readonly ILogger<IAuthenticationDelegate> logger;
        private readonly IHttpClientService httpClientService;
        private readonly IMemoryCache? memoryCache;
        private readonly int tokenCacheMinutes;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="memoryCache">The injected memory cache provider.</param>
        public AuthenticationDelegate(
            ILogger<IAuthenticationDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration? configuration,
            IMemoryCache? memoryCache)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            IConfigurationSection? configSection = configuration?.GetSection(AuthConfigSectionName);
            this.tokenCacheMinutes = configSection?.GetValue<int>("TokenCacheExpireMinutes", 0) ?? 0;
            this.memoryCache = memoryCache;
        }

        /// <inheritdoc/>
        public JWTModel AuthenticateAsSystem(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            this.logger.LogDebug($"Authenticating Service... {tokenRequest.ClientId}");
            JWTModel? jwtModel = this.ClientCredentialsGrant(tokenUri, tokenRequest);
            this.logger.LogDebug($"Finished authenticating Service. {tokenRequest.ClientId}");
            if (jwtModel == null)
            {
                this.logger.LogCritical($"Unable to authenticate to as {tokenRequest.Username} to {tokenUri}");
                throw new InvalidOperationException("Auth failure - JWTModel cannot be null");
            }

            return jwtModel;
        }

        /// <inheritdoc/>
        public JWTModel AuthenticateAsUser(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest, bool cacheEnabled = false)
        {
            (JWTModel jwtModel, _) = this.AuthenticateUser(tokenUri, tokenRequest, cacheEnabled);
            return jwtModel;
        }

            /// <inheritdoc/>
        public (JWTModel JwtModel, bool Cached) AuthenticateUser(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest, bool cacheEnabled)
        {
            string cacheKey = $"{tokenUri}:{tokenRequest.Audience}:{tokenRequest.ClientId}:{tokenRequest.Username}";
            bool cached = false;
            this.logger.LogDebug("Attempting to fetch token from cache");
            JWTModel? jwtModel = null;
            if (cacheEnabled && this.tokenCacheMinutes > 0)
            {
                this.memoryCache.TryGetValue(cacheKey, out jwtModel);
            }

            if (jwtModel != null)
            {
                this.logger.LogDebug("Auth token found in cache");
                cached = true;
            }
            else
            {
                this.logger.LogInformation($"JWT Model not found in cache - Authenticating Direct Grant as User: {tokenRequest.Username}");
                jwtModel = this.ResourceOwnerPasswordGrant(tokenUri, tokenRequest);
                if (jwtModel != null)
                {
                    if (cacheEnabled && this.tokenCacheMinutes > 0)
                    {
                        this.logger.LogDebug("Attempting to store Access token in cache");
                        MemoryCacheEntryOptions cacheEntryOptions = new();
                        cacheEntryOptions.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(this.tokenCacheMinutes));
                        this.memoryCache.Set(cacheKey, jwtModel, cacheEntryOptions);
                    }
                    else
                    {
                        this.logger.LogDebug("Caching is not configured or has been disabled");
                    }
                }
                else
                {
                    this.logger.LogCritical($"Unable to authenticate to as {tokenRequest.Username} to {tokenUri}");
                    throw new InvalidOperationException("Auth failure - JWTModel cannot be null");
                }

                this.logger.LogInformation($"Finished authenticating User: {tokenRequest.Username}");
            }

            return (jwtModel, cached);
        }

        private JWTModel? ClientCredentialsGrant(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            JWTModel? authModel = null;
            try
            {
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();

                IEnumerable<KeyValuePair<string?, string?>> oauthParams = new[]
                {
                    new KeyValuePair<string?, string?>(@"client_id", tokenRequest.ClientId),
                    new KeyValuePair<string?, string?>(@"client_secret", tokenRequest.ClientSecret),
                    new KeyValuePair<string?, string?>(@"audience", tokenRequest.Audience),
                    new KeyValuePair<string?, string?>(@"grant_type", @"client_credentials"),
                };
                using var content = new FormUrlEncodedContent(oauthParams);
                content.Headers.Clear();
                content.Headers.Add(@"Content-Type", @"application/x-www-form-urlencoded");

                using HttpResponseMessage response = client.PostAsync(tokenUri, content).Result;

                string jwtTokenResponse = response.Content.ReadAsStringAsync().Result;
                this.logger.LogTrace($"JWT Token response: {jwtTokenResponse}");
                response.EnsureSuccessStatusCode();
                authModel = JsonSerializer.Deserialize<JWTModel>(jwtTokenResponse)!;
            }
            catch (HttpRequestException e)
            {
                this.logger.LogError($"Error Message {e.Message}");
            }

            return authModel;
        }

        private JWTModel? ResourceOwnerPasswordGrant(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            JWTModel? authModel = null;
            try
            {
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();

                IEnumerable<KeyValuePair<string?, string?>> oauthParams = new[]
                {
                    new KeyValuePair<string?, string?>(@"client_id", tokenRequest.ClientId),
                    new KeyValuePair<string?, string?>(@"client_secret", tokenRequest.ClientSecret),
                    new KeyValuePair<string?, string?>(@"grant_type", @"password"),
                    new KeyValuePair<string?, string?>(@"audience", tokenRequest.Audience),
                    new KeyValuePair<string?, string?>(@"scope", tokenRequest.Scope),
                    new KeyValuePair<string?, string?>(@"username", tokenRequest.Username),
                    new KeyValuePair<string?, string?>(@"password", tokenRequest.Password),
                };

                using var content = new FormUrlEncodedContent(oauthParams);
                content.Headers.Clear();
                content.Headers.Add(@"Content-Type", @"application/x-www-form-urlencoded");

                using (Task<HttpResponseMessage> task = client.PostAsync(tokenUri, content))
                {
                        HttpResponseMessage response = task.Result;
                        string jwtTokenResponse = response.Content.ReadAsStringAsync().Result;
                        this.logger.LogTrace($"JWT Token response: {jwtTokenResponse}");
                        response.EnsureSuccessStatusCode();
                        authModel = JsonSerializer.Deserialize<JWTModel>(jwtTokenResponse);
                }
            }
            catch (HttpRequestException e)
            {
                this.logger.LogError($"Error Message {e.Message}");
            }

            return authModel;
        }
    }
}
