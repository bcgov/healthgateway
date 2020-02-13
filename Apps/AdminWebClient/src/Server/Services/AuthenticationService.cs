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
namespace HealthGateway.Admin.Services
{
    using System.Diagnostics.Contracts;
    using System.Security.Claims;
    using HealthGateway.Admin.Models;
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
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Injected HttpContext Provider.</param>
        /// <param name="configuration">Injected Configuration Provider.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        public AuthenticationService(ILogger<AuthenticationService> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
        }

        /// <summary>
        /// Authenticates the request based on the current context.
        /// </summary>
        /// <returns>The AuthData containing the token and user information.</returns>
        public AuthenticationData GetAuthenticationData()
        {
            AuthenticationData authData = new AuthenticationData();
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            authData.IsAuthenticated = user.Identity.IsAuthenticated;
            if (authData.IsAuthenticated)
            {
                this.logger.LogDebug("Getting Authentication data");
                authData.User = new UserProfile
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
        /// <param name="redirectPath">The URI to redirect to after logon.</param>
        public AuthenticationProperties GetAuthenticationProperties(string redirectPath)
        {
            this.logger.LogDebug("Getting Authentication properties with redirectPath={0}", redirectPath);
            Contract.Requires(redirectPath != null);

            AuthenticationProperties authProps = new AuthenticationProperties()
            {
                RedirectUri = redirectPath,
            };

            return authProps;
        }
    }
}