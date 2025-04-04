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
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A Postgres Database Audit Logger.
    /// </summary>
    public class DbAuditLogger : AuditLogger
    {
        private readonly ILogger<DbAuditLogger> logger;
        private readonly IWriteAuditEventDelegate writeEventDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbAuditLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="writeEventDelegate">The audit event delegate.</param>
        public DbAuditLogger(ILogger<DbAuditLogger> logger, IWriteAuditEventDelegate writeEventDelegate)
        {
            this.logger = logger;
            this.writeEventDelegate = writeEventDelegate;
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(DbAuditLogger).FullName);

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team Decision")]
        public override async Task WriteAuditEventAsync(AuditEvent auditEvent, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            try
            {
                await this.writeEventDelegate.WriteAuditEventAsync(auditEvent, ct);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to write audit event");
            }
        }
    }
}
