using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class PhoneBugfixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "gateway",
                table: "UserProfile");

            migrationBuilder.AddColumn<string>(
                name: "SMSNumber",
                schema: "gateway",
                table: "UserProfileHistory",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SMSNumber",
                schema: "gateway",
                table: "UserProfile",
                maxLength: 10,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SMSNumber",
                schema: "gateway",
                table: "UserProfileHistory");

            migrationBuilder.DropColumn(
                name: "SMSNumber",
                schema: "gateway",
                table: "UserProfile");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "gateway",
                table: "UserProfile",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
