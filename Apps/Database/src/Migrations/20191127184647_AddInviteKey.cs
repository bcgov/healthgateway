using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class AddInviteKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteKey",
                schema: "gateway",
                table: "UserProfile",
                maxLength: 32,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteKey",
                schema: "gateway",
                table: "UserProfile");
        }
    }
}
