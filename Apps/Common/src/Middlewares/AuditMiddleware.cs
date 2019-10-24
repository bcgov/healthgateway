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
namespace HealthGateway.Common.Middlewares
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.Common.Auditing;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// The audit middleware class.
    /// </summary>
    public class AuditMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditMiddleware"/> class.
        /// </summary>

        /// <param name="next">The next request action.</param>
        public AuditMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// The audit middleware handler method.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="auditService">The injected audit service.</param>
        /// <returns>An async task.</returns>
        public async Task Invoke(HttpContext context, IAuditLogger auditService)
        {
            AuditEvent auditEvent = new AuditEvent();
            auditEvent.AuditEventId = Guid.NewGuid();
            auditEvent.AuditEventDateTime = DateTime.UtcNow;

            // Continue down the Middleware pipeline, eventually returning to this class
            await this.next(context).ConfigureAwait(true);

            auditEvent.TransactionDuration = Convert.ToInt64(DateTime.UtcNow.Subtract(auditEvent.AuditEventDateTime).TotalMilliseconds);

            // Write the event
            auditService.PopulateWithHttpContext(context, auditEvent);
            auditService.WriteAuditEvent(auditEvent);
            return;
        }
    }
}
