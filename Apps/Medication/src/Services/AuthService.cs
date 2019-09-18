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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Web;
using System.Text;
using HealthGateway.MedicationService.Models;
using Newtonsoft.Json;

namespace HealthGateway.MedicationService
{
    public class AuthService : IAuthService
    {
        public async Task<IAuthModel> GetAuthTokens(string clientId, string clientSecret, string tokenUri, string grantType = @"client_credentials")
        {

            var content = new StringContent(
                JsonConvert.SerializeObject(
                new
                {
                    client_id = clientId,
                    client_secret = clientSecret,
                    grant_type = grantType,
                }), Encoding.UTF8, "application/json");
            var response = await new HttpClient().PostAsync(tokenUri, content);
            response.EnsureSuccessStatusCode();
            var jwtTokenResponse = await response.Content.ReadAsStringAsync();
            var authModel = new JWTModel();

            return authModel;
            // Now build the AuthModel from the reponse
        }
    }
}