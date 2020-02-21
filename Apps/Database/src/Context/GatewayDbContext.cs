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
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The database context used by the web client application.
    /// </summary>
    public class GatewayDbContext : BaseDbContext
    {
        private string? emailValidationTemplate;
        private string? emailInviteTemplate;
        private string? emailBetaConfirmationTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayDbContext"/> class.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        public GatewayDbContext(DbContextOptions<GatewayDbContext> options)
            : base(options)
        {
            this.emailValidationTemplate = ReadResource("HealthGateway.Database.Assets.docs.EmailValidationTemplate.html");
            this.emailInviteTemplate = ReadResource("HealthGateway.Database.Assets.docs.EmailInviteTemplate.html");
            this.emailBetaConfirmationTemplate = ReadResource("HealthGateway.Database.Assets.docs.EmailWaitlistTemplate.html");
        }

#pragma warning disable CS1591, SA1516, SA1600 // Ignore docs for clarity.
        public DbSet<AuditEvent> AuditEvent { get; set; } = null!;
        public DbSet<DrugProduct> DrugProduct { get; set; } = null!;
        public DbSet<ActiveIngredient> ActiveIngredient { get; set; } = null!;
        public DbSet<Company> Company { get; set; } = null!;
        public DbSet<Status> Status { get; set; } = null!;
        public DbSet<Form> Form { get; set; } = null!;
        public DbSet<Packaging> Packaging { get; set; } = null!;
        public DbSet<PharmaceuticalStd> PharmaceuticalStd { get; set; } = null!;
        public DbSet<Route> Route { get; set; } = null!;
        public DbSet<Schedule> Schedule { get; set; } = null!;
        public DbSet<TherapeuticClass> TherapeuticClass { get; set; } = null!;
        public DbSet<VeterinarySpecies> VeterinarySpecies { get; set; } = null!;
        public DbSet<Email> Email { get; set; } = null!;
        public DbSet<EmailInvite> EmailInvite { get; set; } = null!;
        public DbSet<EmailTemplate> EmailTemplate { get; set; } = null!;
        public DbSet<PharmaCareDrug> PharmaCareDrug { get; set; } = null!;
        public DbSet<FileDownload> FileDownload { get; set; } = null!;
        public DbSet<UserProfile> UserProfile { get; set; } = null!;
        public DbSet<UserFeedback> UserFeedback { get; set; } = null!;
        public DbSet<BetaRequest> BetaRequest { get; set; } = null!;
        public DbSet<LegalAgreement> LegalAgreement { get; set; } = null!;
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
            modelBuilder!.Entity<FileDownload>()
                    .HasIndex(f => f.Hash)
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

            modelBuilder.Entity<Email>()
                .HasOne<EmailStatusCode>()
                .WithMany()
                .HasPrincipalKey(k => k.StatusCode)
                .HasForeignKey(k => k.EmailStatusCode);

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

            // Create Foreign keys for Legal Agreements
            modelBuilder.Entity<LegalAgreement>()
                .HasOne<LegalAgreementTypeCode>()
                .WithMany()
                .HasPrincipalKey(k => k.LegalAgreementCode)
                .HasForeignKey(k => k.LegalAgreementCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Initial seed data
            this.SeedProgramTypes(modelBuilder);
            this.SeedEmail(modelBuilder);
            this.SeedAuditTransactionResults(modelBuilder);
            this.SeedLegalAgreements(modelBuilder);
        }

        /// <summary>
        /// Reads a resource file into a string.
        /// </summary>
        /// <param name="resource">The fully qualified resource to read ie "HealthGateway.Database.Assets.Legal.TermsOfService.txt".</param>
        /// <returns>The contents of the file read.</returns>
        private static string ReadResource(string resource)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(GatewayDbContext));
            Stream? resourceStream = assembly!.GetManifestResourceStream(resource);
            using StreamReader reader = new StreamReader(resourceStream!, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private void SeedAuditTransactionResults(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditTransactionResultCode>().HasData(
                new AuditTransactionResultCode
                {
                    ResultCode = AuditTransactionResult.Success,
                    Description = "Success",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new AuditTransactionResultCode
                {
                    ResultCode = AuditTransactionResult.Failure,
                    Description = "Failure",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new AuditTransactionResultCode
                {
                    ResultCode = AuditTransactionResult.Unauthorized,
                    Description = "Unauthorized",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new AuditTransactionResultCode
                {
                    ResultCode = AuditTransactionResult.SystemError,
                    Description = "System Error",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
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
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = ProgramType.FederalMarketed,
                    Description = "Federal Marketed Drug Load",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = ProgramType.FederalCancelled,
                    Description = "Federal Cancelled Drug Load",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = ProgramType.FederalDormant,
                    Description = "Federal Dormant Drug Load",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = ProgramType.Provincial,
                    Description = "Provincial Pharmacare Drug Load",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.Configuration,
                    Description = "Configuration Service",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.WebClient,
                    Description = "Web Client",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.Immunization,
                    Description = "Immunization Service",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.Patient,
                    Description = "Patient Service",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.Medication,
                    Description = "Medication Service",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new ProgramTypeCode
                {
                    ProgramCode = AuditApplication.AdminWebClient,
                    Description = "Admin Client",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
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
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new EmailFormatCode
                {
                    FormatCode = EmailFormat.HTML,
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
            modelBuilder.Entity<EmailStatusCode>().HasData(
                new EmailStatusCode
                {
                    StatusCode = EmailStatus.New,
                    Description = "A newly created email",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new EmailStatusCode
                {
                    StatusCode = EmailStatus.Pending,
                    Description = "An email pending batch pickup",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new EmailStatusCode
                {
                    StatusCode = EmailStatus.Processed,
                    Description = "An email that has been sent",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                },
                new EmailStatusCode
                {
                    StatusCode = EmailStatus.Error,
                    Description = "An Email that will not be sent",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
            modelBuilder.Entity<EmailTemplate>().HasData(
                new EmailTemplate
                {
                    Id = Guid.Parse("040c2ec3-d6c0-4199-9e4b-ebe6da48d52a"),
                    Name = "Registration",
                    From = "HG_Donotreply@gov.bc.ca",
                    Subject = "Health Gateway Email Verification ${Environment}",
                    Body = this.emailValidationTemplate,
                    Priority = EmailPriority.Standard,
                    EffectiveDate = this.DefaultSeedDate,
                    FormatCode = EmailFormat.HTML,
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
            modelBuilder.Entity<EmailTemplate>().HasData(
                new EmailTemplate
                {
                    Id = Guid.Parse("896f8f2e-3bed-400b-acaf-51dd6082b4bd"),
                    Name = "Invite",
                    From = "HG_Donotreply@gov.bc.ca",
                    Subject = "Health Gateway Private Invitation",
                    Body = this.emailInviteTemplate,
                    Priority = EmailPriority.Low,
                    EffectiveDate = this.DefaultSeedDate,
                    FormatCode = EmailFormat.HTML,
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
            modelBuilder.Entity<EmailTemplate>().HasData(
                new EmailTemplate
                {
                    Id = Guid.Parse("2ab5d4aa-c4c9-4324-a753-cde4e21e7612"),
                    Name = "BetaConfirmation",
                    From = "HG_Donotreply@gov.bc.ca",
                    Subject = "Health Gateway Waitlist Confirmation",
                    Body = this.emailBetaConfirmationTemplate,
                    Priority = EmailPriority.Low,
                    EffectiveDate = this.DefaultSeedDate,
                    FormatCode = EmailFormat.HTML,
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
        }

        /// <summary>
        /// Seeds the Legal Agreement types and the agreements.
        /// </summary>
        /// <param name="modelBuilder">The passed in model builder.</param>
        private void SeedLegalAgreements(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LegalAgreementTypeCode>().HasData(
                new LegalAgreementTypeCode
                {
                    LegalAgreementCode = AgreementType.TermsofService,
                    Description = "Terms of Service",
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
            modelBuilder.Entity<LegalAgreement>().HasData(
                new LegalAgreement // Terms of Service as of Launch
                {
                    Id = Guid.Parse("f5acf1de-2f5f-431e-955d-a837d5854182"),
                    LegalAgreementCode = AgreementType.TermsofService,
                    LegalText = ReadResource("HealthGateway.Database.Assets.Legal.TermsOfService.20191206.html"),
                    EffectiveDate = Convert.ToDateTime("2019/12/06", CultureInfo.InvariantCulture),
                    CreatedBy = UserId.DefaultUser,
                    CreatedDateTime = this.DefaultSeedDate,
                    UpdatedBy = UserId.DefaultUser,
                    UpdatedDateTime = this.DefaultSeedDate,
                });
        }
    }
}