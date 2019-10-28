﻿// <auto-generated />
using System;
using HealthGateway.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HealthGateway.Database.Migrations.AuditDb
{
    [DbContext(typeof(AuditDbContext))]
    partial class AuditDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("HealthGateway.Database.Models.AuditEvent", b =>
                {
                    b.Property<Guid>("AuditEventId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationSubject")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<int>("ApplicationType")
                        .HasMaxLength(100);

                    b.Property<DateTime>("AuditEventDateTime");

                    b.Property<string>("ClientIP")
                        .IsRequired()
                        .HasMaxLength(15);

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<string>("Trace")
                        .HasMaxLength(20);

                    b.Property<string>("TransacationName")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<long?>("TransactionDuration");

                    b.Property<int>("TransactionResultType");

                    b.Property<string>("TransactionVersion")
                        .HasMaxLength(5);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("AuditEventId");

                    b.ToTable("AuditEvent");
                });
#pragma warning restore 612, 618
        }
    }
}
