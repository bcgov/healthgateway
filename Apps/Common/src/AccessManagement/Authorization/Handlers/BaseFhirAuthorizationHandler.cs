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
        private const string RouteResourceIdentifier = "hdid";

        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFhirAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="httpContextAccessor">The injected HttpContext accessor.</param>
        protected BaseFhirAuthorizationHandler(ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public abstract Task HandleAsync(AuthorizationHandlerContext context);

        /// <summary>
        /// Gets the subject identifier for requested health data resource(s) from the request context.
        /// </summary>
        /// <param name="lookupMethod">The mechanism with which to retrieve the subject identifier.</param>
        /// <returns>The subject identifier for requested health data resource(s).</returns>
        protected string? GetResourceHdid(FhirSubjectLookupMethod lookupMethod)
        {
            string? retVal = lookupMethod switch
            {
                FhirSubjectLookupMethod.Route => this.httpContextAccessor.HttpContext?.Request.RouteValues[RouteResourceIdentifier] as string,
                FhirSubjectLookupMethod.Parameter => this.httpContextAccessor.HttpContext?.Request.Query[RouteResourceIdentifier],
                _ => null,
            };

            return retVal;
        }

        /// <summary>
        /// Check if the authenticated user has system-delegated access to the resource(s).
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="requirement">The requirement to satisfy.</param>
        /// <returns>True if the authenticated user has system-delegated scope, false otherwise.</returns>
        protected bool IsSystemDelegated(AuthorizationHandlerContext context, GeneralFhirRequirement requirement)
        {
            bool retVal = false;
            if (context.User.HasClaim(c => c.Type == GatewayClaims.Scope))
            {
                string scopeClaim = context.User.FindFirstValue(GatewayClaims.Scope) ?? string.Empty;
                string[] scopes = scopeClaim.Split(' ');
                this.logger.LogDebug("Performing system delegation validation for resource type {ResourceType}", requirement.ResourceType);
                this.logger.LogDebug("Caller has the following scopes: {ScopeClaim}", scopeClaim);
                string[] systemDelegatedScopes = GetAcceptedScopes(System, requirement);
                if (scopes.Intersect(systemDelegatedScopes).Any())
                {
                    this.logger.LogDebug("Authorized caller as system to have {AccessType} access to resource type {ResourceType}", requirement.AccessType, requirement.ResourceType);
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
            string[] acceptedScopes =
            {
                $"{type}/{FhirResource.Wildcard}.{FhirAccessType.Wildcard}",
                $"{type}/{FhirResource.Wildcard}.{requirement.AccessType}",
                $"{type}/{requirement.ResourceType}.{FhirAccessType.Wildcard}",
                $"{type}/{requirement.ResourceType}.{requirement.AccessType}",
            };
            return acceptedScopes;
        }
    }
}
