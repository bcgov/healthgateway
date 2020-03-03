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
namespace HealthGateway.Common.AccessManagement.Authentication.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// The Authorization service.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> logger;

        private readonly IHttpClientService httpClientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        public AuthService(
            ILogger<AuthService> logger,
            IConfiguration config,
            IHttpClientService httpClientService)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            IConfigurationSection? configSection = config?.GetSection("AuthService");

            this.TokenUri = new Uri(configSection.GetValue<string>("TokenUri"));
            this.TokenRequest = new ClientCredentialsTokenRequest()
            {
                Audience = configSection.GetValue<string>("Audience"),
                ClientId = configSection.GetValue<string>("ClientId"),
                ClientSecret = configSection.GetValue<string>("ClientSecret"),
            };
        }

        /// <inheritdoc/>
        public ClientCredentialsTokenRequest TokenRequest { get; set; }

        /// <inheritdoc/>
        public Uri TokenUri { get; }

        /// <inheritdoc/>
        public JWTModel AuthenticateService()
        {
            this.logger.LogDebug($"Authenticating Service... {this.TokenRequest.ClientId}");
            Task<IAuthModel> authenticating = this.ClientCredentialsAuth(); // @todo: maybe cache this in future for efficiency

            JWTModel jwtModel = (authenticating.Result as JWTModel)!;
            this.logger.LogDebug($"Finished authenticating Service. {this.TokenRequest.ClientId}");
            return jwtModel;
        }

        private async Task<IAuthModel> ClientCredentialsAuth()
        {
            JWTModel authModel = new JWTModel();
            try
            {
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                // Create content for keycloak
                IEnumerable<KeyValuePair<string, string>> keycloakParams = new[]
                {
                        new KeyValuePair<string, string>("client_id", this.TokenRequest.ClientId!),
                        new KeyValuePair<string, string>("client_secret", this.TokenRequest.ClientSecret!),
                        new KeyValuePair<string, string>("audience", this.TokenRequest.Audience!),
                        new KeyValuePair<string, string>("grant_type", @"client_credentials"),
                    };
                using var content = new FormUrlEncodedContent(keycloakParams);
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                using HttpResponseMessage response = await client.PostAsync(this.TokenUri, content).ConfigureAwait(true);

                string jwtTokenResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace($"JWT Token response: ${jwtTokenResponse}");
                response.EnsureSuccessStatusCode();
                authModel = JsonConvert.DeserializeObject<JWTModel>(jwtTokenResponse);
            }
            catch (HttpRequestException e)
            {
                this.logger.LogError($"Error Message ${e.Message}");
            }

            return authModel;
        }
    }
}