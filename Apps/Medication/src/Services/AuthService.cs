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
namespace HealthGateway.MedicationService
{
    using System;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using HealthGateway.MedicationService.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// The Authorization service.
    /// </summary>
    public class AuthService : IAuthService
    {
        /// <inheritdoc/>
        public async Task<IAuthModel> GetAuthTokens(string clientId, string clientSecret, Uri tokenUri, string grantType = @"client_credentials")
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(
                new
                {
                    client_id = clientId,
                    client_secret = clientSecret,
                    grant_type = grantType,
                }),
                Encoding.UTF8,
                MediaTypeNames.Application.Json);

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.PostAsync(tokenUri, content).ConfigureAwait(true))
            {
                response.EnsureSuccessStatusCode();
                var jwtTokenResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            }

            var authModel = new JWTModel();
            content.Dispose();
            return authModel;

            // Now build the AuthModel from the reponse
        }
    }
}