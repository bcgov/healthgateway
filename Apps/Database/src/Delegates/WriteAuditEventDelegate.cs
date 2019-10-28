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
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Npgsql;
    using NpgsqlTypes;

    /// <inheritdoc/>
    public class WriteAuditEventDelegate : IWriteAuditEventDelegate
    {
        private readonly AuditDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of class <see cref="WriteAuditEventDelegate"/>.
        /// </summary>
        /// <param name="dbContext">The context to be used when accessing the database context.</param>
        public WriteAuditEventDelegate(AuditDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public void WriteAuditEvent(AuditEvent auditEvent)
        {
            this.dbContext.AuditEvent.Add(auditEvent);
            this.dbContext.SaveChanges();
        }
    }
}
