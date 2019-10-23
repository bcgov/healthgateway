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
namespace HealthGateway.Medication.Database
{
    using HealthGateway.Common.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The database context to be used for the Medication Service.
    /// </summary>
    public class MedicationDBContext : DbContext, IDbContext
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// The DB name for the Pharmanet Trace ID Sequence.
        /// </summary>
        public const string PHARMANET_TRACE_SEQUENCE = "trace_seq";

        /// <summary>
        /// Constructs a dbcontext using the configuration object.
        /// </summary>
        /// <param name="configuration">The configuration options.</param>
        public MedicationDBContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Constructor required to instantiated the context via startup.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        public MedicationDBContext(DbContextOptions<MedicationDBContext> options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return this.Database.ExecuteSqlCommand(sql, parameters);
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(this.configuration.GetConnectionString("GatewayConnection"));
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>(PHARMANET_TRACE_SEQUENCE)
                        .StartsAt(1)
                        .IncrementsBy(1)
                        .HasMin(1)
                        .HasMax(999999)
                        .IsCyclic(true);
        }

        /// <summary>
        /// Accessor to the ActiveIngredient database set.
        /// </summary>
        public DbSet<ActiveIngredient> ActiveIngredient { get; set; }
        /// <summary>
        /// Accessor to the Company database set.
        /// </summary>
        public DbSet<Company> Company { get; set; }
        /// <summary>
        /// Accessor to the DrugProduct database set.
        /// </summary>
        public DbSet<DrugProduct> DrugProduct { get; set; }
        /// <summary>
        /// Accessor to the Form database set.
        /// </summary>
        public DbSet<Form> Form { get; set; }
        /// <summary>
        /// Accessor to the Packaging database set.
        /// </summary>
        public DbSet<Packaging> Packaging { get; set; }
        /// <summary>
        /// Accessor to the PharmaceuticalStd database set.
        /// </summary>
        public DbSet<PharmaceuticalStd> PharmaceuticalStd { get; set; }
        /// <summary>
        /// Accessor to the Route database set.
        /// </summary>
        public DbSet<Route> Route { get; set; }
        /// <summary>
        /// Accessor to the Schedule database set.
        /// </summary>
        public DbSet<Schedule> Schedule { get; set; }
        /// <summary>
        /// Accessor to the Status database set.
        /// </summary>
        public DbSet<Status> Status { get; set; }
        /// <summary>
        /// Accessor to the TherapeuticClass database set.
        /// </summary>
        public DbSet<TherapeuticClass> TherapeuticClass { get; set; }
        /// <summary>
        /// Accessor to the VeterinarySpecies database set.
        /// </summary>
        public DbSet<VeterinarySpecies> VeterinarySpecies { get; set; }
    }
}
