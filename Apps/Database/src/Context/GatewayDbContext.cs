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
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Utils;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    /// <summary>
    /// The database context used by the web client application.
    /// </summary>
    [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
    [ExcludeFromCodeCoverage]
    public class GatewayDbContext : BaseDbContext
    {
        private static readonly MethodInfo? DateTruncMethod
            = typeof(GatewayDbContext).GetRuntimeMethod(nameof(DateTrunc), new[] { typeof(string), typeof(DateTime) });

        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayDbContext"/> class.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        public GatewayDbContext(DbContextOptions<GatewayDbContext> options)
            : base(options)
        {
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
        public DbSet<MessagingVerification> MessagingVerification { get; set; } = null!;
        public DbSet<EmailTemplate> EmailTemplate { get; set; } = null!;
        public DbSet<PharmaCareDrug> PharmaCareDrug { get; set; } = null!;
        public DbSet<FileDownload> FileDownload { get; set; } = null!;
        public DbSet<UserProfile> UserProfile { get; set; } = null!;
        public DbSet<UserPreference> UserPreference { get; set; } = null!;
        public DbSet<UserFeedback> UserFeedback { get; set; } = null!;
        public DbSet<LegalAgreement> LegalAgreement { get; set; } = null!;
        public DbSet<ApplicationSetting> ApplicationSetting { get; set; } = null!;
        public DbSet<UserProfileHistory> UserProfileHistory { get; set; } = null!;
        public DbSet<Note> Note { get; set; } = null!;
        public DbSet<Comment> Comment { get; set; } = null!;
        public DbSet<Communication> Communication { get; set; } = null!;
        public DbSet<Rating> Rating { get; set; } = null!;
        public DbSet<ResourceDelegate> ResourceDelegate { get; set; } = null!;
        public DbSet<EventLog> EventLog { get; set; } = null!;
        public DbSet<AdminTag> AdminTag { get; set; } = null!;
        public DbSet<UserFeedbackTag> UserFeedbackTag { get; set; } = null!;
        public DbSet<AdminUserProfile> AdminUserProfile { get; set; } = null!;

#pragma warning restore CS1591, SA1600

        /// <summary>
        /// Provides a static mapping for the date_trunc postgres function.
        /// </summary>
        /// <param name="field">selects to which precision to truncate the input value.</param>
        /// <param name="source">datetime to be truncated.</param>
        /// <returns>A datetime that has been truncated to the field precision.</returns>
        /// <exception cref="NotSupportedException">NA.</exception>
        public static DateTime DateTrunc(string field, DateTime source)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("gateway");

            Contract.Requires(modelBuilder != null);
            modelBuilder.HasSequence<long>(Sequence.PharmanetTrace)
                .StartsAt(1)
                .IncrementsBy(1)
                .HasMin(1)
                .HasMax(999999)
                .IsCyclic();

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

            ValueConverter<EmailFormat, string> emailFormatCodeConvertor = new(
                v => EnumUtility.ToEnumString<EmailFormat>(v, false),
                v => EnumUtility.ToEnum<EmailFormat>(v, false));

            modelBuilder.Entity<Email>()
                .Property(e => e.FormatCode)
                .HasConversion(emailFormatCodeConvertor);

            modelBuilder.Entity<EmailTemplate>()
                .HasOne<EmailFormatCode>()
                .WithMany()
                .HasPrincipalKey(k => k.FormatCode)
                .HasForeignKey(k => k.FormatCode);

            modelBuilder.Entity<EmailTemplate>()
                .Property(e => e.FormatCode)
                .HasConversion(emailFormatCodeConvertor);

            modelBuilder.Entity<EmailFormatCode>()
                .Property(e => e.FormatCode)
                .HasConversion(emailFormatCodeConvertor);

            ValueConverter<EmailStatus, string> emailStatusCodeConvertor = new(
                v => EnumUtility.ToEnumString<EmailStatus>(v, false),
                v => EnumUtility.ToEnum<EmailStatus>(v, false));

            modelBuilder.Entity<Email>()
                .Property(e => e.EmailStatusCode)
                .HasConversion(emailStatusCodeConvertor);

            modelBuilder.Entity<EmailStatusCode>()
                .Property(e => e.StatusCode)
                .HasConversion(emailStatusCodeConvertor);

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

            ValueConverter<AuditTransactionResult, string> auditTransactionResultConvertor = new(
                v => EnumUtility.ToEnumString<AuditTransactionResult>(v, true),
                v => EnumUtility.ToEnum<AuditTransactionResult>(v, true));

            modelBuilder.Entity<AuditEvent>()
                .Property(e => e.TransactionResultCode)
                .HasConversion(auditTransactionResultConvertor);

            modelBuilder.Entity<AuditTransactionResultCode>()
                .Property(e => e.ResultCode)
                .HasConversion(auditTransactionResultConvertor);

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

            // Create Foreign keys for Application Settings
            modelBuilder.Entity<ApplicationSetting>()
                .HasOne<ProgramTypeCode>()
                .WithMany()
                .HasPrincipalKey(k => k.ProgramCode)
                .HasForeignKey(k => k.Application)
                .OnDelete(DeleteBehavior.Restrict);

            // Create unique index for app/component/key on app settings
            modelBuilder.Entity<ApplicationSetting>()
                .HasIndex(i => new { i.Application, i.Component, i.Key })
                .IsUnique();

            // Create Composite Key for User Notes
            modelBuilder.Entity<Note>().HasKey(k => new { k.Id, k.HdId });

            // Create Foreign keys for User Notes
            modelBuilder.Entity<Note>()
                .HasOne<UserProfile>()
                .WithMany()
                .HasPrincipalKey(k => k.HdId)
                .HasForeignKey(k => k.HdId);

            // Create Foreign keys for Messaging Verifications
            modelBuilder.Entity<MessagingVerification>()
                .HasOne<MessagingVerificationTypeCode>()
                .WithMany()
                .HasPrincipalKey(k => k.MessagingVerificationCode)
                .HasForeignKey(k => k.VerificationType)
                .OnDelete(DeleteBehavior.Restrict);

            // Create Composite Key for User Preference
            modelBuilder.Entity<UserPreference>()
                .HasKey(c => new { c.HdId, c.Preference });

            modelBuilder.Entity<Communication>()
                .HasOne<CommunicationTypeCode>()
                .WithMany()
                .HasPrincipalKey(k => k.StatusCode)
                .HasForeignKey(k => k.CommunicationTypeCode);

            modelBuilder.Entity<Communication>()
                .HasOne<CommunicationStatusCode>()
                .WithMany()
                .HasPrincipalKey(k => k.StatusCode)
                .HasForeignKey(k => k.CommunicationStatusCode);

            ValueConverter<CommunicationStatus, string> communicationStatusCodeConverter = new(
                v => EnumUtility.ToEnumString<CommunicationStatus>(v, false),
                v => EnumUtility.ToEnum<CommunicationStatus>(v, false));

            modelBuilder.Entity<Communication>()
                .Property(e => e.CommunicationStatusCode)
                .HasConversion(communicationStatusCodeConverter);

            modelBuilder.Entity<CommunicationStatusCode>()
                .Property(e => e.StatusCode)
                .HasConversion(communicationStatusCodeConverter);

            ValueConverter<CommunicationType, string> communicationTypeCodeConverter = new(
                v => EnumUtility.ToEnumString<CommunicationType>(v, false),
                v => EnumUtility.ToEnum<CommunicationType>(v, false));

            modelBuilder.Entity<Communication>()
                .Property(e => e.CommunicationTypeCode)
                .HasConversion(communicationTypeCodeConverter);

            modelBuilder.Entity<CommunicationTypeCode>()
                .Property(e => e.StatusCode)
                .HasConversion(communicationTypeCodeConverter);

            modelBuilder.Entity<LegalAgreementTypeCode>()
                .Property(e => e.LegalAgreementCode)
                .HasConversion(
                    new ValueConverter<LegalAgreementType, string>(
                        v => EnumUtility.ToEnumString<LegalAgreementType>(v, true),
                        v => EnumUtility.ToEnum<LegalAgreementType>(v, true)));

            // Resource Delegate Models

            // Create ResourceDelegate
            modelBuilder.Entity<ResourceDelegate>()
                .HasKey(resourceDelegate => new { resourceDelegate.ResourceOwnerHdid, resourceDelegate.ProfileHdid, resourceDelegate.ReasonCode });

            // Create FK keys
            modelBuilder.Entity<ResourceDelegate>()
                .HasOne<ResourceDelegateReasonCode>()
                .WithMany()
                .HasPrincipalKey(k => k.ReasonTypeCode)
                .HasForeignKey(k => k.ReasonCode);

            modelBuilder.Entity<ResourceDelegate>()
                .HasOne<UserProfile>()
                .WithMany()
                .HasPrincipalKey(k => k.HdId)
                .HasForeignKey(k => k.ProfileHdid);

            ValueConverter<ResourceDelegateReason, string> resourceDelegateReasonCodeConverter = new(
                v => EnumUtility.ToEnumString<ResourceDelegateReason>(v, false),
                v => EnumUtility.ToEnum<ResourceDelegateReason>(v, false));

            modelBuilder.Entity<ResourceDelegate>()
                .Property(e => e.ReasonCode)
                .HasConversion(resourceDelegateReasonCodeConverter);

            modelBuilder.Entity<ResourceDelegateReasonCode>()
                .Property(e => e.ReasonTypeCode)
                .HasConversion(resourceDelegateReasonCodeConverter);

            modelBuilder.Entity<ResourceDelegateHistory>()
                .Property(e => e.ReasonCode)
                .HasConversion(resourceDelegateReasonCodeConverter);

            // Create Foreign keys for Comment
            modelBuilder.Entity<Comment>()
                .HasOne<CommentEntryTypeCode>()
                .WithMany()
                .HasPrincipalKey(k => k.CommentEntryCode)
                .HasForeignKey(k => k.EntryTypeCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Create unique keys for UserFeedbackTag
            modelBuilder.Entity<UserFeedbackTag>()
                .HasIndex(p => new { p.AdminTagId, p.UserFeedbackId })
                .IsUnique();

            // Create unique keys for AdminTag
            modelBuilder.Entity<AdminTag>()
                .HasIndex(p => p.Name)
                .IsUnique();

            // Create non-unique key for UserProfileHistory
            modelBuilder.Entity<UserProfileHistory>()
                .HasIndex(p => p.HdId);

            // Create unique key for AdminUserProfile
            modelBuilder.Entity<AdminUserProfile>()
                .HasIndex(p => p.Username)
                .IsUnique();

            modelBuilder.HasDbFunction(DateTruncMethod).HasName("date_trunc");

            // Initial seed data
            this.SeedProgramTypes(modelBuilder);
            this.SeedEmail(modelBuilder);
            this.SeedAuditTransactionResults(modelBuilder);
            this.SeedLegalAgreements(modelBuilder);
            this.SeedMessagingVerifications(modelBuilder);
            this.SeedCommunication(modelBuilder);
            this.SeedResourceDelegateReason(modelBuilder);
            this.SeedCommentEntryTypeCode(modelBuilder);
        }

        /// <summary>
        /// Reads a resource file into a string.
        /// </summary>
        /// <param name="resource">
        /// The fully qualified resource to read ie
        /// "HealthGateway.Database.Assets.Legal.TermsOfService.txt".
        /// </param>
        /// <returns>The contents of the file read.</returns>
        private static string ReadResource(string resource)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(GatewayDbContext));
            Stream? resourceStream = assembly!.GetManifestResourceStream(resource);
            using StreamReader reader = new(resourceStream!, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private void SeedAuditTransactionResults(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditTransactionResultCode>()
                .HasData(
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
            modelBuilder.Entity<ProgramTypeCode>()
                .HasData(
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
                        ProgramCode = ApplicationType.Configuration,
                        Description = "Configuration Service",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new ProgramTypeCode
                    {
                        ProgramCode = ApplicationType.WebClient,
                        Description = "Web Client",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new ProgramTypeCode
                    {
                        ProgramCode = ApplicationType.Immunization,
                        Description = "Immunization Service",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new ProgramTypeCode
                    {
                        ProgramCode = ApplicationType.Patient,
                        Description = "Patient Service",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new ProgramTypeCode
                    {
                        ProgramCode = ApplicationType.Medication,
                        Description = "Medication Service",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new ProgramTypeCode
                    {
                        ProgramCode = ApplicationType.Laboratory,
                        Description = "Laboratory Service",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new ProgramTypeCode
                    {
                        ProgramCode = ApplicationType.AdminWebClient,
                        Description = "Admin Client",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new ProgramTypeCode
                    {
                        ProgramCode = ApplicationType.Encounter,
                        Description = "Encounter Service",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new ProgramTypeCode
                    {
                        ProgramCode = ApplicationType.JobScheduler,
                        Description = "Job Scheduler",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new ProgramTypeCode
                    {
                        ProgramCode = ApplicationType.ClinicalDocument,
                        Description = "Clinical Document Service",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate.ToUniversalTime(),
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate.ToUniversalTime(),
                    });
        }

        /// <summary>
        /// Seeds the Email Format codes and the Email templates.
        /// </summary>
        /// <param name="modelBuilder">The passed in model builder.</param>
        private void SeedEmail(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailFormatCode>()
                .HasData(
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
            modelBuilder.Entity<EmailStatusCode>()
                .HasData(
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
            modelBuilder.Entity<EmailTemplate>()
                .HasData(
                    new EmailTemplate
                    {
                        Id = Guid.Parse("040c2ec3-d6c0-4199-9e4b-ebe6da48d52a"),
                        Name = "Registration",
                        From = "HG_Donotreply@gov.bc.ca",
                        Subject = "Health Gateway Email Verification ${Environment}",
                        Body = ReadResource("HealthGateway.Database.Assets.docs.EmailValidationTemplate.html"),
                        Priority = EmailPriority.Standard,
                        EffectiveDate = this.DefaultSeedDate,
                        FormatCode = EmailFormat.HTML,
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new EmailTemplate
                    {
                        Id = Guid.Parse("79503a38-c14a-4992-b2fe-5586629f552e"),
                        Name = "AccountClosed",
                        From = "HG_Donotreply@gov.bc.ca",
                        Subject = "Health Gateway Account Closed ",
                        Body = ReadResource("HealthGateway.Database.Assets.docs.EmailAccountClosed.html"),
                        Priority = EmailPriority.Low,
                        EffectiveDate = this.DefaultSeedDate,
                        FormatCode = EmailFormat.HTML,
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new EmailTemplate
                    {
                        Id = Guid.Parse("2fe8c825-d4de-4884-be6a-01a97b466425"),
                        Name = "AccountRecovered",
                        From = "HG_Donotreply@gov.bc.ca",
                        Subject = "Health Gateway Account Recovered",
                        Body = ReadResource("HealthGateway.Database.Assets.docs.EmailAccountRecovered.html"),
                        Priority = EmailPriority.Low,
                        EffectiveDate = this.DefaultSeedDate,
                        FormatCode = EmailFormat.HTML,
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new EmailTemplate
                    {
                        Id = Guid.Parse("d9898318-4e53-4074-9979-5d24bd370055"),
                        Name = "AccountRemoved",
                        From = "HG_Donotreply@gov.bc.ca",
                        Subject = "Health Gateway Account Closure Complete",
                        Body = ReadResource("HealthGateway.Database.Assets.docs.EmailAccountRemoved.html"),
                        Priority = EmailPriority.Low,
                        EffectiveDate = this.DefaultSeedDate,
                        FormatCode = EmailFormat.HTML,
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new EmailTemplate
                    {
                        Id = Guid.Parse("75c79b3e-1a61-403b-82ee-fddcda7144af"),
                        Name = "AdminFeedback",
                        From = "HG_Donotreply@gov.bc.ca",
                        Subject = "Health Gateway Feedback Received",
                        Body = ReadResource("HealthGateway.Database.Assets.docs.AdminFeedback.html"),
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
            modelBuilder.Entity<LegalAgreementTypeCode>()
                .HasData(
                    new LegalAgreementTypeCode
                    {
                        LegalAgreementCode = LegalAgreementType.TermsOfService,
                        Description = "Terms of Service",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    });
            modelBuilder.Entity<LegalAgreement>()
                .HasData(
                    new LegalAgreement // Terms of Service as of Launch
                    {
                        Id = Guid.Parse("f5acf1de-2f5f-431e-955d-a837d5854182"),
                        LegalAgreementCode = LegalAgreementType.TermsOfService,
                        LegalText = ReadResource("HealthGateway.Database.Assets.Legal.TermsOfService.20191206.html"),
                        EffectiveDate = this.DefaultSeedDate,
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new LegalAgreement // Updated Terms of Service for Notes feature
                    {
                        Id = Guid.Parse("ec438d12-f8e2-4719-8444-28e35d34674c"),
                        LegalAgreementCode = LegalAgreementType.TermsOfService,
                        LegalText = ReadResource("HealthGateway.Database.Assets.Legal.TermsOfService.20200317.html"),
                        EffectiveDate = DateTime.ParseExact("03/18/2020", "MM/dd/yyyy", CultureInfo.InvariantCulture),
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = DateTime.ParseExact("03/18/2020", "MM/dd/yyyy", CultureInfo.InvariantCulture),
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = DateTime.ParseExact("03/18/2020", "MM/dd/yyyy", CultureInfo.InvariantCulture),
                    },
                    new LegalAgreement // Updated Terms of Service for Lab/Covid Update
                    {
                        Id = Guid.Parse("1d94c170-5118-4aa6-ba31-e3e07274ccbd"),
                        LegalAgreementCode = LegalAgreementType.TermsOfService,
                        LegalText = ReadResource("HealthGateway.Database.Assets.Legal.TermsOfService.20200511.html"),
                        EffectiveDate = DateTime.ParseExact("07/31/2020", "MM/dd/yyyy", CultureInfo.InvariantCulture),
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = DateTime.ParseExact("06/22/2020", "MM/dd/yyyy", CultureInfo.InvariantCulture),
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = DateTime.ParseExact("06/22/2020", "MM/dd/yyyy", CultureInfo.InvariantCulture),
                    },
                    new LegalAgreement // Updated Terms of Service for Dependents
                    {
                        Id = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd"),
                        LegalAgreementCode = LegalAgreementType.TermsOfService,
                        LegalText = ReadResource("HealthGateway.Database.Assets.Legal.TermsOfService.20201224.html"),
                        EffectiveDate = DateTime.ParseExact("01/07/2021", "MM/dd/yyyy", CultureInfo.InvariantCulture),
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = DateTime.ParseExact("12/24/2020", "MM/dd/yyyy", CultureInfo.InvariantCulture),
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = DateTime.ParseExact("01/06/2021", "MM/dd/yyyy", CultureInfo.InvariantCulture),
                    },
                    new LegalAgreement // Updated Terms of Service Fix Contact
                    {
                        Id = Guid.Parse("eafeee76-8a64-49ee-81ba-ddfe2c01deb8"),
                        LegalAgreementCode = LegalAgreementType.TermsOfService,
                        LegalText = ReadResource("HealthGateway.Database.Assets.Legal.TermsOfService.20220519.html"),
                        EffectiveDate = DateTime.ParseExact("05/19/2022 Z", "MM/dd/yyyy K", CultureInfo.InvariantCulture).ToUniversalTime(),
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = DateTime.ParseExact("05/19/2022 Z", "MM/dd/yyyy K", CultureInfo.InvariantCulture).ToUniversalTime(),
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = DateTime.ParseExact("05/19/2022 Z", "MM/dd/yyyy K", CultureInfo.InvariantCulture).ToUniversalTime(),
                    },
                    new LegalAgreement // Revamped Terms of Service
                    {
                        Id = Guid.Parse("2fab66e7-37c9-4b03-ba25-e8fad604dc7f"),
                        LegalAgreementCode = LegalAgreementType.TermsOfService,
                        LegalText = ReadResource("HealthGateway.Database.Assets.Legal.TermsOfService.20220607.html"),
                        EffectiveDate = DateTime.ParseExact("06/07/2022 Z", "MM/dd/yyyy K", CultureInfo.InvariantCulture).ToUniversalTime(),
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = DateTime.ParseExact("06/07/2022 Z", "MM/dd/yyyy K", CultureInfo.InvariantCulture).ToUniversalTime(),
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = DateTime.ParseExact("06/07/2022 Z", "MM/dd/yyyy K", CultureInfo.InvariantCulture).ToUniversalTime(),
                    });
        }

        /// <summary>
        /// Seeds the Messaging Verification Codes.
        /// </summary>
        /// <param name="modelBuilder">The passed in model builder.</param>
        private void SeedMessagingVerifications(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessagingVerificationTypeCode>()
                .HasData(
                    new MessagingVerificationTypeCode
                    {
                        MessagingVerificationCode = MessagingVerificationType.Email,
                        Description = "Email Verification Type Code",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new MessagingVerificationTypeCode
                    {
                        MessagingVerificationCode = MessagingVerificationType.Sms,
                        Description = "SMS Verification Type Code",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    });
        }

        /// <summary>
        /// Seeds the Communication Status and Communication Type codes.
        /// </summary>
        /// <param name="modelBuilder">The passed in model builder.</param>
        private void SeedCommunication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommunicationTypeCode>()
                .HasData(
                    new CommunicationTypeCode
                    {
                        StatusCode = CommunicationType.Banner,
                        Description = "Banner communication type",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommunicationTypeCode
                    {
                        StatusCode = CommunicationType.InApp,
                        Description = "In-App communication type",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommunicationTypeCode
                    {
                        StatusCode = CommunicationType.Mobile,
                        Description = "Mobile communication type",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate.ToUniversalTime(),
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate.ToUniversalTime(),
                    });

            modelBuilder.Entity<CommunicationStatusCode>()
                .HasData(
                    new CommunicationStatusCode
                    {
                        StatusCode = CommunicationStatus.New,
                        Description = "A newly created Communication",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommunicationStatusCode
                    {
                        StatusCode = CommunicationStatus.Pending,
                        Description = "A Communication pending batch pickup",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommunicationStatusCode
                    {
                        StatusCode = CommunicationStatus.Processing,
                        Description = "Communication is being processed",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommunicationStatusCode
                    {
                        StatusCode = CommunicationStatus.Processed,
                        Description = "A Communication which has been sent",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommunicationStatusCode
                    {
                        StatusCode = CommunicationStatus.Error,
                        Description = "A Communication that will not be sent",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommunicationStatusCode
                    {
                        StatusCode = CommunicationStatus.Draft,
                        Description = "A draft Communication which has not been published",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    });
        }

        /// <summary>
        /// Seeds the ResourceDelegateReason codes.
        /// </summary>
        /// <param name="modelBuilder">The passed in model builder.</param>
        private void SeedResourceDelegateReason(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceDelegateReasonCode>()
                .HasData(
                    new ResourceDelegateReasonCode
                    {
                        ReasonTypeCode = ResourceDelegateReason.COVIDLab,
                        Description = "Resource Delegation for Covid Laboratory",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    });
        }

        /// <summary>
        /// Seeds the CommentEntryType codes.
        /// </summary>
        /// <param name="modelBuilder">The passed in model builder.</param>
        private void SeedCommentEntryTypeCode(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommentEntryTypeCode>()
                .HasData(
                    new CommentEntryTypeCode
                    {
                        CommentEntryCode = CommentEntryType.None,
                        Description = "Comment for an Unknown type Entry",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommentEntryTypeCode
                    {
                        CommentEntryCode = CommentEntryType.Medication,
                        Description = "Comment for a Medication Entry",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommentEntryTypeCode
                    {
                        CommentEntryCode = CommentEntryType.Immunization,
                        Description = "Comment for an Immunization Entry",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommentEntryTypeCode
                    {
                        CommentEntryCode = CommentEntryType.Covid19Laboratory,
                        Description = "Comment for a Covid 19 Laboratory Entry",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommentEntryTypeCode
                    {
                        CommentEntryCode = CommentEntryType.Laboratory,
                        Description = "Comment for a Laboratory Entry",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommentEntryTypeCode
                    {
                        CommentEntryCode = CommentEntryType.Encounter,
                        Description = "Comment for an Encounter Entry",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate,
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate,
                    },
                    new CommentEntryTypeCode
                    {
                        CommentEntryCode = CommentEntryType.ClinicalDocuments,
                        Description = "Comment for Clinical Documents Entry",
                        CreatedBy = UserId.DefaultUser,
                        CreatedDateTime = this.DefaultSeedDate.ToUniversalTime(),
                        UpdatedBy = UserId.DefaultUser,
                        UpdatedDateTime = this.DefaultSeedDate.ToUniversalTime(),
                    });
        }
    }
}
