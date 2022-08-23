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
namespace HealthGateway.Common.AccessManagement.Authorization.Handlers
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// PatientAuthorizationHandler validates that Patient requirements have been met.
    /// </summary>
    public class UserAuthorizationHandler : IAuthorizationHandler
    {
        private const string RouteResourceIdentifier = "hdid";

        private readonly ILogger<UserAuthorizationHandler> logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">the injected logger.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        public UserAuthorizationHandler(ILogger<UserAuthorizationHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Asserts that the user accessing the resource (hdid in route) is one of:
        ///     1) A HealthGateway user (has an HDID)
        ///     2) When the requirement requests ownership verification - checks for matching hdids.
        /// </summary>
        /// <param name="context">the AuthorizationHandlerContext context.</param>
        /// <returns>The Authorization Result.</returns>
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            string? resourceHDID = this.httpContextAccessor.HttpContext?.Request.RouteValues[RouteResourceIdentifier] as string;
            foreach (UserRequirement requirement in context.PendingRequirements.OfType<UserRequirement>().Where(requirement => this.Authorize(context, resourceHDID, requirement)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Check if the authenticated user has an HDID and optionally the owner of the User resource being accessed.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHDID">The health data resource subject identifier.</param>
        /// <param name="requirement">The requirement to validate.</param>
        private bool Authorize(AuthorizationHandlerContext context, string? resourceHDID, UserRequirement requirement)
        {
            bool retVal = false;
            ClaimsPrincipal user = context.User;
            string? userHDID = user.FindFirst(c => c.Type == GatewayClaims.HDID)?.Value;
            if (userHDID != null)
            {
                if (requirement.ValidateOwnership)
                {
                    retVal = userHDID == resourceHDID;
                    this.logger.LogInformation($"{userHDID} is {(!retVal ? "not " : string.Empty)}the resource owner");
                }
                else
                {
                    retVal = true;
                    this.logger.LogInformation($"User has claim {GatewayClaims.HDID} and has been authorized");
                }
            }
            else
            {
                this.logger.LogDebug($"Unable to validate resource owner for {resourceHDID} as no HDID claims present");
            }

            return retVal;
        }
    }
}
