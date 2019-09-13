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
    public interface IAuthService
    {
        /// <summary>
        /// Gets the OAuth Token (OAuth Client Credentials Grant) to use to authenticate with the HNClient API.
        /// </summary>
        /// <param name="clientId">The OIDC client_id.</param>
        /// <param name="cientSecret">The OIDC client_secret</param>
        /// <param name="authTokenURI">The OIDC endpoint to retrieve the Token from</param>
        /// <param name="grantType">Optional grantType for OIDC credentials grant, the default is 'client_credentials'</param> 
        /// <returns>The AccessToken.</returns>
        static async Task<AuthContainer> GetAuthTokens(string clientId, string clientSecret, string tokenURI,  string grantType = @"client_credentials");
    }
}
