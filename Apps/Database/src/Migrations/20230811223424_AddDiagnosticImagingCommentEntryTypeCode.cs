using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthGateway.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosticImagingCommentEntryTypeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "gateway",
                table: "CommentEntryTypeCode",
                columns: new[] { "CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "DIA", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "Comment for a Diagnostic Imaging Entry", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "gateway",
                table: "CommentEntryTypeCode",
                keyColumn: "CommentEntryCode",
                keyValue: "DIA");
        }
    }
}
