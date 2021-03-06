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
using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class UserProfileHistoryLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string schema = "gateway";
            string updateNullLoginSql = @$"
                UPDATE gateway.""UserProfile"" AS up1
                SET ""LastLoginDateTime"" = up2.""CreatedDateTime"", ""UpdatedBy"" = 'UserProfileHistoryLoginMigration', ""UpdatedDateTime""=now()
                FROM gateway.""UserProfile"" AS up2
                WHERE up1.""LastLoginDateTime"" is NULL AND up1.""UserProfileId"" = up2.""UserProfileId"";
                ";
            migrationBuilder.Sql(updateNullLoginSql);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginDateTime",
                schema: "gateway",
                table: "UserProfile",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            string triggerFunction = @$"
CREATE or REPLACE FUNCTION {schema}.""UserProfileHistoryFunction""()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    NOT LEAKPROOF
AS $BODY$
BEGIN
    IF(TG_OP = 'DELETE') THEN
        INSERT INTO {schema}.""UserProfileHistory""(""UserProfileHistoryId"", ""Operation"", ""OperationDateTime"",
                    ""UserProfileId"", ""AcceptedTermsOfService"", ""Email"", ""ClosedDateTime"", ""IdentityManagementId"",
                    ""EncryptionKey"", ""LastLoginDateTime"", ""CreatedBy"", ""CreatedDateTime"", ""UpdatedBy"", ""UpdatedDateTime"", ""SMSNumber"") 
		VALUES(uuid_generate_v4(), TG_OP, now(),
               old.""UserProfileId"", old.""AcceptedTermsOfService"", old.""Email"", old.""ClosedDateTime"", old.""IdentityManagementId"",
               old.""EncryptionKey"", old.""LastLoginDateTime"", old.""CreatedBy"", old.""CreatedDateTime"", old.""UpdatedBy"", old.""UpdatedDateTime"", old.""SMSNumber"");
        RETURN old;
    ELSEIF(TG_OP = 'UPDATE') THEN
        INSERT INTO {schema}.""UserProfileHistory""(""UserProfileHistoryId"", ""Operation"", ""OperationDateTime"",
                    ""UserProfileId"", ""AcceptedTermsOfService"", ""Email"", ""ClosedDateTime"", ""IdentityManagementId"",
                    ""EncryptionKey"", ""LastLoginDateTime"", ""CreatedBy"", ""CreatedDateTime"", ""UpdatedBy"", ""UpdatedDateTime"", ""SMSNumber"") 
		VALUES(uuid_generate_v4(), TG_OP || '_LOGIN', now(),
               old.""UserProfileId"", old.""AcceptedTermsOfService"", old.""Email"", old.""ClosedDateTime"", old.""IdentityManagementId"",
               old.""EncryptionKey"", old.""LastLoginDateTime"", old.""CreatedBy"", old.""CreatedDateTime"", old.""UpdatedBy"", old.""UpdatedDateTime"", old.""SMSNumber"");
        RETURN old;
    END IF;
END;$BODY$;";

            string trigger = @$"
    CREATE TRIGGER ""UserProfileHistoryLoginTrigger""
    AFTER UPDATE OF ""LastLoginDateTime""
    ON {schema}.""UserProfile""
    FOR EACH ROW
    EXECUTE PROCEDURE {schema}.""UserProfileHistoryFunction""();";

            migrationBuilder.Sql(triggerFunction);
            migrationBuilder.Sql(trigger);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string schema = "gateway";
            string triggerFunction = @$"
CREATE or REPLACE FUNCTION {schema}.""UserProfileHistoryFunction""()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    NOT LEAKPROOF
AS $BODY$
BEGIN
    IF(TG_OP = 'DELETE') THEN
        INSERT INTO {schema}.""UserProfileHistory""(""UserProfileHistoryId"", ""Operation"", ""OperationDateTime"",
                    ""UserProfileId"", ""AcceptedTermsOfService"", ""Email"", ""ClosedDateTime"", ""IdentityManagementId"",
                    ""EncryptionKey"", ""LastLoginDateTime"", ""CreatedBy"", ""CreatedDateTime"", ""UpdatedBy"", ""UpdatedDateTime"", ""SMSNumber"") 
		VALUES(uuid_generate_v4(), TG_OP, now(),
               old.""UserProfileId"", old.""AcceptedTermsOfService"", old.""Email"", old.""ClosedDateTime"", old.""IdentityManagementId"",
               old.""EncryptionKey"", old.""LastLoginDateTime"", old.""CreatedBy"", old.""CreatedDateTime"", old.""UpdatedBy"", old.""UpdatedDateTime"", old.""SMSNumber"");
        RETURN old;
    END IF;
END;$BODY$;";

            migrationBuilder.Sql(triggerFunction);

            migrationBuilder.Sql(@$"DROP TRIGGER IF EXISTS ""UserProfileHistoryLoginTrigger"" ON {schema}.""UserProfile""");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginDateTime",
                schema: "gateway",
                table: "UserProfile",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
