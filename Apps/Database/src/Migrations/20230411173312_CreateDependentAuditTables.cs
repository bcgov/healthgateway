using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HealthGateway.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateDependentAuditTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DependentAuditOperationCode",
                schema: "gateway",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependentAuditOperationCode", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "DependentAudit",
                schema: "gateway",
                columns: table => new
                {
                    DependentAuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegateHdId = table.Column<string>(type: "character varying(52)", maxLength: 52, nullable: false),
                    AgentUsername = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ProtectedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OperationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependentAudit", x => x.DependentAuditId);
                    table.ForeignKey(
                        name: "FK_DependentAudit_DependentAuditOperationCode_OperationCode",
                        column: x => x.OperationCode,
                        principalSchema: "gateway",
                        principalTable: "DependentAuditOperationCode",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DependentAudit_Dependent_DelegateHdId",
                        column: x => x.DelegateHdId,
                        principalSchema: "gateway",
                        principalTable: "Dependent",
                        principalColumn: "HdId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "DependentAuditOperationCode",
                columns: new[] { "Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[,]
                {
                    { "Protect", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "Protect Dependent Operation Code", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
                    { "Unprotect", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "Unprotect Dependent Operation Code", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DependentAudit_DelegateHdId",
                schema: "gateway",
                table: "DependentAudit",
                column: "DelegateHdId");

            migrationBuilder.CreateIndex(
                name: "IX_DependentAudit_OperationCode",
                schema: "gateway",
                table: "DependentAudit",
                column: "OperationCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DependentAudit",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "DependentAuditOperationCode",
                schema: "gateway");
        }
    }
}
