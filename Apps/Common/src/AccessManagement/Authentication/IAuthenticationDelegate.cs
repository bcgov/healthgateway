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
namespace HealthGateway.Common.AccessManagement.Authentication
{
    using System;
    using HealthGateway.Common.AccessManagement.Authentication.Models;

    /// <summary>
    /// The authorization service interface.
    /// This supports direct grant for OAuth2 Client Credentials Grant flows 
    /// and Resource Owner Password Grant flows.
    /// </summary>
    public interface IAuthenticationDelegate
    {
        /// <summary>
        /// Gets or sets the Client Credentials Grant Token Request parameters (Open ID Connect standard).
        /// </summary>
        ClientCredentialsTokenRequest TokenRequest { get; set; }

        /// <summary>
        /// Gets the OAuth2 Auth Token URI.
        /// </summary>
        Uri TokenUri { get; }

        /// <summary>
        /// Authenticates as a 'system account' concept, using OAuth 2.0 Client Credentials Grant.
        /// </summary>
        /// <returns>An instance fo the <see cref="JWTModel"/> class.</returns>
        JWTModel AuthenticateAsSystem();

        /// <summary>
        /// Authenticates a resource owner user with direct grant, no user intervention.
        /// </summary>
        /// <returns>An instance fo the <see cref="JWTModel"/> class.</returns>        
        JWTModel AuthenticateAsUser();
    }
}
