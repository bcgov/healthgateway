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
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
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
        private readonly IAuthenticationDelegate authenticationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Injected HttpContext Provider.</param>
        /// <param name="configuration">Injected Configuration Provider.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="profileDelegate">Injected Admin User Profile Delegate Provider.</param>
        /// <param name="authenticationDelegate">Injected authentication delegate.</param>
        public AuthenticationService(
            ILogger<AuthenticationService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IAdminUserProfileDelegate profileDelegate,
            IAuthenticationDelegate authenticationDelegate)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.enabledRoles = configuration.GetSection("EnabledRoles").Get<string[]>() ?? [];
            this.profileDelegate = profileDelegate;
            this.authenticationDelegate = authenticationDelegate;
        }

        /// <inheritdoc/>
        public async Task<AuthenticationData> GetAuthenticationDataAsync(CancellationToken ct = default)
        {
            AuthenticationData authData = new();
            ClaimsPrincipal? user = this.httpContextAccessor.HttpContext?.User;
            authData.IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;
            if (authData.IsAuthenticated && user != null)
            {
                this.logger.LogDebug("Getting Authentication data");
                authData.User = new UserProfile
                {
                    Id = user.FindFirstValue("preferred_username") ?? string.Empty,
                    Name = user.FindFirstValue("name"),
                    Email = user.FindFirstValue(ClaimTypes.Email),
                };
                List<string> userRoles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(role => role.Value).ToList();
                authData.Roles = this.enabledRoles.Intersect(userRoles).ToList();
                authData.IsAuthorized = authData.Roles.Count > 0;
                authData.Token = await this.authenticationDelegate.FetchAuthenticatedUserTokenAsync(ct) ?? string.Empty;
            }

            return authData;
        }

        /// <inheritdoc/>
        public SignOutResult Logout()
        {
            this.logger.LogTrace("Logging out user");
            return new SignOutResult(new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme });
        }

        /// <inheritdoc/>
        public async Task SetLastLoginDateTimeAsync(CancellationToken ct = default)
        {
            ClaimsPrincipal? user = this.httpContextAccessor.HttpContext?.User;
            AuthenticationData authData = await this.GetAuthenticationDataAsync(ct);
            if (authData is not { IsAuthenticated: true, User: not null } || user == null)
            {
                return;
            }

            DateTime jwtAuthTime = ClaimsPrincipalReader.GetAuthDateTime(user);

            DbResult<AdminUserProfile> result = await this.profileDelegate.GetAdminUserProfileAsync(authData.User.Id, ct);
            if (result.Status == DbStatusCode.NotFound)
            {
                // Create profile
                AdminUserProfile newProfile = new()
                {
                    // Keycloak always creates username in lowercase
                    Username = authData.User.Id,
                    LastLoginDateTime = jwtAuthTime,
                };
                DbResult<AdminUserProfile> insertResult = await this.profileDelegate.AddAsync(newProfile, ct);
                if (insertResult.Status == DbStatusCode.Error)
                {
                    this.logger.LogError("Unable to add admin user profile to DB.... {Result}", JsonSerializer.Serialize(insertResult));
                }
            }
            else
            {
                // Update profile
                result.Payload.LastLoginDateTime = jwtAuthTime;
                DbResult<AdminUserProfile> updateResult = await this.profileDelegate.UpdateAsync(result.Payload, ct: ct);
                if (updateResult.Status == DbStatusCode.Error)
                {
                    this.logger.LogError("Unable to update admin user profile to DB... {Result}", JsonSerializer.Serialize(updateResult));
                }
            }
        }
    }
}
