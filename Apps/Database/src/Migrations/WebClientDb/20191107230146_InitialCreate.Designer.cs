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
#pragma warning disable SA1118, SA1200, SA1205, SA1413, SA1600, SA1601, CA1062, CS1591, CA1812, CA1814
using System;
using HealthGateway.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HealthGateway.Database.Migrations.WebClientDb
{
    [DbContext(typeof(WebClientDbContext))]
    [Migration("20191107230146_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("HealthGateway.Database.Models.UserProfile", b =>
                {
                    b.Property<string>("HdId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("UserProfileId")
                        .HasMaxLength(52);

                    b.Property<bool>("AcceptedTermsOfService");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<string>("Email")
                        .HasMaxLength(254);

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<DateTime>("UpdatedDateTime");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("HdId");

                    b.ToTable("UserProfile");
                });
#pragma warning restore 612, 618
        }
    }
}
