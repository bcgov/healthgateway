﻿//-------------------------------------------------------------------------
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

namespace HealthGateway.WebClient.Services
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Authentication and Authorization Service.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates the request based on the current context.
        /// </summary>
        /// <returns>The AuthData containing the token and user information.</returns>
        Models.AuthData GetAuthenticationData();

        /// <summary>
        /// Clears the authorization data from the context.
        /// </summary>
        /// <returns>The signout confirmation followed by the redirect uri.</returns>
        SignOutResult Logout();

        /// <summary>
        /// Returns the authentication properties with the populated hint and redirect URL.
        /// </summary>
        /// <returns> The AuthenticationProperties.</returns>
        /// <param name="hint">The OIDC IDP Hint.</param>
        /// <param name="redirectUri">The URI to redirect to after logon.</param>
        AuthenticationProperties GetAuthenticationProperties(string hint, System.Uri redirectUri);
    }
}
