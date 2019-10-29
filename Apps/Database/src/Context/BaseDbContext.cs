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
namespace HealthGateway.Database.Context
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    /// <summary>
    /// The common database context to be used by all other HealthGateway contexts.
    /// </summary>
    public class BaseDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        public BaseDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Executes a sql command.
        /// </summary>
        /// <param name="sql">The sql query script.</param>
        /// <param name="parameters">The sql query parameters.</param>
        /// <returns>The number of lines affected.</returns>
        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return this.Database.ExecuteSqlCommand(sql, parameters);
        }

        /// <inheritdoc />
        public override int SaveChanges()
        {
            const string user = "System";
            DateTime now = System.DateTime.UtcNow;

            IEnumerable<EntityEntry<IAuditable>> auditableEntries = this.ChangeTracker.Entries<IAuditable>()
                   .Where(x => (x.Entity is IAuditable && (x.State == EntityState.Added || x.State == EntityState.Modified)));

            foreach (EntityEntry<IAuditable> auditEntity in auditableEntries)
            {
                if (auditEntity.State == EntityState.Added)
                {
                    auditEntity.Entity.CreatedDateTime = now;
                    auditEntity.Entity.CreatedBy = auditEntity.Entity.CreatedBy ?? user;
                }

                auditEntity.Entity.UpdatedDateTime = now;
                auditEntity.Entity.UpdatedBy = auditEntity.Entity.CreatedBy ?? user;
            }

            return base.SaveChanges();
        }
    }
}
