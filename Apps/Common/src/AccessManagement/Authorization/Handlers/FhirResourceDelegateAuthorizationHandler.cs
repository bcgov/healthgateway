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
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// FhirResourceAuthorizationHandler validates that a FhirRequirement with User Delegation has been met.
    /// </summary>
    public class FhirResourceDelegateAuthorizationHandler : BaseFhirAuthorizationHandler
    {
        private readonly ILogger<FhirResourceDelegateAuthorizationHandler> logger;
        private readonly int? maxDependentAge;
        private readonly IPatientService patientService;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="FhirResourceDelegateAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">the injected logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        /// <param name="patientService">The injected Patient service.</param>
        /// <param name="resourceDelegateDelegate">The ResourceDelegate delegate to interact with the DB.</param>
        public FhirResourceDelegateAuthorizationHandler(
            ILogger<FhirResourceDelegateAuthorizationHandler> logger,
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
                string? resourceHdid = this.GetResourceHdid(requirement);
                if (resourceHdid != null)
                {
                    if (requirement.SupportsUserDelegation)
                    {
                        if (this.IsDelegated(context, resourceHdid, requirement))
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            this.logger.LogDebug("Non-owner access to {ResourceHdid} rejected", resourceHdid);
                        }
                    }
                    else
                    {
                        this.logger.LogDebug("Non-owner access to {ResourceHdid} rejected as user delegation is disabled", resourceHdid);
                    }
                }
                else
                {
                    this.logger.LogWarning("Fhir resource Handler has been invoked without route resource being specified, ignoring");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Check if the authenticated user has delegated read to the patient resource being accessed.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHdid">The health data resource subject identifier.</param>
        /// <param name="requirement">The Fhir requirement to satisfy.</param>
        private bool IsDelegated(AuthorizationHandlerContext context, string resourceHdid, FhirRequirement requirement)
        {
            bool retVal = false;
            this.logger.LogInformation("Performing user delegation validation for resource {ResourceHdid}", resourceHdid);
            string? userHdid = context.User.FindFirst(c => c.Type == GatewayClaims.Hdid)?.Value;
            if (userHdid != null)
            {
                if (this.resourceDelegateDelegate.Exists(resourceHdid, userHdid))
                {
                    if (this.IsExpired(resourceHdid))
                    {
                        this.logger.LogError("Performing Observation delegation on resource {ResourceHdid} failed as delegation is expired.", resourceHdid);
                    }
                    else
                    {
                        this.logger.LogInformation("Authorized user {UserHdid} to have {AccessType} access to Observation resource {ResourceHdid}", userHdid, requirement.AccessType, resourceHdid);
                        retVal = true;
                    }
                }
                else
                {
                    this.logger.LogWarning("Delegation validation for User {UserHdid} on Observation resource {ResourceHdid} failed", userHdid, resourceHdid);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Checks if the resource delegate has expired.
        /// </summary>
        /// <param name="resourceHdid">The resource hdid.</param>
        /// <returns>True if expired, false otherwise.</returns>
        private bool IsExpired(string resourceHdid)
        {
            if (!this.maxDependentAge.HasValue)
            {
                this.logger.LogInformation("Delegate expired check on resource {ResourceHdid} skipped as max dependent age is null", resourceHdid);
                return false;
            }

            RequestResult<PatientModel> patientResult = Task.Run(async () => await this.patientService.GetPatient(resourceHdid).ConfigureAwait(true)).Result;

            return patientResult.ResourcePayload!.Birthdate.AddYears(this.maxDependentAge.Value) < DateTime.Now;
        }
    }
}
