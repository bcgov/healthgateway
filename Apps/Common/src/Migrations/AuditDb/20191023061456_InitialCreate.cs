using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Common.Migrations.AuditDb
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEvent",
                columns: table => new
                {
                    AuditEventId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 30, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    AuditEventDateTime = table.Column<DateTime>(nullable: false),
                    ClientIP = table.Column<string>(maxLength: 15, nullable: false),
                    ApplicationSubject = table.Column<string>(maxLength: 100, nullable: false),
                    ApplicationName = table.Column<int>(maxLength: 100, nullable: false),
                    TransacationName = table.Column<string>(maxLength: 100, nullable: false),
                    TransactionVersion = table.Column<string>(maxLength: 5, nullable: true),
                    Trace = table.Column<string>(maxLength: 20, nullable: true),
                    TransactionResult = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvent", x => x.AuditEventId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEvent");
        }
    }
}
