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
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using HealthGateway.Common.Constants;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Base class to authorize access to one or more FHIR resources.
    /// </summary>
    public abstract class BaseFhirAuthorizationHandler : IAuthorizationHandler
    {
        private const string System = "system";

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFhirAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="httpContextAccessor">The injected HttpContext accessor.</param>
        protected BaseFhirAuthorizationHandler(ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.HttpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the HTTP context accessor.
        /// </summary>
        protected IHttpContextAccessor HttpContextAccessor { get; }

        /// <inheritdoc/>
        public abstract Task HandleAsync(AuthorizationHandlerContext context);

        /// <summary>
        /// Check if the authenticated user has system-delegated access to the resource(s).
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="requirement">The requirement to satisfy.</param>
        /// <returns>True if the authenticated user has system-delegated scope, false otherwise.</returns>
        protected bool IsSystemDelegated(AuthorizationHandlerContext context, GeneralFhirRequirement requirement)
        {
            bool retVal = false;

            this.logger.LogDebug("Performing system delegation validation for resource type {ResourceType}", requirement.ResourceType);

            if (context.User.HasClaim(c => c.Type == GatewayClaims.Scope))
            {
                string scopeClaim = context.User.FindFirstValue(GatewayClaims.Scope) ?? string.Empty;
                IEnumerable<string> scopes = scopeClaim.Split(' ');

                string[] systemDelegatedScopes = GetAcceptedScopes(System, requirement);
                if (scopes.Intersect(systemDelegatedScopes).Any())
                {
                    this.logger.LogDebug("User has a valid scope");
                    retVal = true;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Generates a list of valid scopes which will be used to determine authorization.
        /// No validation is done on the parameters.
        /// </summary>
        /// <param name="type">The type: System or User.</param>
        /// <param name="requirement">The requirement that defines the type of resource and type of access.</param>
        /// <returns>An array of acceptable scopes.</returns>
        private static string[] GetAcceptedScopes(string type, GeneralFhirRequirement requirement)
        {
            return
            [
                $"{type}/{FhirResource.Wildcard}.{FhirAccessType.Wildcard}",
                $"{type}/{FhirResource.Wildcard}.{requirement.AccessType}",
                $"{type}/{requirement.ResourceType}.{FhirAccessType.Wildcard}",
                $"{type}/{requirement.ResourceType}.{requirement.AccessType}",
            ];
        }
    }
}
