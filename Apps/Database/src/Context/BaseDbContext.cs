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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using HealthGateway.Common.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    /// <summary>
    /// The common database context to be used by all other HealthGateway contexts.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class BaseDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        protected BaseDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets the concurrency column that we use in our DB.
        /// </summary>
        protected virtual string ConcurrencyColumn => "xmin";

        /// <summary>
        /// Gets the initial seed date for loading data.
        /// The value returned is the first date of the Health Gateway Project.
        /// </summary>
        protected virtual DateTime DefaultSeedDate => Convert.ToDateTime("5/1/2019", CultureInfo.InvariantCulture);

        /// <summary>
        /// Executes a sql command.
        /// </summary>
        /// <param name="sql">The sql query script.</param>
        /// <param name="parameters">The sql query parameters.</param>
        /// <returns>The number of lines affected.</returns>
        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return this.Database.ExecuteSqlRaw(sql, parameters);
        }

        /// <inheritdoc/>
        public override int SaveChanges()
        {
            DateTime now = DateTime.UtcNow;
            IEnumerable<EntityEntry> entities = this.ChangeTracker.Entries()
                .Where(x => (x.Entity is IAuditable || x.Entity is IConcurrencyGuard) && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (EntityEntry entityEntry in entities)
            {
                if (entityEntry.Entity is IAuditable)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        // newly created records must have a createdby and date/time stamp associated to them
                        entityEntry.Property(nameof(IAuditable.CreatedDateTime)).CurrentValue = now;
                    }
                    else if (entityEntry.State == EntityState.Modified)
                    {
                        // Updated entriews cannot have thier createdBy and createdDateTime changed.
                        entityEntry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
                        entityEntry.Property(nameof(IAuditable.CreatedDateTime)).IsModified = false;
                    }

                    // set the updated by and date/time columns for both created and updated rows
                    entityEntry.Property(nameof(IAuditable.UpdatedDateTime)).CurrentValue = now;
                }

                if (entityEntry.Entity is IConcurrencyGuard &&
                    entityEntry.State == EntityState.Modified &&
                    entityEntry.Property(nameof(IConcurrencyGuard.Version)).IsModified)
                {
                    // xmin is the Postgres system column that we use for concurrency,
                    // we set the original value regardless of load state to the value we have in our object
                    // which ensures that we're only updating the row we think we have.
                    entityEntry.Property(nameof(IConcurrencyGuard.Version)).OriginalValue =
                        entityEntry.Property(nameof(IConcurrencyGuard.Version)).CurrentValue;
                }
            }

            return base.SaveChanges();
        }
    }
}
