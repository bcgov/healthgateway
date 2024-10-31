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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Authorizes access to FHIR resources when the user is user-delegated to access the resources.
    /// </summary>
    public class UserDelegatedAccessHandler : BaseFhirAuthorizationHandler
    {
        private readonly ILogger<UserDelegatedAccessHandler> logger;
        private readonly int? maxDependentAge;
        private readonly IPatientService patientService;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDelegatedAccessHandler"/> class.
        /// </summary>
        /// <param name="logger">the injected logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        /// <param name="patientService">The injected Patient service.</param>
        /// <param name="resourceDelegateDelegate">The ResourceDelegate delegate to interact with the DB.</param>
        public UserDelegatedAccessHandler(
            ILogger<UserDelegatedAccessHandler> logger,
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
        /// Asserts that the user accessing the resource(s) is user-delegated to access the resource(s).
        /// </summary>
        /// <param name="context">The authorization information.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task HandleAsync(AuthorizationHandlerContext context)
        {
            IEnumerable<PersonalFhirRequirement> pendingRequirements = context.PendingRequirements.OfType<PersonalFhirRequirement>();
            foreach (PersonalFhirRequirement requirement in pendingRequirements)
            {
                string? resourceHdid = HttpContextHelper.GetResourceHdid(this.HttpContextAccessor.HttpContext, requirement.SubjectLookupMethod);
                if (resourceHdid == null)
                {
                    this.logger.LogWarning("UserDelegatedAccessHandler has been invoked without patient HDID specified in the request, ignoring");
                    continue;
                }

                if (!requirement.SupportsUserDelegation)
                {
                    this.logger.LogDebug("User-delegated access to {ResourceHdid} not granted as user delegation is not supported", resourceHdid);
                    continue;
                }

                if (!await this.IsDelegatedAsync(context, resourceHdid))
                {
                    this.logger.LogDebug("User-delegated access to {ResourceHdid} not granted", resourceHdid);
                    continue;
                }

                this.logger.LogInformation("User-delegated access to {ResourceHdid} granted", resourceHdid);
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Check if the authenticated user has delegated permission to access the resource.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHdid">The health data resource subject identifier.</param>
        /// <returns>A boolean indicating if the user has delegated permission to access the resource.</returns>
        private async Task<bool> IsDelegatedAsync(AuthorizationHandlerContext context, string resourceHdid)
        {
            bool retVal = false;

            this.logger.LogDebug("Performing user delegation validation");

            string? userHdid = context.User.FindFirst(c => c.Type == GatewayClaims.Hdid)?.Value;
            if (userHdid != null)
            {
                if (await this.resourceDelegateDelegate.ExistsAsync(resourceHdid, userHdid))
                {
                    if (await this.IsExpiredAsync(resourceHdid))
                    {
                        this.logger.LogDebug("Delegation relationship exists but is expired");
                    }
                    else
                    {
                        this.logger.LogDebug("Active delegation relationship exists");
                        retVal = true;
                    }
                }
                else
                {
                    this.logger.LogDebug("Delegation relationship does not exist");
                }
            }

            return retVal;
        }

        /// <summary>
        /// Checks if the resource delegate has expired.
        /// </summary>
        /// <param name="resourceHdid">The resource hdid.</param>
        /// <returns>True if expired, false otherwise.</returns>
        private async Task<bool> IsExpiredAsync(string resourceHdid)
        {
            if (!this.maxDependentAge.HasValue)
            {
                this.logger.LogWarning("The maximum dependent age has not been configured");
                return false;
            }

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(resourceHdid);

            return patientResult.ResourcePayload!.Birthdate.AddYears(this.maxDependentAge.Value) < DateTime.Now;
        }
    }
}
