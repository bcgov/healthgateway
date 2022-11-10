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
namespace HealthGateway.Database.Delegates
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DbWriteAuditEventDelegate : IWriteAuditEventDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbWriteAuditEventDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database context.</param>
        public DbWriteAuditEventDelegate(
            ILogger<DbWriteAuditEventDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public void WriteAuditEvent(AuditEvent auditEvent)
        {
            this.logger.LogTrace("Writing audit event to DB... {Id}", auditEvent.Id);
            this.dbContext.Add(auditEvent);
            this.dbContext.SaveChanges();
            this.logger.LogDebug("Finished writing audit event to DB... {Id}", auditEvent.Id);
        }
    }
}
