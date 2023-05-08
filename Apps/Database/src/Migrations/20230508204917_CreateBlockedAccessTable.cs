﻿// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
#pragma warning disable CS1591
// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HealthGateway.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateBlockedAccessTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Store DependentAudit data into a temporary table to be used later to populate AgentAddit
            string dependentAuditTempTable = "TempDependentAudit";
            string dependentAuditTempTableSql = @$"
                SELECT ""DependentAuditId"", ""HdId"", ""AgentUsername"", ""ProtectedReason"", ""OperationCode""||'Dependent',
                'Dependent' AS ""GroupCode"", ""TransactionDateTime"", ""CreatedBy"", ""CreatedDateTime"", ""UpdatedBy"", ""UpdatedDateTime""
                INTO TEMPORARY {dependentAuditTempTable} FROM gateway.""DependentAudit"";
                ";
            migrationBuilder.Sql(dependentAuditTempTableSql);

            migrationBuilder.DropTable(
                name: "DependentAudit",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "DependentAuditOperationCode",
                schema: "gateway");

            migrationBuilder.CreateTable(
                name: "AuditGroupCode",
                schema: "gateway",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditGroupCode", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "AuditOperationCode",
                schema: "gateway",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditOperationCode", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "BlockedAccess",
                schema: "gateway",
                columns: table => new
                {
                    Hdid = table.Column<string>(type: "character varying(52)", maxLength: 52, nullable: false),
                    DataSources = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedAccess", x => x.Hdid);
                });

            migrationBuilder.CreateTable(
                name: "AgentAudit",
                schema: "gateway",
                columns: table => new
                {
                    AgentAuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    Hdid = table.Column<string>(type: "character varying(52)", maxLength: 52, nullable: false),
                    AgentUsername = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OperationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GroupCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentAudit", x => x.AgentAuditId);
                    table.ForeignKey(
                        name: "FK_AgentAudit_AuditGroupCode_GroupCode",
                        column: x => x.GroupCode,
                        principalSchema: "gateway",
                        principalTable: "AuditGroupCode",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentAudit_AuditOperationCode_OperationCode",
                        column: x => x.OperationCode,
                        principalSchema: "gateway",
                        principalTable: "AuditOperationCode",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "AuditGroupCode",
                columns: new[] { "Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[,]
                {
                    { "BlockedAccess", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "Audit Blocked Access Group Code", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
                    { "Dependent", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "Audit Dependent Group Code", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "AuditOperationCode",
                columns: new[] { "Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[,]
                {
                    { "ChangeDataSourceAccess", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "Change Data Source Access Operation Code", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
                    { "ProtectDependent", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "Protect Dependent Operation Code", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc) },
                    { "UnprotectDependent", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc), "Unprotect Dependent Operation Code", "System", new DateTime(2019, 5, 1, 7, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentAudit_GroupCode",
                schema: "gateway",
                table: "AgentAudit",
                column: "GroupCode");

            migrationBuilder.CreateIndex(
                name: "IX_AgentAudit_OperationCode",
                schema: "gateway",
                table: "AgentAudit",
                column: "OperationCode");

            // Populate AgentAudit table with DependentAudit data stored in a temporary table
            string insertAgentAuditSql = @$"
                INSERT INTO gateway.""AgentAudit"" (""AgentAuditId"", ""Hdid"", ""AgentUsername"", ""Reason"",
                                                    ""OperationCode"", ""GroupCode"", ""TransactionDateTime"",
                                                    ""CreatedBy"", ""CreatedDateTime"", ""UpdatedBy"", ""UpdatedDateTime"")
                SELECT * FROM {dependentAuditTempTable};
                ";
            migrationBuilder.Sql(insertAgentAuditSql);

            // Drop temporary table
            string dropAgentAuditTableSql = @$"DROP TABLE {dependentAuditTempTable};";
            migrationBuilder.Sql(dropAgentAuditTableSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Store AgentAudit data into a temporary table to be used later to populate DependentAddit
            string agentAuditTempTable = "TempAgentAudit";
            string agentAuditTempTableSql = @$"
                SELECT ""AgentAuditId"", ""Hdid"", ""AgentUsername"", ""Reason"", TRIM(TRAILING 'D' FROM TRIM(TRAILING 'ependent' FROM ""OperationCode"")),
                       ""TransactionDateTime"", ""CreatedBy"", ""CreatedDateTime"", ""UpdatedBy"", ""UpdatedDateTime""
                INTO TEMPORARY {agentAuditTempTable} FROM gateway.""AgentAudit"";
                ";
            migrationBuilder.Sql(agentAuditTempTableSql);

            migrationBuilder.DropTable(
                name: "AgentAudit",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "BlockedAccess",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "AuditGroupCode",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "AuditOperationCode",
                schema: "gateway");

            migrationBuilder.CreateTable(
                name: "DependentAuditOperationCode",
                schema: "gateway",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    AgentUsername = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HdId = table.Column<string>(type: "character varying(52)", maxLength: 52, nullable: false),
                    OperationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ProtectedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "IX_DependentAudit_OperationCode",
                schema: "gateway",
                table: "DependentAudit",
                column: "OperationCode");

            // Populate DependentAudit table with AgentAudit data stored in a temporary table
            string insertDependentAuditSql = @$"
            INSERT INTO gateway.""DependentAudit"" (""DependentAuditId"", ""HdId"", ""AgentUsername"", ""ProtectedReason"",
                                                ""OperationCode"", ""TransactionDateTime"", ""CreatedBy"",
                                                ""CreatedDateTime"", ""UpdatedBy"", ""UpdatedDateTime"")
            SELECT * FROM {agentAuditTempTable};
            ";
            migrationBuilder.Sql(insertDependentAuditSql);

            // Drop temporary table
            string dropAgentAuditTableSql = @$"DROP TABLE {agentAuditTempTable};";
            migrationBuilder.Sql(dropAgentAuditTableSql);
        }
    }
}
