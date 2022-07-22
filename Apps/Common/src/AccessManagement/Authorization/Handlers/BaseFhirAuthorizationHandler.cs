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
    /// Base class for IAuthorizationHandler interface.
    /// </summary>
    public abstract class BaseFhirAuthorizationHandler : IAuthorizationHandler
    {
        private const string System = "system";
        private const string RouteResourceIdentifier = "hdid";

        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFhirAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">the injected logger.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        protected BaseFhirAuthorizationHandler(
            ILogger logger,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public abstract Task HandleAsync(AuthorizationHandlerContext context);

        /// <summary>
        /// Gets the resource hdid from the request context.
        /// </summary>
        /// <param name="requirement">The Fhir Requirement.</param>
        /// <returns>The resource hdid.</returns>
        protected string? GetResourceHDID(FhirRequirement requirement)
        {
            string? retVal = null;
            if (requirement.Lookup == FhirResourceLookup.Route)
            {
                retVal = this.httpContextAccessor.HttpContext?.Request.RouteValues[RouteResourceIdentifier] as string;
            }
            else if (requirement.Lookup == FhirResourceLookup.Parameter)
            {
                retVal = this.httpContextAccessor.HttpContext?.Request.Query[RouteResourceIdentifier];
            }

            return retVal;
        }

        /// <summary>
        /// Check if the authenticated user is the owner of the resource being accessed.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHDID">The health data resource subject identifier.</param>
        /// <returns>True if the authenticated user is the owner of the resource, false otherwise.</returns>
        protected bool IsOwner(AuthorizationHandlerContext context, string resourceHDID)
        {
            bool retVal = false;
            string? userHDID = context.User.FindFirst(c => c.Type == GatewayClaims.HDID)?.Value;
            if (userHDID != null)
            {
                retVal = userHDID == resourceHDID;
                this.logger.LogDebug($"{userHDID} is {(!retVal ? "not " : string.Empty)}the resource owner");
            }
            else
            {
                this.logger.LogInformation($"Unable to validate resource owner for {resourceHDID} as no HDID claims present");
            }

            return retVal;
        }

        /// <summary>
        /// Check if the authenticated user has system delegated access to the resource.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHDID">The health data resource subject identifier.</param>
        /// <param name="requirement">The Fhir requirement to satisfy.</param>
        /// <returns>True if the authenticated user has system delegated scope, false otherwise.</returns>
        protected bool IsSystemDelegated(AuthorizationHandlerContext context, string resourceHDID, FhirRequirement requirement)
        {
            bool retVal = false;
            if (context.User.HasClaim(c => c.Type == GatewayClaims.Scope))
            {
                string scopeclaim = context.User.FindFirstValue(GatewayClaims.Scope);
                string[] scopes = scopeclaim.Split(' ');
                this.logger.LogDebug($"Performing system delegation validation for resource {resourceHDID}");
                this.logger.LogDebug($"Caller has the following scopes: {scopeclaim}");
                string[] systemDelegatedScopes = GetAcceptedScopes(System, requirement);
                if (scopes.Intersect(systemDelegatedScopes).Any())
                {
                    this.logger.LogDebug($"Authorized caller as system to have {requirement.AccessType} access to resource {resourceHDID}");
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
        /// <param name="requirement">The requirement to get the resource and access from.</param>
        /// <returns>An array of acceptable scopes.</returns>
        private static string[] GetAcceptedScopes(string type, FhirRequirement requirement)
        {
            string[] acceptedScopes =
            {
                $"{type}/{FhirResource.Wildcard}.{FhirAccessType.Wildcard}",
                $"{type}/{FhirResource.Wildcard}.{requirement.AccessType}",
                $"{type}/{requirement.Resource}.{FhirAccessType.Wildcard}",
                $"{type}/{requirement.Resource}.{requirement.AccessType}",
            };
            return acceptedScopes;
        }
    }
}
