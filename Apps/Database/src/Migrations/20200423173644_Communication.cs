using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class Communication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Communication",
                schema: "gateway",
                columns: table => new
                {
                    CommunicationId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Text = table.Column<string>(maxLength: 1000, nullable: true),
                    Subject = table.Column<string>(maxLength: 1000, nullable: true),
                    EffectiveDateTime = table.Column<DateTime>(nullable: false),
                    ExpiryDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communication", x => x.CommunicationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Communication",
                schema: "gateway");
        }
    }
}
