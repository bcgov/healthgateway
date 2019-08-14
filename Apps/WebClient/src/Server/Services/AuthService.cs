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
    using System.Diagnostics.Contracts;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

#pragma warning disable CA1303 // disable literal strings check

    /// <summary>
    /// Authentication and Authorization Service.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Injected HttpContext Provider.</param>
        /// <param name="configuration">Injected Configuration Provider.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        public AuthService(ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
        }

        /// <summary>
        /// Authenticates the request based on the current context.
        /// </summary>
        /// <returns>The AuthData containing the token and user information.</returns>
        public Models.AuthData GetAuthenticationData()
        {
            Models.AuthData authData = new Models.AuthData();
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            authData.IsAuthenticated = user.Identity.IsAuthenticated;
            if (authData.IsAuthenticated)
            {
                this.logger.LogDebug("Getting Authentication data");
                authData.User = new Models.User
                {
                    Id = user.FindFirstValue("preferred_username"),
                    Name = user.FindFirstValue("name"),
                    Email = user.FindFirstValue(ClaimTypes.Email),
                };
                authData.Token = user.FindFirstValue("access_token");
            }

            return authData;
        }

        /// <summary>
        /// Clears the authorization data from the context.
        /// </summary>
        /// <returns>The signout confirmation followed by the redirect uri.</returns>
        public SignOutResult Logout()
        {
            this.logger.LogTrace("Logging out user");
            return new SignOutResult(new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme });
        }

        /// <summary>
        /// Returns the authentication properties with the populated hint and redirect URL.
        /// </summary>
        /// <returns> The AuthenticationProperties.</returns>
        /// <param name="hint">The OIDC IDP Hint.</param>
        /// <param name="redirectUri">The URI to redirect to after logon.</param>
        public AuthenticationProperties GetAuthenticationProperties(string hint, System.Uri redirectUri)
        {
            this.logger.LogDebug("Getting Authentication properties with hint={0} and redirectUri={1}", hint, redirectUri);
            Contract.Requires(redirectUri != null);

            AuthenticationProperties authProps = new AuthenticationProperties()
            {
                RedirectUri = redirectUri.ToString(),
            };

            if (!string.IsNullOrEmpty(hint))
            {
                authProps.Items.Add(this.configuration["KeyCloak:IDPHintKey"], hint);
            }

            return authProps;
        }
    }
}
