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
namespace HealthGateway.Common.Authentication
{
    using System;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using HealthGateway.Common.Authentication.Models;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// The Authorization service
    /// </summary>
    public class AuthService : IAuthService
    {
        public AuthService(IConfiguration config)
        {
            IConfigurationSection configSection = config?.GetSection("AuthService");

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
        public async Task<IAuthModel> ClientCredentialsAuth()
        {
            JWTModel authModel = new JWTModel();
            try
            {
                using (var content = new StringContent(
                    JsonConvert.SerializeObject(
                    new
                    {
                        client_id = this.TokenRequest.ClientId,
                        client_secret = this.TokenRequest.ClientSecret,
                        audience = this.TokenRequest.Audience,
                        grant_type = @"client_credentials",
                    }),
                    Encoding.UTF8,
                    mediaType: MediaTypeNames.Application.Json))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using (HttpResponseMessage response = await client.PostAsync(this.TokenUri, content).ConfigureAwait(true))
                        {
                            response.EnsureSuccessStatusCode();
                            var jwtTokenResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                            authModel = JsonConvert.DeserializeObject<JWTModel>(jwtTokenResponse);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error Message ${e.Message}");
            }

            return authModel;
        }
    }
}