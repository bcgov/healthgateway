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
            modelBuilder.HasDefaultSchema("gateway");

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

            modelBuilder.Entity<EmailFormatCode>()
                    .HasIndex(i => i.FormatCode)
                    .IsUnique();

            modelBuilder.Entity<AuditTransactionResultCode>()
                    .HasIndex(i => i.ResultCode)
                    .IsUnique();

            modelBuilder.Entity<FileDownload>()
                    .HasIndex(i => i.ProgramCode)
                    .IsUnique();

            // Create Foreign keys for Email
            modelBuilder.Entity<Email>()
                .HasOne<EmailFormatCode>()
                .WithMany()
                .HasPrincipalKey(k => k.FormatCode)
                .HasForeignKey(k => k.FormatCode);

            modelBuilder.Entity<EmailTemplate>()
                .HasOne<EmailFormatCode>()
                .WithMany()
                .HasPrincipalKey(k => k.FormatCode)
                .HasForeignKey(k => k.FormatCode);

            // Create Foreign keys for Audit
            modelBuilder.Entity<AuditEvent>()
                    .HasOne<ProgramTypeCode>()
                    .WithMany()
                    .HasPrincipalKey(k => k.ProgramCode)
                    .HasForeignKey(k => k.ApplicationType);

            modelBuilder.Entity<AuditEvent>()
                    .HasOne<AuditTransactionResultCode>()
                    .WithMany()
                    .HasPrincipalKey(k => k.ResultCode)
                    .HasForeignKey(k => k.TransactionResultCode)
                    .OnDelete(DeleteBehavior.Restrict);

            // Create Foreign keys for FileDownload
            modelBuilder.Entity<FileDownload>()
                    .HasOne<ProgramTypeCode>()
                    .WithMany()
                    .HasPrincipalKey(k => k.ProgramCode)
                    .HasForeignKey(k => k.ProgramCode);

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
                    Id = Guid.Parse("ea5fe1d0-2c08-4ba7-b061-dff536bb2085"),
                    ResultCode = AuditTransactionResult.Success,
                    Description = "Success",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new AuditTransactionResultCode
                {
                    Id = Guid.Parse("43710083-fdc4-4401-ab59-2de9dcc1db7c"),
                    ResultCode = AuditTransactionResult.Failure,
                    Description = "Failure",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new AuditTransactionResultCode
                {
                    Id = Guid.Parse("d25dac06-bd1e-4048-abfc-2a1f6103364b"),
                    ResultCode = AuditTransactionResult.Unauthorized,
                    Description = "Unauthorized",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new AuditTransactionResultCode
                {
                    Id = Guid.Parse("4c173b2b-1774-48fa-bc3a-6f3c9b2cd363"),
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
                    Id = Guid.Parse("38861a8b-d46d-4a05-a9b8-7f3a05790652"),
                    ProgramCode = ProgramType.FederalApproved,
                    Description = "Federal Approved Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = Guid.Parse("33e37a49-e59b-4c26-8375-66195e4e36ed"),
                    ProgramCode = ProgramType.FederalMarketed,
                    Description = "Federal Marketed Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = Guid.Parse("7e1b1f43-db55-4a0d-bc33-a53e740999d0"),
                    ProgramCode = ProgramType.FederalCancelled,
                    Description = "Federal Cancelled Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = Guid.Parse("02606989-b30b-40cd-ac51-03ca2e4e9242"),
                    ProgramCode = ProgramType.FederalDormant,
                    Description = "Federal Dormant Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = Guid.Parse("ddc05e78-c0cd-4f77-879f-d2a55b84b20a"),
                    ProgramCode = ProgramType.Provincial,
                    Description = "Provincial Pharmacare Drug Load",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = Guid.Parse("83fc7381-d57c-4c13-9173-1f0a01e0f543"),
                    ProgramCode = AuditApplication.Configuration,
                    Description = "Configuration Service",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = Guid.Parse("d1175a3e-aafb-4895-b7ee-358500775a7e"),
                    ProgramCode = AuditApplication.WebClient,
                    Description = "Web Client",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = Guid.Parse("5135cdf7-59ec-4c03-9499-38c3ccd6ecb8"),
                    ProgramCode = AuditApplication.Immunization,
                    Description = "Immunization Service",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = Guid.Parse("4a1db4fe-f91b-4902-941c-68dbadd9d243"),
                    ProgramCode = AuditApplication.Patient,
                    Description = "Patient Service",
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    Id = Guid.Parse("c0b7962a-9f66-4aa8-9864-5259bee6cedd"),
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
                    Id = Guid.Parse("d809db40-5034-4d37-8b6f-d02c17a87c8d"),
                    FormatCode = EmailFormat.Text,
                    CreatedBy = this.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = this.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new EmailFormatCode
                {
                    Id = Guid.Parse("99b4c85a-aa21-4742-9898-8172933e31a3"),
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