using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class AddIndexToUserProfileHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserProfileHistory_UserProfileId",
                schema: "gateway",
                table: "UserProfileHistory",
                column: "UserProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfileHistory_UserProfileId",
                schema: "gateway",
                table: "UserProfileHistory");
        }
    }
}
