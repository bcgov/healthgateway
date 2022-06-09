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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// UmaSharedAccessAuthorizationHandler validates that a FhirRequirement with UMA 2.0 Shared Access has been met.
    /// This is support for UMA 2.0 and implements a Policy Enforcer, aka Policy Enforcement Point or PEP.
    /// This must connect to Keycloak to exchange a resource identifier (rsid) for the URI of the protected
    /// and shared resource.  Once the URI is returned, it gets the resource owner HDID from that URI and compares
    /// it to the resourceHDID in context. 
    /// </summary>
    public class UmaSharedAccessAuthorizationHandler : BaseFhirAuthorizationHandler
    {
        private readonly ILogger<UmaSharedAccessAuthorizationHandler> logger;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly IPatientService patientService;
        private readonly int? maxDependentAge;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmaSharedAccessAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">the injected logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        /// <param name="umaService">The injected UMA 2.0 Authorization service.</param>
        public UmaSharedAccessAuthorizationHandler(
            ILogger<SharedAccessAuthorizationHandler> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IPatientService patientService,
            IResourceDelegateDelegate resourceDelegateDelegate)
            : base(logger, httpContextAccessor)
        {
            this.logger = logger;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.patientService = patientService;
            this.maxDependentAge = configuration.GetSection("Authorization").GetValue<int?>("MaxDependentAge");
        }

        /// <summary>
        /// Asserts that the user accessing the resource (hdid in route) is:
        ///     1) User Delegated to access the resource.
        /// </summary>
        /// <param name="context">the AuthorizationHandlerContext context.</param>
        /// <returns>The Authorization Result.</returns>
        public override Task HandleAsync(AuthorizationHandlerContext context)
        {
            IEnumerable<FhirRequirement> pendingRequirements =
                context.PendingRequirements.OfType<FhirRequirement>();
            foreach (FhirRequirement requirement in pendingRequirements)
            {
                string? resourceHDID = this.GetResourceHDID(requirement);
                if (resourceHDID != null)
                {
                    if (requirement.SupportsUserManagedAccess)
                    {
                        if (this.IsSharedWith(context, resourceHDID, requirement))
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            this.logger.LogWarning($"Shared access to resource owned by {resourceHDID} rejected.");
                        }
                    }
                    else
                    {
                        this.logger.LogWarning($"Shared access resource owned by {resourceHDID} rejected as UMA support for this resource not enabled");
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
        /// Check if the authenticated user has delegated read to the patient resource being accessed.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHDID">The health data resource subject identifier.</param>
        /// <param name="requirement">The Fhir requirement to satisfy.</param>
        private bool IsSharedWith(AuthorizationHandlerContext context, string resourceHDID, FhirRequirement requirement)
        {
            bool retVal = false;
            this.logger.LogInformation($"Performing UMA 2.0 user sharing check for resource {resourceHDID}");
            string? roles = context.User.FindFirst(c => c.Type == ResourcePermissionClaims.Roles)?.Value;

            string? resourceId = context.User.FindFirst(c => c.Type == ResourcePermissionClaims.ResourceId)?.Value;
            if (resourceId != null)
            {
                this.logger.LogDebug($"ResourceId in the RPT is {resourceId}.");
            }

            return retVal;
        }

        /// <summary>
        /// Checks if the resource delegate has expired.
        /// </summary>
        /// <param name="resourceHDID">The resource hdid.</param>
        /// <returns>True if expired, false otherwise.</returns>
        private bool IsExpired(string resourceHDID)
        {
            if (!this.maxDependentAge.HasValue)
            {
                this.logger.LogInformation($"Delegate expired check on resource {resourceHDID} skipped as max dependent age is null");
                return false;
            }

            RequestResult<PatientModel> patientResult = Task.Run(async () =>
            {
                return await this.patientService.GetPatient(resourceHDID, PatientIdentifierType.HDID).ConfigureAwait(true);
            }).Result;

            return patientResult.ResourcePayload!.Birthdate.AddYears(this.maxDependentAge.Value) < DateTime.Now;
        }
    }
}
