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
namespace HealthGateway.Common.AccessManagement.Authorization
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// PatientAuthorizationHandler validates that Patient requirements have been met.
    /// </summary>
    public class PatientAuthorizationHandler : IAuthorizationHandler
    {
        private const string System = "system";
        private const string User = "user";

        private const string Wildcard = "*";
        private const string FhirResource = "Patient";

        private const string Read = "read";
        private const string Write = "write";

        private const string RouteResourceIdentifier = "hdid";

        private readonly ILogger<PatientAuthorizationHandler> logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">the injected logger.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        public PatientAuthorizationHandler(ILogger<PatientAuthorizationHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
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
            string? resourceHDID = this.httpContextAccessor.HttpContext.Request.RouteValues[RouteResourceIdentifier] as string;
            foreach (IAuthorizationRequirement requirement in context.PendingRequirements.ToList())
            {
                if (requirement is PatientReadRequirement || requirement is PatientWriteRequirement)
                {
                    if (resourceHDID != null)
                    {
                        if (this.IsOwner(context, resourceHDID))
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            if (requirement is PatientReadRequirement)
                            {
                                if (this.IsDelegated(context, resourceHDID, Read))
                                {
                                    context.Succeed(requirement);
                                }
                            }
                            else if (requirement is PatientWriteRequirement)
                            {
                                if (this.IsDelegated(context, resourceHDID, Write))
                                {
                                    context.Succeed(requirement);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.logger.LogInformation($"Patient Auth Handler has been invoked without route resource being specified");
                    }
                }
                else if (requirement is PatientRequirement)
                {
                    ClaimsPrincipal user = context.User;
                    if (user.HasClaim(c => c.Type == PatientClaims.Patient))
                    {
                        context.Succeed(requirement);
                    }
                }
                else
                {
                    this.logger.LogDebug($"Requirement not known to Patient Auth Handler and will be ignored");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Generates a list of valid scopes which will be used to determine authorization.
        /// No validation is done on the parameters.
        /// </summary>
        /// <param name="type">The type: System or User.</param>
        /// <param name="access">The access level read or write.</param>
        /// <returns>An array of acceptable scopes.</returns>
        private static string[] GetAcceptedScopes(string type, string access)
        {
            string[] acceptedScopes = new string[]
            {
                $"{type}/{Wildcard}.{Wildcard}",
                $"{type}/{Wildcard}.{access}",
                $"{type}/{FhirResource}.{Wildcard}",
                $"{type}/{FhirResource}.{access}",
            };
            return acceptedScopes;
        }

        /// <summary>
        /// Check if the authenticated user is the owner of the patient resource being accessed.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHDID">The health data resource subject identifier.</param>
        private bool IsOwner(AuthorizationHandlerContext context, string resourceHDID)
        {
            bool retVal = false;
            ClaimsPrincipal user = context.User;
            if (user.HasClaim(c => c.Type == PatientClaims.Patient))
            {
                string userHDID = user.FindFirst(c => c.Type == PatientClaims.Patient).Value;
                retVal = userHDID == resourceHDID;
                this.logger.LogDebug($"{userHDID} is{(!retVal ? "not " : string.Empty)}the resource owner");
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
        /// <param name="access">The access level to validate.</param>
        private bool IsDelegated(AuthorizationHandlerContext context, string resourceHDID, string access)
        {
            bool retVal = false;
            if (context.User.HasClaim(c => c.Type == GatewayClaims.Scope))
            {
                string scopeclaim = context.User.FindFirstValue(GatewayClaims.Scope);
                string[] scopes = scopeclaim.Split(' ');
                this.logger.LogInformation($"Performing system delegation validation for Patient resource {resourceHDID}");
                string[] systemDelegatedScopes = GetAcceptedScopes(System, access);
                if (scopes.Intersect(systemDelegatedScopes).Any())
                {
                    this.logger.LogInformation($"Authorized system to have {access} access to Patient resource {resourceHDID}");
                    retVal = true;
                }
                else
                {
                    this.logger.LogInformation($"Performing user delegation validation for Patient resource {resourceHDID}");
                    string[] userDelegatedScopes = GetAcceptedScopes(User, access);
                    if (context.User.HasClaim(c => c.Type == PatientClaims.Patient) && scopes.Intersect(userDelegatedScopes).Any())
                    {
                        this.logger.LogError("user delgation is not implemented, returning not authorized");

                        // TODO:  Future check needed for patient to patient delegation.
                    }
                    else
                    {
                        this.logger.LogWarning($"Patient delgation validation for Patient resource {resourceHDID} failed");
                    }
                }
            }

            return retVal;
        }
    }
}