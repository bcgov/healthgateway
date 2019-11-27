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

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationDashboardFilter"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        public AuthorizationDashboardFilter(IConfiguration configuration, ILogger logger)
        {
            this.configuration = configuration;
            this.requiredUserRole = this.configuration.GetValue<string>("UserRole");
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
            if (principal != null)
            {
                foreach (Claim claim in principal.Claims)
                {
                    System.Console.WriteLine("CLAIM TYPE: " + claim.Type + "; CLAIM VALUE: " + claim.Value);
                }
            }

            if (!user.Identity.IsAuthenticated)
            {
                httpContext.Response.Redirect(AuthorizationConstants.LoginPath);
            }
            if (!principal.IsInRole(this.requiredUserRole))
            {
                    System.Console.WriteLine(@"USER IN ROLE");
            }

            return true;
        }
    }
}
