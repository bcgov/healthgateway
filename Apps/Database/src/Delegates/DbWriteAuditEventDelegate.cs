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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database context.</param>
    [ExcludeFromCodeCoverage]
    public class DbWriteAuditEventDelegate(ILogger<DbWriteAuditEventDelegate> logger, GatewayDbContext dbContext) : IWriteAuditEventDelegate
    {
        /// <inheritdoc/>
        public async Task WriteAuditEventAsync(AuditEvent auditEvent, CancellationToken ct = default)
        {
            logger.LogDebug("Adding audit event to DB");
            dbContext.Add(auditEvent);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
