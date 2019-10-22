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
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Logging;

    public class UserAuthorizationHandler : AuthorizationHandler<UserIsPatientRequirement, string>
    {
        private readonly ILogger<UserAuthorizationHandler> logger;

        public UserAuthorizationHandler(ILogger<UserAuthorizationHandler> logger)
        {
            this.logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserIsPatientRequirement requirement, string resource)
        {
            string hdidClaim = context?.User.FindFirst(c => c.Type == "hdid").Value;

            if (!string.Equals(hdidClaim, resource, System.StringComparison.Ordinal))
            {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                this.logger?.LogWarning(@"hdid parameter doest not match user's JWT");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                context?.Fail();
                return Task.CompletedTask;
            }

            context?.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}