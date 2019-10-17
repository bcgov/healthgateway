using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Medication.Migrations
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
                name: "Companies",
                columns: table => new
                {
                    CompanyId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    DrugProductId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugCode = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_Drugs", x => x.DrugProductId);
                });

            migrationBuilder.CreateTable(
                name: "ActiveIngredients",
                columns: table => new
                {
                    ActiveIngredientId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: true),
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
                    table.PrimaryKey("PK_ActiveIngredients", x => x.ActiveIngredientId);
                    table.ForeignKey(
                        name: "FK_ActiveIngredients_Drugs_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "Drugs",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    FormId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: true),
                    PharmaceuticalFormCode = table.Column<int>(nullable: false),
                    PharmaceuticalForm = table.Column<string>(maxLength: 40, nullable: true),
                    PharmaceuticalFormFrench = table.Column<string>(maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.FormId);
                    table.ForeignKey(
                        name: "FK_Forms_Drugs_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "Drugs",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Packaging",
                columns: table => new
                {
                    PackagingId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: true),
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
                        name: "FK_Packaging_Drugs_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "Drugs",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmaceuticalStds",
                columns: table => new
                {
                    PharmaceuticalStdId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: true),
                    PharmaceuticalStdDesc = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaceuticalStds", x => x.PharmaceuticalStdId);
                    table.ForeignKey(
                        name: "FK_PharmaceuticalStds_Drugs_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "Drugs",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    RouteId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: true),
                    AdministrationCode = table.Column<int>(nullable: false),
                    Administration = table.Column<string>(maxLength: 40, nullable: true),
                    AdministrationFrench = table.Column<string>(maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_Routes_Drugs_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "Drugs",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: true),
                    ScheduleDesc = table.Column<string>(maxLength: 40, nullable: true),
                    ScheduleDescFrench = table.Column<string>(maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedules_Drugs_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "Drugs",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    StatusId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: true),
                    CurrentStatusFlag = table.Column<string>(maxLength: 1, nullable: true),
                    StatusDesc = table.Column<string>(maxLength: 40, nullable: true),
                    StatusDescFrench = table.Column<string>(maxLength: 80, nullable: true),
                    HistoryDate = table.Column<DateTime>(nullable: false),
                    LotNumber = table.Column<string>(maxLength: 80, nullable: true),
                    ExpirationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.StatusId);
                    table.ForeignKey(
                        name: "FK_Status_Drugs_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "Drugs",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TherapeuticClass",
                columns: table => new
                {
                    TherapeuticClassId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: true),
                    AtcNumber = table.Column<string>(maxLength: 8, nullable: true),
                    Atc = table.Column<string>(maxLength: 120, nullable: true),
                    AtcFrench = table.Column<string>(maxLength: 240, nullable: true),
                    Ahfs = table.Column<string>(maxLength: 80, nullable: true),
                    AhfsFrench = table.Column<string>(maxLength: 160, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapeuticClass", x => x.TherapeuticClassId);
                    table.ForeignKey(
                        name: "FK_TherapeuticClass_Drugs_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "Drugs",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VeterinarySpecies",
                columns: table => new
                {
                    VeterinarySpeciesId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    DrugProductId = table.Column<Guid>(nullable: true),
                    Species = table.Column<string>(maxLength: 80, nullable: true),
                    SpeciesFrench = table.Column<string>(maxLength: 160, nullable: true),
                    SubSpecies = table.Column<string>(maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeterinarySpecies", x => x.VeterinarySpeciesId);
                    table.ForeignKey(
                        name: "FK_VeterinarySpecies_Drugs_DrugProductId",
                        column: x => x.DrugProductId,
                        principalTable: "Drugs",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveIngredients_DrugProductId",
                table: "ActiveIngredients",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_DrugProductId",
                table: "Forms",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Packaging_DrugProductId",
                table: "Packaging",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmaceuticalStds_DrugProductId",
                table: "PharmaceuticalStds",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_DrugProductId",
                table: "Routes",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DrugProductId",
                table: "Schedules",
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
                name: "ActiveIngredients");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Forms");

            migrationBuilder.DropTable(
                name: "Packaging");

            migrationBuilder.DropTable(
                name: "PharmaceuticalStds");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropTable(
                name: "TherapeuticClass");

            migrationBuilder.DropTable(
                name: "VeterinarySpecies");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropSequence(
                name: "trace_seq");
        }
    }
}
