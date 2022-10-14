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
// <auto-generated />
#pragma warning disable CS1591
using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class UpdateProgramTypeCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "ENC", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Encounter Service", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "gateway",
                table: "ProgramTypeCode",
                keyColumn: "ProgramCode",
                keyValue: "ENC");
        }
    }
}
