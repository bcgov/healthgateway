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
namespace HealthGateway.Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
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
        private readonly string[] enabledRoles;
        private readonly IAdminUserProfileDelegate profileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Injected HttpContext Provider.</param>
        /// <param name="configuration">Injected Configuration Provider.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="profileDelegate">Injected Admin User Profile Delegate Provider.</param>
        public AuthenticationService(ILogger<AuthenticationService> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IAdminUserProfileDelegate profileDelegate)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.enabledRoles = configuration.GetSection("EnabledRoles").Get<string[]>();
            this.profileDelegate = profileDelegate;
        }

        /// <summary>
        /// Authenticates the request based on the current context.
        /// </summary>
        /// <returns>The AuthData containing the token and user information.</returns>
        public AuthenticationData GetAuthenticationData()
        {
            AuthenticationData authData = new AuthenticationData();
            ClaimsPrincipal? user = this.httpContextAccessor.HttpContext?.User;
            authData.IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;
            if (authData.IsAuthenticated && user != null)
            {
                this.logger.LogDebug("Getting Authentication data");
                authData.User = new UserProfile
                {
                    Id = user.FindFirstValue("preferred_username"),
                    Name = user.FindFirstValue("name"),
                    Email = user.FindFirstValue(ClaimTypes.Email),
                };
                List<string> userRoles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(role => role.Value).ToList();
                authData.Roles = this.enabledRoles.Intersect(userRoles).ToList();
                authData.IsAuthorized = authData.Roles.Count > 0;
                authData.Token = Task.Run(async () => await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true)).Result ?? string.Empty;
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

        /// <inheritdoc />
        public void SetLastLoginDateTime()
        {
            ClaimsPrincipal? user = this.httpContextAccessor.HttpContext?.User;
            AuthenticationData authData = this.GetAuthenticationData();
            if (authData.IsAuthenticated && authData.User != null && user != null)
            {
                DateTime jwtAuthTime = ClaimsPrincipalReader.GetAuthDateTime(user);

                DBResult<Database.Models.AdminUserProfile> result = this.profileDelegate.GetAdminUserProfile(username: authData.User.Id);
                if (result.Status == DBStatusCode.NotFound)
                {
                    // Create profile
                    Database.Models.AdminUserProfile newProfile = new()
                    {
                        // Keycloak id is case insensitive
                        Username = authData.User.Id.ToLower(CultureInfo.CurrentCulture),
                        Email = authData.User.Email,
                        LastLoginDateTime = jwtAuthTime,
                    };
                    DBResult<Database.Models.AdminUserProfile> insertResult = this.profileDelegate.Add(newProfile);
                    if (insertResult.Status == DBStatusCode.Error)
                    {
                        this.logger.LogError("Unable to add admin user profile to DB.... {Result}", JsonSerializer.Serialize(insertResult));
                    }
                }
                else
                {
                    // Update profile
                    result.Payload.LastLoginDateTime = jwtAuthTime;
                    DBResult<Database.Models.AdminUserProfile> updateResult = this.profileDelegate.Update(result.Payload);
                    if (updateResult.Status == DBStatusCode.Error)
                    {
                        this.logger.LogError("Unable to update admin user profile to DB... {Result}", JsonSerializer.Serialize(updateResult));
                    }
                }
            }
        }
    }
}
