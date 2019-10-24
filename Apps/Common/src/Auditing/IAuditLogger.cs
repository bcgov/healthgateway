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
namespace HealthGateway.Common.Auditing
{
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// The audit service interface.
    /// </summary>
    public interface IAuditLogger
    {
        /// <summary>
        /// Writes an Audit entry to the audit log
        /// </summary>
        void WriteAuditEvent(AuditEvent auditEvent);

        /// <summary>
        /// Parsers the httpcontext and populates the audit event with it values.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="audit">The audit event object to be populated.</param>
        void PopulateWithHttpContext(HttpContext context, AuditEvent audit);
    }
}