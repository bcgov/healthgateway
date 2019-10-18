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
    using Microsoft.EntityFrameworkCore;
    using HealthGateway.DIN.Models;

    /// <summary>
    /// The database context to be used for the Medication Service.
    /// </summary>
    public class MedicationDBContext : DbContext
    {
        /// <summary>
        /// The DB name for the Pharmanet Trace ID Sequence.
        /// </summary>
        public const string PHARMANET_TRACE_SEQUENCE = "trace_seq";

        /// <summary>
        /// Constructor required to instantiated the context via startup.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        public MedicationDBContext(DbContextOptions<MedicationDBContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>(PHARMANET_TRACE_SEQUENCE)
                        .StartsAt(1)
                        .IncrementsBy(1)
                        .HasMin(1)
                        .HasMax(999999)
                        .IsCyclic(true);
        }

        public DbSet<ActiveIngredient> ActiveIngredient { get; set; }
        public DbSet<Company> Company{ get; set; }
        public DbSet<DrugProduct> DrugProduct { get; set; }
        public DbSet<Form> Form { get; set; }
        public DbSet<Packaging> Packaging { get; set; }
        public DbSet<PharmaceuticalStd> PharmaceuticalStd { get; set; }
        public DbSet<Route> Route { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<TherapeuticClass> TherapeuticClass { get; set; }
        public DbSet<VeterinarySpecies> VeterinarySpecies { get; set; }
    }
}
