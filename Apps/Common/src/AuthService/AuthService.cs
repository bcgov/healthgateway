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
namespace HealthGateway.Common.AuthService
{
    using System;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using HealthGateway.Common.AuthService.Models;

    /// <summary>
    /// The Authorization service.
    /// </summary>
    public class AuthService : IAuthService
    {
        public AuthService(string clientId, string clientSecret, Uri tokenUri, string grantType = @"client_credentials")
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            GrantType = grantType;
            TokenUri = tokenUri;
        }

        /// <inheritdoc/>
        public string ClientId { get; }
        /// <inheritdoc/>
        public string ClientSecret { get; }
        /// <inheritdoc/>
        public Uri TokenUri { get; }
        /// <inheritdoc/>
        public string GrantType { get; }
        /// <inheritdoc/>
        public async Task<IAuthModel> GetAuthTokens()
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(
                new
                {
                    client_id = this.ClientId,
                    client_secret = this.ClientSecret,
                    grant_type = this.GrantType
                }),
                Encoding.UTF8,
                MediaTypeNames.Application.Json);

            IAuthModel authModel;
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.PostAsync(this.TokenUri, content).ConfigureAwait(true))
            {
                response.EnsureSuccessStatusCode();
                var jwtTokenResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                authModel = JsonConvert.DeserializeObject<JWTModel>(jwtTokenResponse);
            }
            return authModel;
        }
    }
}