using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Common.Migrations
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
                name: "DrugProduct",
                columns: table => new
                {
                    DrugProductId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
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
                    AiGroupNumber = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugProduct", x => x.DrugProductId);
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
                    DrugProductId = table.Column<Guid>(nullable: false),
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
                    Notes = table.Column<string>(maxLength: 2000, nullable: true)
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
                    DrugProductId = table.Column<Guid>(nullable: false),
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
                    PostOfficeBox = table.Column<string>(maxLength: 15, nullable: true)
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
                    DrugProductId = table.Column<Guid>(nullable: false),
                    PharmaceuticalFormCode = table.Column<int>(nullable: false),
                    PharmaceuticalForm = table.Column<string>(maxLength: 40, nullable: true),
                    PharmaceuticalFormFrench = table.Column<string>(maxLength: 80, nullable: true)
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
                    DrugProductId = table.Column<Guid>(nullable: false),
                    UPC = table.Column<string>(maxLength: 12, nullable: true),
                    PackageType = table.Column<string>(maxLength: 40, nullable: true),
                    PackageTypeFrench = table.Column<string>(maxLength: 80, nullable: true),
                    PackageSizeUnit = table.Column<string>(maxLength: 40, nullable: true),
                    PackageSizeUnitFrench = table.Column<string>(maxLength: 80, nullable: true),
                    PackageSize = table.Column<string>(maxLength: 5, nullable: true),
                    ProductInformation = table.Column<string>(maxLength: 80, nullable: true)
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
                    DrugProductId = table.Column<Guid>(nullable: false),
                    PharmaceuticalStdDesc = table.Column<string>(nullable: true)
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
                    DrugProductId = table.Column<Guid>(nullable: false),
                    AdministrationCode = table.Column<int>(nullable: false),
                    Administration = table.Column<string>(maxLength: 40, nullable: true),
                    AdministrationFrench = table.Column<string>(maxLength: 80, nullable: true)
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
                name: "Schedule",
                columns: table => new
                {
                    ScheduleId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: false),
                    ScheduleDesc = table.Column<string>(maxLength: 40, nullable: true),
                    ScheduleDescFrench = table.Column<string>(maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedule_DrugProduct_DrugProductId",
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
                    DrugProductId = table.Column<Guid>(nullable: false),
                    CurrentStatusFlag = table.Column<string>(maxLength: 1, nullable: true),
                    StatusDesc = table.Column<string>(maxLength: 40, nullable: true),
                    StatusDescFrench = table.Column<string>(maxLength: 80, nullable: true),
                    HistoryDate = table.Column<DateTime>(nullable: true),
                    LotNumber = table.Column<string>(maxLength: 80, nullable: true),
                    ExpirationDate = table.Column<DateTime>(nullable: true)
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
                    DrugProductId = table.Column<Guid>(nullable: false),
                    AtcNumber = table.Column<string>(maxLength: 8, nullable: true),
                    Atc = table.Column<string>(maxLength: 120, nullable: true),
                    AtcFrench = table.Column<string>(maxLength: 240, nullable: true),
                    AhfsNumber = table.Column<string>(maxLength: 20, nullable: true),
                    Ahfs = table.Column<string>(maxLength: 80, nullable: true),
                    AhfsFrench = table.Column<string>(maxLength: 160, nullable: true)
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
                    DrugProductId = table.Column<Guid>(nullable: false),
                    Species = table.Column<string>(maxLength: 80, nullable: true),
                    SpeciesFrench = table.Column<string>(maxLength: 160, nullable: true),
                    SubSpecies = table.Column<string>(maxLength: 80, nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_ActiveIngredient_DrugProductId",
                table: "ActiveIngredient",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_DrugProductId",
                table: "Company",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Form_DrugProductId",
                table: "Form",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Packaging_DrugProductId",
                table: "Packaging",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmaceuticalStd_DrugProductId",
                table: "PharmaceuticalStd",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Route_DrugProductId",
                table: "Route",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_DrugProductId",
                table: "Schedule",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Status_DrugProductId",
                table: "Status",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TherapeuticClass_DrugProductId",
                table: "TherapeuticClass",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VeterinarySpecies_DrugProductId",
                table: "VeterinarySpecies",
                column: "DrugProductId");
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

            migrationBuilder.DropSequence(
                name: "trace_seq");
        }
    }
}
