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
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.CacheProviders;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Authorization service.
    /// </summary>
    public class AuthenticationDelegate : IAuthenticationDelegate
    {
        private const string CacheConfigSectionName = "AuthCache";
        private readonly ICacheProvider cacheProvider;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IHttpContextAccessor? httpContextAccessor;

        private readonly ILogger<IAuthenticationDelegate> logger;
        private readonly int tokenCacheMinutes;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientFactory">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="cacheProvider">The injected cache provider.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        public AuthenticationDelegate(
            ILogger<AuthenticationDelegate> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ICacheProvider cacheProvider,
            IHttpContextAccessor? httpContextAccessor)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.cacheProvider = cacheProvider;
            this.httpContextAccessor = httpContextAccessor;

            IConfigurationSection configSection = configuration.GetSection(CacheConfigSectionName);
            this.tokenCacheMinutes = configSection.GetValue("TokenCacheExpireMinutes", 0);
        }

        /// <inheritdoc/>
        public JwtModel AuthenticateAsSystem(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest, bool cacheEnabled = true)
        {
            JwtModel? jwtModel;
            string cacheKey = $"{tokenUri}:{tokenRequest.Audience}:{tokenRequest.ClientId}";
            if (cacheEnabled)
            {
                jwtModel = this.cacheProvider.GetItem<JwtModel>(cacheKey);
                if (jwtModel is null)
                {
                    jwtModel = this.GetSystemToken(tokenUri, tokenRequest);
                    if (jwtModel.ExpiresIn is not null)
                    {
                        int expiry = jwtModel.ExpiresIn.Value - 10;
                        this.cacheProvider.AddItem(cacheKey, jwtModel, TimeSpan.FromSeconds(expiry));
                    }
                }
            }
            else
            {
                jwtModel = this.GetSystemToken(tokenUri, tokenRequest);
            }

            this.logger.LogDebug("Authenticating Service... {ClientId}", tokenRequest.ClientId);
            return jwtModel;
        }

        /// <inheritdoc/>
        public string? AccessTokenAsUser(string sectionName = IAuthenticationDelegate.DefaultAuthConfigSectionName)
        {
            (Uri tUri, ClientCredentialsTokenRequest tRequest) = this.GetConfiguration(sectionName);
            return this.AccessTokenAsUser(tUri, tRequest);
        }

        /// <inheritdoc/>
        public string? AccessTokenAsUser(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest, bool cacheEnabled = true)
        {
            string? accessToken = null;
            try
            {
                accessToken = this.AuthenticateAsUser(tokenUri, tokenRequest, cacheEnabled).AccessToken;
            }
            catch (InvalidOperationException e)
            {
                this.logger.LogDebug("Internal issue - returning null access token {Exception}", e.ToString());
            }

            return accessToken;
        }

        /// <inheritdoc/>
        public JwtModel AuthenticateAsUser(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest, bool cacheEnabled = false)
        {
            (JwtModel jwtModel, _) = this.AuthenticateUser(tokenUri, tokenRequest, cacheEnabled);
            return jwtModel;
        }

        /// <inheritdoc/>
        public (JwtModel JwtModel, bool Cached) AuthenticateUser(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest, bool cacheEnabled)
        {
            string cacheKey = $"{tokenUri}:{tokenRequest.Audience}:{tokenRequest.ClientId}:{tokenRequest.Username}";
            bool cached = false;
            this.logger.LogDebug("Attempting to fetch token from cache");
            JwtModel? jwtModel = null;
            if (cacheEnabled && this.tokenCacheMinutes > 0)
            {
                jwtModel = this.cacheProvider.GetItem<JwtModel>(cacheKey);
            }

            if (jwtModel != null)
            {
                this.logger.LogDebug("Auth token found in cache");
                cached = true;
            }
            else
            {
                this.logger.LogInformation("JWT Model not found in cache - Authenticating Direct Grant as User: {Username}", tokenRequest.Username);
                jwtModel = this.ResourceOwnerPasswordGrant(tokenUri, tokenRequest);
                if (jwtModel != null)
                {
                    if (cacheEnabled && this.tokenCacheMinutes > 0)
                    {
                        this.logger.LogDebug("Attempting to store Access token in cache");
                        this.cacheProvider.AddItem(cacheKey, jwtModel, TimeSpan.FromMinutes(this.tokenCacheMinutes));
                    }
                    else
                    {
                        this.logger.LogDebug("Caching is not configured or has been disabled");
                    }
                }
                else
                {
                    this.logger.LogCritical("Unable to authenticate to as {Username} to {TokenUri}", tokenRequest.Username, tokenUri);
                    throw new InvalidOperationException("Auth failure - JwtModel cannot be null");
                }

                this.logger.LogInformation("Finished authenticating User: {Username}", tokenRequest.Username);
            }

            return (jwtModel, cached);
        }

        /// <inheritdoc/>
        public (Uri TokenUri, ClientCredentialsTokenRequest TokenRequest) GetClientCredentialsAuth(string section)
        {
            IConfigurationSection configSection = this.configuration.GetSection(section);
            Uri tokenUri = configSection.GetValue<Uri>(@"TokenUri") ??
                            throw new ArgumentNullException(nameof(section), $"{section} does not contain a valid TokenUri");
            ClientCredentialsTokenRequest tokenRequest = new ClientCredentialsTokenRequest();
            configSection.Bind(tokenRequest); // Client ID, Client Secret, Audience, Scope
            return (tokenUri, tokenRequest);
        }

        /// <inheritdoc/>
        public string? FetchAuthenticatedUserToken()
        {
            HttpContext? httpContext = this.httpContextAccessor?.HttpContext;
            return httpContext.GetTokenAsync("access_token").Result;
        }

        /// <inheritdoc/>
        public string? FetchAuthenticatedUserHdid()
        {
            ClaimsPrincipal? user = this.httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst("hdid")?.Value;
        }

        /// <inheritdoc/>
        public string? FetchAuthenticatedUserId()
        {
            ClaimsPrincipal? user = this.httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        }

        private (Uri TokenUri, ClientCredentialsTokenRequest TokenRequest) GetConfiguration(string sectionName)
        {
            IConfigurationSection configSection = this.configuration.GetSection(sectionName);
            Uri configUri = configSection.GetValue<Uri>(@"TokenUri") ??
                            throw new ArgumentNullException(nameof(sectionName), $"{sectionName} does not contain a valid TokenUri");
            ClientCredentialsTokenRequest configTokenRequest = new();
            configSection.Bind(configTokenRequest); // Client ID, Client Secret, Audience, Username, Password
            return (configUri, configTokenRequest);
        }

        private JwtModel GetSystemToken(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            JwtModel? jwtModel = this.ClientCredentialsGrant(tokenUri, tokenRequest);
            this.logger.LogDebug("Finished authenticating Service. {ClientId}", tokenRequest.ClientId);
            return jwtModel ?? throw new InvalidOperationException("Auth failure - JwtModel cannot be null");
        }

        private JwtModel? ClientCredentialsGrant(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            JwtModel? authModel = null;
            try
            {
                using HttpClient client = this.httpClientFactory.CreateClient();

                IEnumerable<KeyValuePair<string?, string?>> oauthParams = new[]
                {
                    new KeyValuePair<string?, string?>(@"client_id", tokenRequest.ClientId),
                    new KeyValuePair<string?, string?>(@"client_secret", tokenRequest.ClientSecret),
                    new KeyValuePair<string?, string?>(@"audience", tokenRequest.Audience),
                    new KeyValuePair<string?, string?>(@"grant_type", @"client_credentials"),
                };
                using FormUrlEncodedContent content = new(oauthParams);
                content.Headers.Clear();
                content.Headers.Add(@"Content-Type", @"application/x-www-form-urlencoded");

                using HttpResponseMessage response = client.PostAsync(tokenUri, content).Result;

                string jwtTokenResponse = response.Content.ReadAsStringAsync().Result;
                this.logger.LogTrace("JWT Token response: {JwtTokenResponse}", jwtTokenResponse);
                response.EnsureSuccessStatusCode();
                authModel = JsonSerializer.Deserialize<JwtModel>(jwtTokenResponse)!;
            }
            catch (HttpRequestException e)
            {
                this.logger.LogError("Error Message {Message}", e.Message);
            }
            catch (InvalidOperationException e)
            {
                this.logger.LogError("Error Message {Message}", e.Message);
            }

            return authModel;
        }

        private JwtModel? ResourceOwnerPasswordGrant(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            JwtModel? authModel = null;
            try
            {
                using HttpClient client = this.httpClientFactory.CreateClient();

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

                using FormUrlEncodedContent content = new(oauthParams);
                content.Headers.Clear();
                content.Headers.Add(@"Content-Type", @"application/x-www-form-urlencoded");

                using (Task<HttpResponseMessage> task = client.PostAsync(tokenUri, content))
                {
                    HttpResponseMessage response = task.Result;
                    string jwtTokenResponse = response.Content.ReadAsStringAsync().Result;
                    this.logger.LogTrace("JWT Token response: {JwtTokenResponse}", jwtTokenResponse);
                    response.EnsureSuccessStatusCode();
                    authModel = JsonSerializer.Deserialize<JwtModel>(jwtTokenResponse);
                }
            }
            catch (HttpRequestException e)
            {
                this.logger.LogError("Error Message {Message}", e.Message);
            }
            catch (InvalidOperationException e)
            {
                this.logger.LogError("Error Message {Message}", e.Message);
            }

            return authModel;
        }
    }
}
