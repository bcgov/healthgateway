using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations.AuditDb
{
    public partial class IncreaseTraceLenght : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Trace",
                table: "AuditEvent",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Trace",
                table: "AuditEvent",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
