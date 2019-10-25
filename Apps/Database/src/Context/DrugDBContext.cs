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
    /// The database context to be used for the Medication Service.
    /// </summary>
    public class DrugDbContext : DbContext
    {
        /// <summary>
        /// Constructor required to instantiated the context via startup.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        public DrugDbContext(DbContextOptions<DrugDbContext> options)
            : base(options)
        {
        }

        public DbSet<ActiveIngredient> ActiveIngredient { get; set; }

        public DbSet<Company> Company { get; set; }

        public DbSet<DrugProduct> DrugProduct { get; set; }

        public DbSet<Form> Form { get; set; }

        public DbSet<Packaging> Packaging { get; set; }

        public DbSet<PharmaceuticalStd> PharmaceuticalStd { get; set; }

        public DbSet<Route> Route { get; set; }

        public DbSet<Schedule> Schedule { get; set; }

        public DbSet<Status> Status { get; set; }

        public DbSet<TherapeuticClass> TherapeuticClass { get; set; }

        public DbSet<VeterinarySpecies> VeterinarySpecies { get; set; }

        public DbSet<PharmaCareDrug> PharmaCareDrug { get; set; }

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

        public override int SaveChanges()
        {
            const string user = "DrugMaintainer";
            DateTime now = System.DateTime.UtcNow;

            IEnumerable<EntityEntry<IAuditable>> auditableEntries = this.ChangeTracker.Entries<IAuditable>()
                   .Where(x => (x.Entity is IAuditable && (x.State == EntityState.Added || x.State == EntityState.Modified)));

            foreach (EntityEntry<IAuditable> auditEntity in auditableEntries)
            {
                if (auditEntity.State == EntityState.Added)
                {
                    auditEntity.Entity.CreatedDateTime = now;
                    auditEntity.Entity.CreatedBy = user;
                }

                auditEntity.Entity.UpdatedDateTime = now;
                auditEntity.Entity.UpdatedBy = user;
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>(Sequence.PHARMANET_TRACE)
                        .StartsAt(1)
                        .IncrementsBy(1)
                        .HasMin(1)
                        .HasMax(999999)
                        .IsCyclic(true);
        }
    }
}
