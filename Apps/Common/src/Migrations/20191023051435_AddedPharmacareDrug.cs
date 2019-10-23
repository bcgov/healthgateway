using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Common.Migrations
{
    public partial class AddedPharmacareDrug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PharmaCareDrug",
                columns: table => new
                {
                    PharmaCareDrugId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    DINPIN = table.Column<string>(maxLength: 8, nullable: false),
                    Plan = table.Column<string>(maxLength: 2, nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    BenefitGroupList = table.Column<string>(maxLength: 60, nullable: true),
                    LCAIndicator = table.Column<string>(maxLength: 2, nullable: true),
                    PayGenericIndicator = table.Column<string>(maxLength: 1, nullable: true),
                    BrandName = table.Column<string>(maxLength: 60, nullable: true),
                    GenericName = table.Column<string>(maxLength: 60, nullable: true),
                    DosageForm = table.Column<string>(maxLength: 20, nullable: true),
                    TrialFlag = table.Column<string>(maxLength: 1, nullable: true),
                    MaxPrice = table.Column<decimal>(nullable: false),
                    LCAPrice = table.Column<decimal>(nullable: false),
                    RDPCategory = table.Column<string>(maxLength: 4, nullable: true),
                    RDPSubCategory = table.Column<string>(maxLength: 4, nullable: true),
                    RDPExcludedPlans = table.Column<string>(maxLength: 20, nullable: true),
                    CanadianFederalRegulatoryCode = table.Column<string>(maxLength: 1, nullable: true),
                    PharmaCarePlanDescription = table.Column<string>(maxLength: 20, nullable: true),
                    MaximumDaysSupply = table.Column<int>(nullable: false),
                    QuantityLimit = table.Column<int>(nullable: false),
                    FormularyListDate = table.Column<DateTime>(nullable: false),
                    LimitedUseFlag = table.Column<string>(maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaCareDrug", x => x.PharmaCareDrugId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PharmaCareDrug");
        }
    }
}
