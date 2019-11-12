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
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "trace_seq",
                minValue: 1L,
                maxValue: 999999L,
                cyclic: true);

            migrationBuilder.CreateTable(
                name: "ProgramTypeCode",
                columns: table => new
                {
                    ProgramTypeCodeId = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramTypeCode", x => x.ProgramTypeCodeId);
                });

            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    ScheduleId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    ScheduleDesc = table.Column<string>(maxLength: 40, nullable: true),
                    ScheduleDescFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.ScheduleId);
                });

            migrationBuilder.CreateTable(
                name: "FileDownload",
                columns: table => new
                {
                    FileDownloadId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Name = table.Column<string>(maxLength: 35, nullable: false),
                    Hash = table.Column<string>(maxLength: 44, nullable: false),
                    ProgramTypeCodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDownload", x => x.FileDownloadId);
                    table.ForeignKey(
                        name: "FK_FileDownload_ProgramTypeCode_ProgramTypeCodeId",
                        column: x => x.ProgramTypeCodeId,
                        principalTable: "ProgramTypeCode",
                        principalColumn: "ProgramTypeCodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugProduct",
                columns: table => new
                {
                    DrugProductId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    DrugCode = table.Column<string>(maxLength: 8, nullable: false),
                    ProductCategorization = table.Column<string>(maxLength: 80, nullable: true),
                    DrugClass = table.Column<string>(maxLength: 40, nullable: true),
                    DrugClassFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DrugIdentificationNumber = table.Column<string>(maxLength: 29, nullable: true),
                    BrandName = table.Column<string>(maxLength: 200, nullable: true),
                    BrandNameFrench = table.Column<string>(maxLength: 300, nullable: true),
                    Descriptor = table.Column<string>(maxLength: 150, nullable: true),
                    DescriptorFrench = table.Column<string>(maxLength: 200, nullable: true),
                    PediatricFlag = table.Column<string>(maxLength: 1, nullable: true),
                    AccessionNumber = table.Column<string>(maxLength: 5, nullable: true),
                    NumberOfAis = table.Column<string>(maxLength: 10, nullable: true),
                    LastUpdate = table.Column<DateTime>(nullable: false),
                    AiGroupNumber = table.Column<string>(maxLength: 10, nullable: true),
                    FileDownloadId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugProduct", x => x.DrugProductId);
                    table.ForeignKey(
                        name: "FK_DrugProduct_FileDownload_FileDownloadId",
                        column: x => x.FileDownloadId,
                        principalTable: "FileDownload",
                        principalColumn: "FileDownloadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmaCareDrug",
                columns: table => new
                {
                    PharmaCareDrugId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    DINPIN = table.Column<string>(maxLength: 8, nullable: false),
                    Plan = table.Column<string>(maxLength: 2, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "Date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "Date", nullable: false),
                    BenefitGroupList = table.Column<string>(maxLength: 60, nullable: true),
                    LCAIndicator = table.Column<string>(maxLength: 2, nullable: true),
                    PayGenericIndicator = table.Column<string>(maxLength: 1, nullable: true),
                    BrandName = table.Column<string>(maxLength: 80, nullable: true),
                    Manufacturer = table.Column<string>(maxLength: 6, nullable: true),
                    GenericName = table.Column<string>(maxLength: 60, nullable: true),
                    DosageForm = table.Column<string>(maxLength: 20, nullable: true),
                    TrialFlag = table.Column<string>(maxLength: 1, nullable: true),
                    MaximumPrice = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    LCAPrice = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    RDPCategory = table.Column<string>(maxLength: 4, nullable: true),
                    RDPSubCategory = table.Column<string>(maxLength: 4, nullable: true),
                    RDPPrice = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    RDPExcludedPlans = table.Column<string>(maxLength: 20, nullable: true),
                    CFRCode = table.Column<string>(maxLength: 1, nullable: true),
                    PharmaCarePlanDescription = table.Column<string>(maxLength: 80, nullable: true),
                    MaximumDaysSupply = table.Column<int>(nullable: true),
                    QuantityLimit = table.Column<int>(nullable: true),
                    FormularyListDate = table.Column<DateTime>(type: "Date", nullable: false),
                    LimitedUseFlag = table.Column<string>(maxLength: 1, nullable: true),
                    FileDownloadId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaCareDrug", x => x.PharmaCareDrugId);
                    table.ForeignKey(
                        name: "FK_PharmaCareDrug_FileDownload_FileDownloadId",
                        column: x => x.FileDownloadId,
                        principalTable: "FileDownload",
                        principalColumn: "FileDownloadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveIngredient",
                columns: table => new
                {
                    ActiveIngredientId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    ActiveIngredientCode = table.Column<int>(nullable: false),
                    Ingredient = table.Column<string>(maxLength: 240, nullable: true),
                    IngredientFrench = table.Column<string>(maxLength: 400, nullable: true),
                    IngredientSuppliedInd = table.Column<string>(maxLength: 1, nullable: true),
                    Strength = table.Column<string>(maxLength: 20, nullable: true),
                    StrengthUnit = table.Column<string>(maxLength: 40, nullable: true),
                    StrengthUnitFrench = table.Column<string>(maxLength: 80, nullable: true),
                    StrengthType = table.Column<string>(maxLength: 40, nullable: true),
                    StrengthTypeFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DosageValue = table.Column<string>(maxLength: 20, nullable: true),
                    Base = table.Column<string>(maxLength: 1, nullable: true),
                    DosageUnit = table.Column<string>(maxLength: 40, nullable: true),
                    DosageUnitFrench = table.Column<string>(maxLength: 80, nullable: true),
                    Notes = table.Column<string>(maxLength: 2000, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveIngredient", x => x.ActiveIngredientId);
                    table.ForeignKey(
                        name: "FK_ActiveIngredient_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    CompanyId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    ManufacturerCode = table.Column<string>(maxLength: 5, nullable: true),
                    CompanyCode = table.Column<int>(nullable: false),
                    CompanyName = table.Column<string>(maxLength: 80, nullable: true),
                    CompanyType = table.Column<string>(maxLength: 40, nullable: true),
                    AddressMailingFlag = table.Column<string>(maxLength: 1, nullable: true),
                    AddressBillingFlag = table.Column<string>(maxLength: 1, nullable: true),
                    AddressNotificationFlag = table.Column<string>(maxLength: 1, nullable: true),
                    AddressOther = table.Column<string>(maxLength: 1, nullable: true),
                    SuiteNumber = table.Column<string>(maxLength: 20, nullable: true),
                    StreetName = table.Column<string>(maxLength: 80, nullable: true),
                    CityName = table.Column<string>(maxLength: 60, nullable: true),
                    Province = table.Column<string>(maxLength: 40, nullable: true),
                    ProvinceFrench = table.Column<string>(maxLength: 100, nullable: true),
                    Country = table.Column<string>(maxLength: 40, nullable: true),
                    CountryFrench = table.Column<string>(maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(maxLength: 20, nullable: true),
                    PostOfficeBox = table.Column<string>(maxLength: 15, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_Company_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Form",
                columns: table => new
                {
                    FormId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    PharmaceuticalFormCode = table.Column<int>(nullable: false),
                    PharmaceuticalForm = table.Column<string>(maxLength: 40, nullable: true),
                    PharmaceuticalFormFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form", x => x.FormId);
                    table.ForeignKey(
                        name: "FK_Form_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Packaging",
                columns: table => new
                {
                    PackagingId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    UPC = table.Column<string>(maxLength: 12, nullable: true),
                    PackageType = table.Column<string>(maxLength: 40, nullable: true),
                    PackageTypeFrench = table.Column<string>(maxLength: 80, nullable: true),
                    PackageSizeUnit = table.Column<string>(maxLength: 40, nullable: true),
                    PackageSizeUnitFrench = table.Column<string>(maxLength: 80, nullable: true),
                    PackageSize = table.Column<string>(maxLength: 5, nullable: true),
                    ProductInformation = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packaging", x => x.PackagingId);
                    table.ForeignKey(
                        name: "FK_Packaging_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmaceuticalStd",
                columns: table => new
                {
                    PharmaceuticalStdId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    PharmaceuticalStdDesc = table.Column<string>(nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaceuticalStd", x => x.PharmaceuticalStdId);
                    table.ForeignKey(
                        name: "FK_PharmaceuticalStd_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Route",
                columns: table => new
                {
                    RouteId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    AdministrationCode = table.Column<int>(nullable: false),
                    Administration = table.Column<string>(maxLength: 40, nullable: true),
                    AdministrationFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Route", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_Route_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    StatusId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    CurrentStatusFlag = table.Column<string>(maxLength: 1, nullable: true),
                    StatusDesc = table.Column<string>(maxLength: 40, nullable: true),
                    StatusDescFrench = table.Column<string>(maxLength: 80, nullable: true),
                    HistoryDate = table.Column<DateTime>(nullable: true),
                    LotNumber = table.Column<string>(maxLength: 80, nullable: true),
                    ExpirationDate = table.Column<DateTime>(nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.StatusId);
                    table.ForeignKey(
                        name: "FK_Status_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TherapeuticClass",
                columns: table => new
                {
                    TherapeuticClassId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    AtcNumber = table.Column<string>(maxLength: 8, nullable: true),
                    Atc = table.Column<string>(maxLength: 120, nullable: true),
                    AtcFrench = table.Column<string>(maxLength: 240, nullable: true),
                    AhfsNumber = table.Column<string>(maxLength: 20, nullable: true),
                    Ahfs = table.Column<string>(maxLength: 80, nullable: true),
                    AhfsFrench = table.Column<string>(maxLength: 160, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapeuticClass", x => x.TherapeuticClassId);
                    table.ForeignKey(
                        name: "FK_TherapeuticClass_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VeterinarySpecies",
                columns: table => new
                {
                    VeterinarySpeciesId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Species = table.Column<string>(maxLength: 80, nullable: true),
                    SpeciesFrench = table.Column<string>(maxLength: 160, nullable: true),
                    SubSpecies = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeterinarySpecies", x => x.VeterinarySpeciesId);
                    table.ForeignKey(
                        name: "FK_VeterinarySpecies_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ProgramTypeCode",
                columns: new[] { "ProgramTypeCodeId", "CreatedBy", "CreatedDateTime", "Name", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { 105, "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FederalApproved", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "ProgramTypeCode",
                columns: new[] { "ProgramTypeCodeId", "CreatedBy", "CreatedDateTime", "Name", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { 110, "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FederalMarketed", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "ProgramTypeCode",
                columns: new[] { "ProgramTypeCodeId", "CreatedBy", "CreatedDateTime", "Name", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { 115, "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FederalCancelled", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "ProgramTypeCode",
                columns: new[] { "ProgramTypeCodeId", "CreatedBy", "CreatedDateTime", "Name", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { 120, "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FederalDormant", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "ProgramTypeCode",
                columns: new[] { "ProgramTypeCodeId", "CreatedBy", "CreatedDateTime", "Name", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { 200, "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Provincial", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveIngredient_DrugProductId",
                table: "ActiveIngredient",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_DrugProductId",
                table: "Company",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrugProduct_FileDownloadId",
                table: "DrugProduct",
                column: "FileDownloadId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDownload_Hash",
                table: "FileDownload",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileDownload_ProgramTypeCodeId",
                table: "FileDownload",
                column: "ProgramTypeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Form_DrugProductId",
                table: "Form",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packaging_DrugProductId",
                table: "Packaging",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmaCareDrug_FileDownloadId",
                table: "PharmaCareDrug",
                column: "FileDownloadId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmaceuticalStd_DrugProductId",
                table: "PharmaceuticalStd",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Route_DrugProductId",
                table: "Route",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Status_DrugProductId",
                table: "Status",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TherapeuticClass_DrugProductId",
                table: "TherapeuticClass",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VeterinarySpecies_DrugProductId",
                table: "VeterinarySpecies",
                column: "DrugProductId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveIngredient");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "Form");

            migrationBuilder.DropTable(
                name: "Packaging");

            migrationBuilder.DropTable(
                name: "PharmaCareDrug");

            migrationBuilder.DropTable(
                name: "PharmaceuticalStd");

            migrationBuilder.DropTable(
                name: "Route");

            migrationBuilder.DropTable(
                name: "Schedule");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropTable(
                name: "TherapeuticClass");

            migrationBuilder.DropTable(
                name: "VeterinarySpecies");

            migrationBuilder.DropTable(
                name: "DrugProduct");

            migrationBuilder.DropTable(
                name: "FileDownload");

            migrationBuilder.DropTable(
                name: "ProgramTypeCode");

            migrationBuilder.DropSequence(
                name: "trace_seq");
        }
    }
}
