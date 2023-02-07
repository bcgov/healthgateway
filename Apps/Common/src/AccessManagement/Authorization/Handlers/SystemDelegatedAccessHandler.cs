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
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Authorizes access to FHIR resources when the user is system-delegated to access the resources.
    /// </summary>
    public class SystemDelegatedAccessHandler : BaseFhirAuthorizationHandler
    {
        private readonly ILogger<SystemDelegatedAccessHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemDelegatedAccessHandler"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="httpContextAccessor">The injected HttpContext accessor.</param>
        public SystemDelegatedAccessHandler(ILogger<SystemDelegatedAccessHandler> logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Asserts that the user accessing the resource(s) is system-delegated to access the resource(s).
        /// </summary>
        /// <param name="context">The authorization information.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (GeneralFhirRequirement requirement in context.PendingRequirements.OfType<GeneralFhirRequirement>())
            {
                if (requirement.SupportsSystemDelegation && this.IsSystemDelegated(context, requirement))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    this.logger.LogDebug("System-delegated access rejected; Supports delegation: {SupportsSystemDelegation}", requirement.SupportsSystemDelegation);
                }
            }

            return Task.CompletedTask;
        }
    }
}
