﻿//-------------------------------------------------------------------------
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
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class CommunicationDateConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string schema = "gateway";
            string constraint = @$"
                ALTER TABLE {schema}.""Communication"" ADD CONSTRAINT unique_date_range EXCLUDE USING gist (tsrange(""EffectiveDateTime"", ""ExpiryDateTime"") WITH &&);";

            migrationBuilder.Sql(constraint);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string schema = "gateway";
            string constraint = @$"
                ALTER TABLE {schema}.""Communication"" DROP CONSTRAINT unique_date_range;";

            migrationBuilder.Sql(constraint);
        }
    }
}
