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
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// FhirResourceAuthorizationHandler validates that a FhirRequirement has been met.
    /// </summary>
    public class FhirResourceAuthorizationHandler : IAuthorizationHandler
    {
        private const string System = "system";
        private const string User = "user";
        private const string RouteResourceIdentifier = "hdid";

        private readonly ILogger<FhirResourceAuthorizationHandler> logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IResourceDelegateDelegate? resourceDelegateDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="FhirResourceAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">the injected logger.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        /// <param name="resourceDelegateDelegate">The ResourceDelegate delegate to interact with the DB.</param>
        public FhirResourceAuthorizationHandler(ILogger<FhirResourceAuthorizationHandler> logger, IHttpContextAccessor httpContextAccessor, IResourceDelegateDelegate? resourceDelegateDelegate = null)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
        }

        /// <summary>
        /// Asserts that the user accessing the resource (hdid in route) is one of:
        ///     1) The owner of the resource.
        ///     2) Delegated to access the resource.
        /// </summary>
        /// <param name="context">the AuthorizationHandlerContext context.</param>
        /// <returns>The Authorization Result.</returns>
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (FhirRequirement requirement in context.PendingRequirements.OfType<FhirRequirement>().ToList())
            {
                string? resourceHDID = this.GetResourceHDID(requirement);
                if (resourceHDID != null)
                {
                    if (this.IsOwner(context, resourceHDID))
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        if (requirement.SupportsDelegation)
                        {
                            if (this.IsDelegated(context, resourceHDID, requirement))
                            {
                                context.Succeed(requirement);
                            }
                            else
                            {
                                this.logger.LogWarning($"Non-owner access to {resourceHDID} rejected");
                            }
                        }
                        else
                        {
                            this.logger.LogWarning($"Non-owner access to {resourceHDID} rejected as delegation is disabled");
                        }
                    }
                }
                else
                {
                    this.logger.LogWarning($"Fhir resource Handler has been invoked without route resource being specified, ignoring");
                }
            }

            return Task.CompletedTask;
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
            string[] acceptedScopes = new string[]
            {
                $"{type}/{FhirResource.Wildcard}.{FhirAccessType.Wildcard}",
                $"{type}/{FhirResource.Wildcard}.{requirement.AccessType}",
                $"{type}/{requirement.Resource}.{FhirAccessType.Wildcard}",
                $"{type}/{requirement.Resource}.{requirement.AccessType}",
            };
            return acceptedScopes;
        }

        private string? GetResourceHDID(FhirRequirement requirement)
        {
            string? retVal = null;
            if (requirement.Lookup == Constants.FhirResourceLookup.Route)
            {
                retVal = this.httpContextAccessor.HttpContext?.Request.RouteValues[RouteResourceIdentifier] as string;
            }
            else if (requirement.Lookup == Constants.FhirResourceLookup.Parameter)
            {
                retVal = this.httpContextAccessor.HttpContext?.Request.Query[RouteResourceIdentifier];
            }

            return retVal;
        }

        /// <summary>
        /// Check if the authenticated user is the owner of the patient resource being accessed.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHDID">The health data resource subject identifier.</param>
        private bool IsOwner(AuthorizationHandlerContext context, string resourceHDID)
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
        /// Check if the authenticated user has delegated read to the patient resource being accessed.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHDID">The health data resource subject identifier.</param>
        /// <param name="requirement">The Fhir requirement to satisfy.</param>
        private bool IsDelegated(AuthorizationHandlerContext context, string resourceHDID, FhirRequirement requirement)
        {
            bool retVal = false;
            if (context.User.HasClaim(c => c.Type == GatewayClaims.Scope))
            {
                string scopeclaim = context.User.FindFirstValue(GatewayClaims.Scope);
                string[] scopes = scopeclaim.Split(' ');
                this.logger.LogInformation($"Performing system delegation validation for resource {resourceHDID}");
                this.logger.LogInformation($"Caller has the following scopes: {scopeclaim}");
                string[] systemDelegatedScopes = GetAcceptedScopes(System, requirement);
                if (scopes.Intersect(systemDelegatedScopes).Any())
                {
                    this.logger.LogInformation($"Authorized caller as system to have {requirement.AccessType} access to resource {resourceHDID}");
                    retVal = true;
                }
                else
                {
                    switch (requirement.Resource)
                    {
                        case FhirResource.Observation:
                            retVal = this.ValidateObservationDelegate(context, resourceHDID, requirement);
                            break;
                        default:
                            this.logger.LogError($"User delegation is not implemented on resource type {requirement.Resource.GetType().Name} for Resource {resourceHDID}");
                            break;
                    }
                }
            }

            return retVal;
        }

        private bool ValidateObservationDelegate(AuthorizationHandlerContext context, string resourceHDID, FhirRequirement requirement)
        {
            bool retVal = false;
            if (this.resourceDelegateDelegate != null)
            {
                this.logger.LogInformation($"Performing user delegation validation for resource {resourceHDID}");
                string? userHDID = context.User.FindFirst(c => c.Type == GatewayClaims.HDID)?.Value;
                if (userHDID != null)
                {
                    if (this.resourceDelegateDelegate.Exists(resourceHDID, userHDID))
                    {
                        this.logger.LogInformation($"Authorized user {userHDID} to have {requirement.AccessType} access to Observation resource {resourceHDID}");
                        retVal = true;
                    }
                    else
                    {
                        this.logger.LogWarning($"Delegation validation for User {userHDID} on Observation resource {resourceHDID} failed");
                    }
                }
            }
            else
            {
                this.logger.LogError($"Performing Observation delegation on resource {resourceHDID} failed as resourceDelegateDelegate is null");
            }

            return retVal;
        }
    }
}
