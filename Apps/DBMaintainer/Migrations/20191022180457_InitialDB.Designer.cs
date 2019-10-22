﻿// <auto-generated />
using System;
using HealthGateway.DrugMaintainer.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DBMaintainer.Migrations
{
    [DbContext(typeof(MigrationDBContext))]
    [Migration("20191022180457_InitialDB")]
    partial class InitialDB
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("Relational:Sequence:.trace_seq", "'trace_seq', '', '1', '1', '1', '999999', 'Int64', 'True'");

            modelBuilder.Entity("HealthGateway.Common.Database.Models.ActiveIngredient", b =>
                {
                    b.Property<Guid>("ActiveIngredientId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ActiveIngredientCode");

                    b.Property<string>("Base")
                        .HasMaxLength(1);

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<string>("DosageUnit")
                        .HasMaxLength(40);

                    b.Property<string>("DosageUnitFrench")
                        .HasMaxLength(80);

                    b.Property<string>("DosageValue")
                        .HasMaxLength(20);

                    b.Property<Guid>("DrugProductId");

                    b.Property<string>("Ingredient")
                        .HasMaxLength(240);

                    b.Property<string>("IngredientFrench")
                        .HasMaxLength(400);

                    b.Property<string>("IngredientSuppliedInd")
                        .HasMaxLength(1);

                    b.Property<string>("Notes")
                        .HasMaxLength(2000);

                    b.Property<string>("Strength")
                        .HasMaxLength(20);

                    b.Property<string>("StrengthType")
                        .HasMaxLength(40);

                    b.Property<string>("StrengthTypeFrench")
                        .HasMaxLength(80);

                    b.Property<string>("StrengthUnit")
                        .HasMaxLength(40);

                    b.Property<string>("StrengthUnitFrench")
                        .HasMaxLength(80);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("ActiveIngredientId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("ActiveIngredient");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Company", b =>
                {
                    b.Property<Guid>("CompanyId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AddressBillingFlag")
                        .HasMaxLength(1);

                    b.Property<string>("AddressMailingFlag")
                        .HasMaxLength(1);

                    b.Property<string>("AddressNotificationFlag")
                        .HasMaxLength(1);

                    b.Property<string>("AddressOther")
                        .HasMaxLength(1);

                    b.Property<string>("CityName")
                        .HasMaxLength(60);

                    b.Property<int>("CompanyCode");

                    b.Property<string>("CompanyName")
                        .HasMaxLength(80);

                    b.Property<string>("CompanyType")
                        .HasMaxLength(40);

                    b.Property<string>("Country")
                        .HasMaxLength(40);

                    b.Property<string>("CountryFrench")
                        .HasMaxLength(100);

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<Guid>("DrugProductId");

                    b.Property<string>("ManufacturerCode")
                        .HasMaxLength(5);

                    b.Property<string>("PostOfficeBox")
                        .HasMaxLength(15);

                    b.Property<string>("PostalCode")
                        .HasMaxLength(20);

                    b.Property<string>("Province")
                        .HasMaxLength(40);

                    b.Property<string>("ProvinceFrench")
                        .HasMaxLength(100);

                    b.Property<string>("StreetName")
                        .HasMaxLength(80);

                    b.Property<string>("SuiteNumber")
                        .HasMaxLength(20);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("CompanyId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("Company");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.DrugProduct", b =>
                {
                    b.Property<Guid>("DrugProductId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccessionNumber")
                        .HasMaxLength(5);

                    b.Property<string>("AiGroupNumber")
                        .HasMaxLength(10);

                    b.Property<string>("BrandName")
                        .HasMaxLength(200);

                    b.Property<string>("BrandNameFrench")
                        .HasMaxLength(300);

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<string>("Descriptor")
                        .HasMaxLength(150);

                    b.Property<string>("DescriptorFrench")
                        .HasMaxLength(200);

                    b.Property<string>("DrugClass")
                        .HasMaxLength(40);

                    b.Property<string>("DrugClassFrench")
                        .HasMaxLength(80);

                    b.Property<string>("DrugCode")
                        .IsRequired()
                        .HasMaxLength(8);

                    b.Property<string>("DrugIdentificationNumber")
                        .HasMaxLength(29);

                    b.Property<DateTime>("LastUpdate");

                    b.Property<string>("NumberOfAis")
                        .HasMaxLength(10);

                    b.Property<string>("PediatricFlag")
                        .HasMaxLength(1);

                    b.Property<string>("ProductCategorization")
                        .HasMaxLength(80);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("DrugProductId");

                    b.ToTable("DrugProduct");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Form", b =>
                {
                    b.Property<Guid>("FormId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<Guid>("DrugProductId");

                    b.Property<string>("PharmaceuticalForm")
                        .HasMaxLength(40);

                    b.Property<int>("PharmaceuticalFormCode");

                    b.Property<string>("PharmaceuticalFormFrench")
                        .HasMaxLength(80);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("FormId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("Form");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Packaging", b =>
                {
                    b.Property<Guid>("PackagingId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<Guid>("DrugProductId");

                    b.Property<string>("PackageSize")
                        .HasMaxLength(5);

                    b.Property<string>("PackageSizeUnit")
                        .HasMaxLength(40);

                    b.Property<string>("PackageSizeUnitFrench")
                        .HasMaxLength(80);

                    b.Property<string>("PackageType")
                        .HasMaxLength(40);

                    b.Property<string>("PackageTypeFrench")
                        .HasMaxLength(80);

                    b.Property<string>("ProductInformation")
                        .HasMaxLength(80);

                    b.Property<string>("UPC")
                        .HasMaxLength(12);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("PackagingId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("Packaging");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.PharmaceuticalStd", b =>
                {
                    b.Property<Guid>("PharmaceuticalStdId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<Guid>("DrugProductId");

                    b.Property<string>("PharmaceuticalStdDesc");

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("PharmaceuticalStdId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("PharmaceuticalStd");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Route", b =>
                {
                    b.Property<Guid>("RouteId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Administration")
                        .HasMaxLength(40);

                    b.Property<int>("AdministrationCode");

                    b.Property<string>("AdministrationFrench")
                        .HasMaxLength(80);

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<Guid>("DrugProductId");

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("RouteId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("Route");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Schedule", b =>
                {
                    b.Property<Guid>("ScheduleId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<Guid>("DrugProductId");

                    b.Property<string>("ScheduleDesc")
                        .HasMaxLength(40);

                    b.Property<string>("ScheduleDescFrench")
                        .HasMaxLength(80);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("ScheduleId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("Schedule");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Status", b =>
                {
                    b.Property<Guid>("StatusId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<string>("CurrentStatusFlag")
                        .HasMaxLength(1);

                    b.Property<Guid>("DrugProductId");

                    b.Property<DateTime?>("ExpirationDate");

                    b.Property<DateTime?>("HistoryDate");

                    b.Property<string>("LotNumber")
                        .HasMaxLength(80);

                    b.Property<string>("StatusDesc")
                        .HasMaxLength(40);

                    b.Property<string>("StatusDescFrench")
                        .HasMaxLength(80);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("StatusId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("Status");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.TherapeuticClass", b =>
                {
                    b.Property<Guid>("TherapeuticClassId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Ahfs")
                        .HasMaxLength(80);

                    b.Property<string>("AhfsFrench")
                        .HasMaxLength(160);

                    b.Property<string>("AhfsNumber")
                        .HasMaxLength(20);

                    b.Property<string>("Atc")
                        .HasMaxLength(120);

                    b.Property<string>("AtcFrench")
                        .HasMaxLength(240);

                    b.Property<string>("AtcNumber")
                        .HasMaxLength(8);

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<Guid>("DrugProductId");

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("TherapeuticClassId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("TherapeuticClass");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.VeterinarySpecies", b =>
                {
                    b.Property<Guid>("VeterinarySpeciesId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<Guid>("DrugProductId");

                    b.Property<string>("Species")
                        .HasMaxLength(80);

                    b.Property<string>("SpeciesFrench")
                        .HasMaxLength(160);

                    b.Property<string>("SubSpecies")
                        .HasMaxLength(80);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.HasKey("VeterinarySpeciesId");

                    b.HasIndex("DrugProductId");

                    b.ToTable("VeterinarySpecies");
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.ActiveIngredient", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Company", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Form", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Packaging", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.PharmaceuticalStd", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Route", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Schedule", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.Status", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.TherapeuticClass", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HealthGateway.Common.Database.Models.VeterinarySpecies", b =>
                {
                    b.HasOne("HealthGateway.Common.Database.Models.DrugProduct", "Drug")
                        .WithMany()
                        .HasForeignKey("DrugProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
