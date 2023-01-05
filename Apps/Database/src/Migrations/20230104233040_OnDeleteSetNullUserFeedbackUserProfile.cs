using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthGateway.Database.Migrations
{
    /// <inheritdoc />
    public partial class OnDeleteSetNullUserFeedbackUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFeedback_UserProfile_UserProfileId",
                schema: "gateway",
                table: "UserFeedback");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFeedback_UserProfile_UserProfileId",
                schema: "gateway",
                table: "UserFeedback",
                column: "UserProfileId",
                principalSchema: "gateway",
                principalTable: "UserProfile",
                principalColumn: "UserProfileId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFeedback_UserProfile_UserProfileId",
                schema: "gateway",
                table: "UserFeedback");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFeedback_UserProfile_UserProfileId",
                schema: "gateway",
                table: "UserFeedback",
                column: "UserProfileId",
                principalSchema: "gateway",
                principalTable: "UserProfile",
                principalColumn: "UserProfileId");
        }
    }
}
