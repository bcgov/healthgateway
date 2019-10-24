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
    using HealthGateway.Common.Database.Models;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// The audit middleware class.
    /// </summary>
    public class AuditMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IAuditService auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditMiddleware"/> class.
        /// </summary>
        /// <param name="auditService">The injected audit service.</param>
        /// <param name="next">The next request action.</param>
        public AuditMiddleware(IAuditService auditService, RequestDelegate next)
        {
            this.next = next;
            this.auditService = auditService;
        }

        /// <summary>
        /// The audit middleware handler method.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>An async task.</returns>
        public async Task Invoke(HttpContext context)
        {
            AuditEvent audit = new AuditEvent();
            audit.AuditEventDateTime = DateTime.UtcNow;
            audit.AuditEventId = Guid.NewGuid();

            //Continue down the Middleware pipeline, eventually returning to this class
            await this.next(context);

            this.auditService.ParseHttpContext(context, audit);
            await this.auditService.WriteAuditEvent(audit);

            return;
        }

    }
}
