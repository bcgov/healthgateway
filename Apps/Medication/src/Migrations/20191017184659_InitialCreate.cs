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
                name: "Drugs",
                columns: table => new
                {
                    DrugProductId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    UpdatedDateTime = table.Column<string>(nullable: false),
                    ProductCategorization = table.Column<string>(maxLength: 80, nullable: true),
                    DrugClass = table.Column<string>(maxLength: 40, nullable: true),
                    DrugIdentificationNumber = table.Column<string>(maxLength: 29, nullable: true),
                    BrandName = table.Column<string>(maxLength: 200, nullable: true),
                    Descriptor = table.Column<string>(maxLength: 150, nullable: true),
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
                    IngredientSuppliedInd = table.Column<string>(maxLength: 1, nullable: true),
                    Strength = table.Column<string>(maxLength: 20, nullable: true),
                    StrengthUnit = table.Column<string>(maxLength: 40, nullable: true),
                    StrengthType = table.Column<string>(maxLength: 40, nullable: true),
                    DosageValue = table.Column<string>(maxLength: 20, nullable: true),
                    Base = table.Column<string>(maxLength: 1, nullable: true),
                    DosageUnit = table.Column<string>(maxLength: 40, nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_ActiveIngredients_DrugProductId",
                table: "ActiveIngredients",
                column: "DrugProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveIngredients");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropSequence(
                name: "trace_seq");
        }
    }
}
