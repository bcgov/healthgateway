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
namespace HealthGateway.Common.Authorization
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// UserAuthorizationHandler is a custom authorization handler that 
    /// checks whether the authenticated user has an hdid claim that matches the hdid of the resource being accessed.
    /// </summary>
    public class UserAuthorizationHandler : AuthorizationHandler<UserIsPatientRequirement, string>
    {
        private readonly ILogger<UserAuthorizationHandler> logger;

        /// <summary>
        /// UserAuthorizationHandler constructor
        /// </summary>
        /// <param name="logger">the injected logger.</param>
        public UserAuthorizationHandler(ILogger<UserAuthorizationHandler> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// HandleRequirementAsync method to assert that the hdid in the JWT matches the hdid in the request
        /// </summary>
        /// <param name="context">the AuthorizationHandlerContext context.</param>
        /// <param name="requirement">the UserIsPatientRequirement requirement.</param>
        /// <param name="hdid">The patient identifier used as the resource argument.</param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserIsPatientRequirement requirement, string hdid)
        {
            if (this.IsOwner(context.User, hdid))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Check if the authenticated user is the same patient as what is being accessed.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        /// <param name="hdid">The health data resource subject identifier.</param>        
        private bool IsOwner(ClaimsPrincipal user, string hdid)
        {
            if (user == null || hdid == null)
                return false;
            string hdidClaim = user.FindFirst(c => c.Type == "hdid").Value;
            return string.Equals(hdidClaim, hdid, System.StringComparison.Ordinal);
        }
    }

}