//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
namespace namespace HealthGateway.Common.UserManagedAccess.Models
{
    /// <summary>
    /// An authorization response in form of an OAuth2 access token.
    /// </summary>
    public class AuthorizationResponse : AccessTokenResponse
    {
        /// <summary>Initializes a new instance of the <see cref="AuthorizationResponse"/> class.</summary>
        /// <param name="response">An <see cref="AccessTokenResponse"/> response.</param>
        /// <param name="upgraded">A boolean whether the token was upgraded.</param>
        public AuthorizationResponse(AccessTokenResponse response, bool upgraded)
        {
            this.AccessToken = response.AccessToken;
            this.TokenType = "Bearer";
            this.RefreshToken = response.RefreshToken;
            this.RefreshExpiresIn = response.RefreshExpiresIn;
            this.ExpiresIn = response.ExpiresIn;
            this.NotBeforePolicy = response.NotBeforePolicy;
            this.Upgraded = upgraded;
        }

        /// <summary>Gets or sets a value indicating whether the token was upgraded.</summary>
        public bool Upgraded { get; set; }
    }
}