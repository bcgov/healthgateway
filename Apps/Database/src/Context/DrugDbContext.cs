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
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    /// <summary>
    /// The database context to be used for accessing the Federal and Provincial drug files.
    /// </summary>
    public class DrugDbContext : BaseDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrugDbContext"/> class.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        public DrugDbContext(DbContextOptions<DrugDbContext> options)
            : base(options)
        {
        }

#pragma warning disable CS1591 // These are self explanatory.
#pragma warning disable SA1600 // These are self explanatory.
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

        public DbSet<FileDownload> FileDownload { get; set; }

#pragma warning restore CS1591 //Turn XML documentation back on.

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Contract.Requires(modelBuilder != null);

            modelBuilder.HasSequence<long>(Sequence.PHARMANET_TRACE)
                        .StartsAt(1)
                        .IncrementsBy(1)
                        .HasMin(1)
                        .HasMax(999999)
                        .IsCyclic(true);

            // Create the unique index for the SHA256 hash
            modelBuilder.Entity<FileDownload>()
                    .HasIndex(f => f.Hash)
                    .IsUnique();

            modelBuilder.Entity<ProgramTypeCode>().HasData(
                new ProgramTypeCode
                {
                    Id = ProgramType.FederalApproved,
                    Name = "FederalApproved",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = ProgramType.FederalMarketed,
                    Name = "FederalMarketed",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = ProgramType.FederalCancelled,
                    Name = "FederalCancelled",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = ProgramType.FederalDormant,
                    Name = "FederalDormant",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = ProgramType.Provincial,
                    Name = "Provincial",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
        }
    }
}
