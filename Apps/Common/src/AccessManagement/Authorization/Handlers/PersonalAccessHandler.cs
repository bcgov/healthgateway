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
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Authorizes access to FHIR resources when the user is the owner of the resources.
    /// </summary>
    public class PersonalAccessHandler : BaseFhirAuthorizationHandler
    {
        private readonly ILogger<PersonalAccessHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalAccessHandler"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="httpContextAccessor">The injected HttpContext accessor.</param>
        public PersonalAccessHandler(ILogger<PersonalAccessHandler> logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Asserts that the user accessing the resource(s) is the owner of the resource(s).
        /// </summary>
        /// <param name="context">The authorization information.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (PersonalFhirRequirement requirement in context.PendingRequirements.OfType<PersonalFhirRequirement>())
            {
                string? resourceHdid = this.GetResourceHdid(requirement.SubjectLookupMethod);
                if (resourceHdid == null)
                {
                    this.logger.LogWarning("PersonalAccessHandler has been invoked without patient HDID specified in the request, ignoring");
                }
                else if (this.IsOwner(context, resourceHdid))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    this.logger.LogDebug("Personal access to {ResourceHdid} rejected", resourceHdid);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Check if the authenticated user is the owner of the resource(s) being accessed.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="resourceHdid">The subject identifier for the health data resource(s).</param>
        /// <returns>True if the authenticated user is the owner of the resource(s), false otherwise.</returns>
        private bool IsOwner(AuthorizationHandlerContext context, string resourceHdid)
        {
            bool retVal = false;
            string? userHdid = context.User.FindFirst(c => c.Type == GatewayClaims.Hdid)?.Value;
            if (userHdid != null)
            {
                retVal = userHdid == resourceHdid;
                string message = $"{userHdid} is {(!retVal ? "not " : string.Empty)}the resource owner";
                this.logger.LogDebug("{Message}", message);
            }
            else
            {
                this.logger.LogDebug("Unable to validate resource owner for {ResourceHdid} as no HDID claims present", resourceHdid);
            }

            return retVal;
        }
    }
}
