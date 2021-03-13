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

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Authorization service.
    /// </summary>
    public class AuthenticationDelegate : IAuthenticationDelegate
    {
        private readonly ILogger<IAuthenticationDelegate> logger;
        private readonly IHttpClientService httpClientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        public AuthenticationDelegate(
            ILogger<IAuthenticationDelegate> logger,
            IHttpClientService httpClientService)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
        }

        /// <inheritdoc/>
        public JWTModel AuthenticateAsSystem(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            this.logger.LogDebug($"Authenticating Service... {tokenRequest.ClientId}");
            Task<IAuthModel> authenticating = this.ClientCredentialsGrant(tokenUri, tokenRequest);

<<<<<<< HEAD
            JWTModel jwtModel = (authenticating.Result as JWTModel)!;
            this.logger.LogDebug($"Finished authenticating Service. {tokenRequest.ClientId}");
=======
            JWTModel jwtModel = (authenticating.Result as JWTModel) !;
            this.logger.LogDebug($"Finished authenticating Service. {this.TokenRequest.ClientId}");
>>>>>>> features/9710
            return jwtModel;
        }

        /// <inheritdoc/>
        public JWTModel AuthenticateAsUser(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            this.logger.LogDebug($"Authenticating Direct Grant as User: {tokenRequest.Username}");
            Task<IAuthModel> authenticating = this.ResourceOwnerPasswordGrant(tokenUri, tokenRequest);

            JWTModel jwtModel = (authenticating.Result as JWTModel)!;
            this.logger.LogDebug($"Finished authenticating User: {tokenRequest.Username}");

            return jwtModel;
        }

        private async Task<IAuthModel> ClientCredentialsGrant(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            JWTModel authModel = new ();
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

                using HttpResponseMessage response = await client.PostAsync(tokenUri, content).ConfigureAwait(true);

                string jwtTokenResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace($"JWT Token response: {jwtTokenResponse}");
                response.EnsureSuccessStatusCode();
                authModel = JsonSerializer.Deserialize<JWTModel>(jwtTokenResponse) !;
            }
            catch (HttpRequestException e)
            {
                this.logger.LogError($"Error Message {e.Message}");
            }

            return authModel;
        }

        private async Task<IAuthModel> ResourceOwnerPasswordGrant(Uri tokenUri, ClientCredentialsTokenRequest tokenRequest)
        {
            JWTModel authModel = new ();
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

                using HttpResponseMessage response = await client.PostAsync(tokenUri, content).ConfigureAwait(true);

                string jwtTokenResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace($"JWT Token response: {jwtTokenResponse}");
                response.EnsureSuccessStatusCode();
                authModel = JsonSerializer.Deserialize<JWTModel>(jwtTokenResponse) !;
            }
            catch (HttpRequestException e)
            {
                this.logger.LogError($"Error Message {e.Message}");
            }

            return authModel;
        }
    }
}
