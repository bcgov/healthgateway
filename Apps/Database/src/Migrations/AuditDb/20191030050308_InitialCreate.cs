//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
#pragma warning disable CS1591, SA1200, SA1600, SA1413, SA1601, CA1062
using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations.AuditDb
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
                    ApplicationSubject = table.Column<string>(maxLength: 100, nullable: true),
                    ApplicationType = table.Column<int>(maxLength: 100, nullable: false),
                    TransactionName = table.Column<string>(maxLength: 100, nullable: false),
                    TransactionVersion = table.Column<string>(maxLength: 5, nullable: true),
                    Trace = table.Column<string>(maxLength: 200, nullable: true),
                    TransactionResultType = table.Column<int>(nullable: false),
                    TransactionDuration = table.Column<long>(nullable: true)
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
