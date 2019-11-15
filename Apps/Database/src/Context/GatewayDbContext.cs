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
    using System.Diagnostics.Contracts;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The database context used by the web client application.
    /// </summary>
    public class GatewayDbContext : BaseDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayDbContext"/> class.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        public GatewayDbContext(DbContextOptions<GatewayDbContext> options)
            : base(options)
        {
        }

        #pragma warning disable CS1591, SA1516, SA1600 // Ignore docs for clarity.
        public DbSet<AuditEvent> AuditEvent { get; set; }
        public DbSet<DrugProduct> DrugProduct { get; set; }
        public DbSet<ActiveIngredient> ActiveIngredient { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Form> Form { get; set; }
        public DbSet<Packaging> Packaging { get; set; }
        public DbSet<PharmaceuticalStd> PharmaceuticalStd { get; set; }
        public DbSet<Route> Route { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<TherapeuticClass> TherapeuticClass { get; set; }
        public DbSet<VeterinarySpecies> VeterinarySpecies { get; set; }
        public DbSet<Email> Email { get; set; }
        public DbSet<EmailTemplate> EmailTemplate { get; set; }
        public DbSet<PharmaCareDrug> PharmaCareDrug { get; set; }
        public DbSet<FileDownload> FileDownload { get; set; }
        public DbSet<UserProfile> UserProfile { get; set; }
        #pragma warning restore CS1591, SA1600

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

            // Create Foreign keys for Audit
            modelBuilder.Entity<AuditEvent>()
                    .HasOne<ProgramTypeCode>()
                    .WithMany()
                    .HasForeignKey(k => k.ApplicationType);

            modelBuilder.Entity<AuditEvent>()
                    .HasOne<AuditTransactionResultCode>()
                    .WithMany()
                    .HasForeignKey(k => k.TransactionResultCode)
                    .OnDelete(DeleteBehavior.Restrict);

            // Initial seed data
            this.SeedProgramTypes(modelBuilder);
            this.SeedEmail(modelBuilder);
            this.SeedAuditTransactionResults(modelBuilder);
        }

        private void SeedAuditTransactionResults(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditTransactionResultCode>().HasData(
                new AuditTransactionResultCode
                {
                    ResultCode = AuditTransactionResult.Success,
                    Description = "Success",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new AuditTransactionResultCode
                {
                    ResultCode = AuditTransactionResult.Failure,
                    Description = "Failure",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new AuditTransactionResultCode
                {
                    ResultCode = AuditTransactionResult.Unauthorized,
                    Description = "Unauthorized",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new AuditTransactionResultCode
                {
                    ResultCode = AuditTransactionResult.SystemError,
                    Description = "System Error",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
        }

        private void SeedProgramTypes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProgramTypeCode>().HasData(
                new ProgramTypeCode
                {
                    ProgramCode = ProgramType.FederalApproved,
                    Description = "Federal Approved Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = ProgramType.FederalMarketed,
                    Description = "Federal Marketed Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = ProgramType.FederalCancelled,
                    Description = "Federal Cancelled Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = ProgramType.FederalDormant,
                    Description = "Federal Dormant Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = ProgramType.Provincial,
                    Description = "Provincial Pharmacare Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.Configuration,
                    Description = "Configuration Service",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.WebClient,
                    Description = "Web Client",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.Immunization,
                    Description = "Immunization Service",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.Patient,
                    Description = "Patient Service",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.Medication,
                    Description = "Medication Service",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
        }

        /// <summary>
        /// Seeds the Email Format codes and the Email templates.
        /// </summary>
        /// <param name="modelBuilder">The passed in model builder.</param>
        private void SeedEmail(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailFormatCode>().HasData(
                new EmailFormatCode
                {
                    FormatCode = EmailFormat.Text,
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new EmailFormatCode
                {
                    FormatCode = EmailFormat.HTML,
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });

            modelBuilder.Entity<EmailTemplate>().HasData(
                new EmailTemplate
                {
                    Id = Guid.Parse("040c2ec3-d6c0-4199-9e4b-ebe6da48d52a"),
                    Name = "Registration",
                    Subject = "{ENVIRONMENT} Health Gateway Email Verification",
                    Body = string.Join(
                                        Environment.NewLine,
                                        "<!doctype html>",
                                        "<html lang=\"en\">",
                                        "<head></head>",
                                        "<body style = \"margin:0\">",
                                        "    <table cellspacing = \"0\" align = \"left\" width = \"100%\" style = \"margin:0;color:#707070;font-family:Helvetica;font-size:12px;\">",
                                        "        <tr style = \"background:#003366;\">",
                                        "            <th width=\"45\" ></th>",
                                        "            <th width=\"450\" align=\"left\" style=\"text-align:left;\">",
                                        "                <div role=\"img\" aria - label=\"Health Gateway Logo\">",
                                        "                    <img src=\"\" alt=\"Health Gateway Logo\"/>",
                                        "                </div>",
                                        "            </th>",
                                        "            <th width=\"\"></th>",
                                        "        </tr>",
                                        "        <tr>",
                                        "            <td colspan=\"3\" height=\"20\"></td>",
                                        "        </tr>",
                                        "        <tr>",
                                        "            <td></td>",
                                        "            <td>",
                                        "                <h1 style = \"font-size:18px;\">Almost there!</h1>",
                                        "                <p>We've received a request to register your email address for a Ministry of Health Gateway account.</p>",
                                        "                <p>To activate your account, please verify your email by clicking the link:</p>",
                                        "                <a style = \"color:#1292c5;font-weight:600;\" href = \"\" > Health Gateway account verification </a>",
                                        "            </td>",
                                        "            <td></td>",
                                        "        </tr>",
                                        "    </table>",
                                        "</body>",
                                        "</html>"),
                    EffectiveDate = this.DefaultSeedDate,
                    FormatCode = EmailFormat.Text,
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
        }
    }
}