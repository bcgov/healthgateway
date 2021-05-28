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
    /// PatientAuthorizationHandler validates that Patient requirements have been met.
    /// </summary>
    public class ApiKeyAuthorizationHandler : IAuthorizationHandler
    {
        private const string ApiKeyHeaderName = "X-API-KEY";

        private readonly ILogger<UserAuthorizationHandler> logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="logger">the injected logger.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        public ApiKeyAuthorizationHandler(ILogger<UserAuthorizationHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Asserts that the header contains the X-API-Key with the corret value.
        /// </summary>
        /// <param name="context">the AuthorizationHandlerContext context.</param>
        /// <returns>The Authorization Result.</returns>
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (ApiKeyRequirement requirement in context.PendingRequirements.OfType<ApiKeyRequirement>().ToList())
            {
                var apiKey = this.httpContextAccessor.HttpContext?.Request.Headers[ApiKeyHeaderName].FirstOrDefault();
                if (!string.IsNullOrEmpty(requirement.ApiKey))
                {
                    if (apiKey != null && apiKey == requirement.ApiKey)
                    {
                        this.logger.LogDebug("Api Key authorization successful");
                        context.Succeed(requirement);
                    }
                }
                else
                {
                    this.logger.LogCritical("Requirement Api Key Configuration is invalid, ApiKey authorization disabled");
                }
            }

            return Task.CompletedTask;
        }
    }
}
