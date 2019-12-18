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
namespace HealthGateway.JobScheduler.Authorization
{
    using System.Security.Claims;
    using Hangfire.Dashboard;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The custom user auhorization filter class.
    /// </summary>
    public class AuthorizationDashboardFilter : IDashboardAuthorizationFilter
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        private readonly string requiredUserRole;
        private readonly string userRoleClaim;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationDashboardFilter"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        public AuthorizationDashboardFilter(IConfiguration configuration, ILogger logger)
        {
            this.configuration = configuration;
            this.requiredUserRole = this.configuration.GetValue<string>("OpenIdConnect:UserRole");
            this.userRoleClaim = this.configuration.GetValue<string>("OpenIdConnect:RolesClaim");
            this.logger = logger;
        }

        /// <summary>
        /// Authorization hook for Hangfire.
        /// </summary>
        /// <param name="context">An instance of <see cref="DashboardContext"/> class.</param>
        /// <returns>true, if authorized.</returns>
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var user = httpContext.User;

            ClaimsPrincipal principal = user as ClaimsPrincipal;

            if (!user.Identity.IsAuthenticated)
            {
                string redirectUri = $"{httpContext.Request.PathBase.Value}{AuthorizationConstants.LoginPath}";
                this.logger.LogDebug($"Sending redirect {redirectUri}");
                httpContext.Response.Redirect(redirectUri);
                return true;
            }

            if (!principal.HasClaim(this.userRoleClaim, this.requiredUserRole))
            {
                this.logger.LogInformation($"User is not authorized. This usually means user is not in the proper user role: {this.requiredUserRole}");
                return false;
            }

            return true;
        }
    }
}
