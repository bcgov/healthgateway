﻿//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
#pragma warning disable CS1591
// <auto-generated />
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class GenericCacheIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GenericCache_HdId_Domain",
                schema: "gateway",
                table: "GenericCache",
                columns: new[] { "HdId", "Domain" });

            string schema = "gateway";
            string table = "GenericCache";
            string index = $"CREATE INDEX \"IX_{table}_JSON_GIN\" on {schema}.\"{table}\" USING gin(\"JSON\")";
            migrationBuilder.Sql(index);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GenericCache_HdId_Domain",
                schema: "gateway",
                table: "GenericCache");

            string schema = "gateway";
            string table = "GenericCache";
            string index = $"DROP INDEX {schema}.\"IX_{table}_JSON_GIN\"";
            migrationBuilder.Sql(index);
        }
    }
}
