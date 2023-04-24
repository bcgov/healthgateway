using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthGateway.Database.Migrations
{
    /// <inheritdoc />
    public partial class TourChangeInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ApplicationSetting",
                columns: new[] { "ApplicationSettingsId", "Application", "Component", "CreatedBy", "CreatedDateTime", "Key", "UpdatedBy", "UpdatedDateTime", "Value" },
                values: new object[] { new Guid("bfcb45f6-27f9-4c0c-b494-80b147bcba8e"), "WEB", "Tour", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "latestChangeDateTime", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "2023-05-03T22:00:00.0000000Z" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "gateway",
                table: "ApplicationSetting",
                keyColumn: "ApplicationSettingsId",
                keyValue: new Guid("bfcb45f6-27f9-4c0c-b494-80b147bcba8e"));
        }
    }
}
