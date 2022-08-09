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
namespace HealthGateway.Common.Filters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Auditing;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// The audit middleware class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AuditFilter : IAsyncActionFilter
    {
        private readonly IAuditLogger auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditFilter"/> class.
        /// </summary>
        /// <param name="auditService">The injected audit service.</param>
        public AuditFilter(IAuditLogger auditService)
        {
            this.auditService = auditService;
        }

        /// <inheritdoc/>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            AuditEvent auditEvent = new();
            auditEvent.AuditEventDateTime = DateTime.UtcNow;

            // Executes the action (Controller method)
            await next().ConfigureAwait(true);

            // Check for ignored controllers
            if (context.Controller.GetType().GetCustomAttributes(typeof(IgnoreAuditAttribute), true).Length > 0)
            {
                return;
            }

            // Check for controller or method authorization attribute
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                bool isControllerAuthorization = context.Controller.GetType().GetCustomAttributes(inherit: true).OfType<AuthorizeAttribute>().Any();
                bool isMethodAuthorization = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true).OfType<AuthorizeAttribute>().Any();
                bool isAllowAnonymous = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true).OfType<AllowAnonymousAttribute>().Any();
                if (!(isControllerAuthorization || isMethodAuthorization) || isAllowAnonymous)
                {
                    return;
                }
            }

            auditEvent.TransactionDuration = Convert.ToInt64(DateTime.UtcNow.Subtract(auditEvent.AuditEventDateTime).TotalMilliseconds);

            // Write the event
            this.auditService.PopulateWithHttpContext(context.HttpContext, auditEvent);
            auditEvent.CreatedDateTime = DateTime.UtcNow;
            this.auditService.WriteAuditEvent(auditEvent);
        }
    }
}
