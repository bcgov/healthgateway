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
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
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
        public async Task<JwtModel> AuthenticateAsSystemAsync(ClientCredentialsRequest request, bool cacheEnabled = true, CancellationToken ct = default)
        {
            JwtModel? jwtModel;

            if (cacheEnabled)
            {
                string cacheKey = $"{request.TokenUri}:{request.Parameters.Audience}:{request.Parameters.ClientId}";
                jwtModel = await this.cacheProvider.GetItemAsync<JwtModel>(cacheKey, ct);
                if (jwtModel is null)
                {
                    jwtModel = await this.GetSystemTokenAsync(request, ct);
                    if (jwtModel.ExpiresIn is not null)
                    {
                        int expiry = jwtModel.ExpiresIn.Value - 10;
                        await this.cacheProvider.AddItemAsync(cacheKey, jwtModel, TimeSpan.FromSeconds(expiry), ct);
                    }
                }
            }
            else
            {
                jwtModel = await this.GetSystemTokenAsync(request, ct);
            }

            this.logger.LogDebug("Authenticating Service... {ClientId}", request.Parameters.ClientId);
            return jwtModel;
        }

        /// <inheritdoc/>
        public async Task<JwtModel> AuthenticateUserAsync(ClientCredentialsRequest request, bool cacheEnabled = false, CancellationToken ct = default)
        {
            JwtModel? jwtModel = null;

            string cacheKey = $"{request.TokenUri}:{request.Parameters.Audience}:{request.Parameters.ClientId}:{request.Parameters.Username}";
            if (cacheEnabled && this.tokenCacheMinutes > 0)
            {
                this.logger.LogDebug("Attempting to fetch token from cache");
                jwtModel = await this.cacheProvider.GetItemAsync<JwtModel>(cacheKey, ct);
            }

            if (jwtModel == null)
            {
                this.logger.LogInformation("JWT Model not found in cache - Authenticating Direct Grant as User: {Username}", request.Parameters.Username);
                jwtModel = await this.ResourceOwnerPasswordGrantAsync(request, ct);

                if (jwtModel == null)
                {
                    this.logger.LogCritical("Unable to authenticate to as {Username} to {TokenUri}", request.Parameters.Username, request.TokenUri);
                    throw new InvalidOperationException("Auth failure - JwtModel cannot be null");
                }

                if (cacheEnabled && this.tokenCacheMinutes > 0)
                {
                    this.logger.LogDebug("Attempting to store Access token in cache");
                    await this.cacheProvider.AddItemAsync(cacheKey, jwtModel, TimeSpan.FromMinutes(this.tokenCacheMinutes), ct);
                }
                else
                {
                    this.logger.LogDebug("Caching is not configured or has been disabled");
                }

                this.logger.LogInformation("Finished authenticating User: {Username}", request.Parameters.Username);
            }
            else
            {
                this.logger.LogDebug("Auth token found in cache");
            }

            return jwtModel;
        }

        /// <inheritdoc/>
        public ClientCredentialsRequest GetClientCredentialsRequestFromConfig(string section)
        {
            IConfigurationSection configSection = this.configuration.GetSection(section);
            Uri tokenUri = configSection.GetValue<Uri>("TokenUri") ??
                           throw new ArgumentNullException(nameof(section), $"{section} does not contain a valid TokenUri");
            ClientCredentialsRequestParameters parameters = new();
            configSection.Bind(parameters); // Client ID, Client Secret, Audience, Scope
            return new() { TokenUri = tokenUri, Parameters = parameters };
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage(Justification = "Convenience method to extract access token from HttpContext")]
        public async Task<string?> FetchAuthenticatedUserTokenAsync(CancellationToken ct = default)
        {
            HttpContext? httpContext = this.httpContextAccessor?.HttpContext;
            return await httpContext.GetTokenAsync("access_token");
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage(Justification = "Convenience method to extract claim from HttpContext")]
        public string? FetchAuthenticatedUserHdid()
        {
            ClaimsPrincipal? user = this.httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst("hdid")?.Value;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage(Justification = "Convenience method to extract claim from HttpContext")]
        public string? FetchAuthenticatedUserId()
        {
            ClaimsPrincipal? user = this.httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        }

        /// <inheritdoc/>
        public UserLoginClientType? FetchAuthenticatedUserClientType()
        {
            ClaimsPrincipal? user = this.httpContextAccessor?.HttpContext?.User;
            string? azp = user?.FindFirst("azp")?.Value;
            UserLoginClientType? userLoginClientType = azp switch
            {
                "hg" => UserLoginClientType.Web,
                "hg-mobile" => UserLoginClientType.Mobile,
                "icarus" => UserLoginClientType.Salesforce,
                _ => null,
            };

            return userLoginClientType;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage(Justification = "Convenience method to extract claim from HttpContext")]
        public string? FetchAuthenticatedPreferredUsername()
        {
            ClaimsPrincipal? user = this.httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst("preferred_username")?.Value;
        }

        private async Task<JwtModel> GetSystemTokenAsync(ClientCredentialsRequest request, CancellationToken ct)
        {
            JwtModel? jwtModel = await this.ClientCredentialsGrantAsync(request, ct);
            this.logger.LogDebug("Finished authenticating Service. {ClientId}", request.Parameters.ClientId);
            return jwtModel ?? throw new InvalidOperationException("Auth failure - JwtModel cannot be null");
        }

        private async Task<JwtModel?> ClientCredentialsGrantAsync(ClientCredentialsRequest request, CancellationToken ct)
        {
            ClientCredentialsRequestParameters parameters = request.Parameters;
            IEnumerable<KeyValuePair<string?, string?>> oauthParams = new[]
            {
                new KeyValuePair<string?, string?>("client_id", parameters.ClientId),
                new KeyValuePair<string?, string?>("client_secret", parameters.ClientSecret),
                new KeyValuePair<string?, string?>("audience", parameters.Audience),
                new KeyValuePair<string?, string?>("grant_type", "client_credentials"),
            };
            using FormUrlEncodedContent content = new(oauthParams);
            return await this.AuthenticateAsync(request.TokenUri, content, ct);
        }

        private async Task<JwtModel?> ResourceOwnerPasswordGrantAsync(ClientCredentialsRequest request, CancellationToken ct)
        {
            ClientCredentialsRequestParameters parameters = request.Parameters;
            IEnumerable<KeyValuePair<string?, string?>> oauthParams = new[]
            {
                new KeyValuePair<string?, string?>("client_id", parameters.ClientId),
                new KeyValuePair<string?, string?>("client_secret", parameters.ClientSecret),
                new KeyValuePair<string?, string?>("grant_type", "password"),
                new KeyValuePair<string?, string?>("audience", parameters.Audience),
                new KeyValuePair<string?, string?>("scope", parameters.Scope),
                new KeyValuePair<string?, string?>("username", parameters.Username),
                new KeyValuePair<string?, string?>("password", parameters.Password),
            };

            using FormUrlEncodedContent content = new(oauthParams);
            return await this.AuthenticateAsync(request.TokenUri, content, ct);
        }

        private async Task<JwtModel?> AuthenticateAsync(Uri tokenUri, FormUrlEncodedContent content, CancellationToken ct)
        {
            JwtModel? authModel = null;
            try
            {
                using HttpClient client = this.httpClientFactory.CreateClient();
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync(tokenUri, content, ct);
                response.EnsureSuccessStatusCode();
                authModel = await response.Content.ReadFromJsonAsync<JwtModel>(ct);
            }
            catch (Exception e) when (e is HttpRequestException or InvalidOperationException or JsonException)
            {
                this.logger.LogError(e, "Error Message {Message}", e.Message);
            }

            return authModel;
        }
    }
}
