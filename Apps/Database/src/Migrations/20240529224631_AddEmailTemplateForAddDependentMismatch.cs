using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthGateway.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTemplateForAddDependentMismatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "gateway",
                table: "EmailTemplate",
                columns: new[] { "EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { new Guid("491dabc6-f799-427c-ace4-b49ece2d612c"), "<!DOCTYPE html>\n<html lang=\"en\">\n    <head>\n        <title>Health Gateway Debug Info: Add Dependent Mismatch</title>\n        <style>\n            td:not(:first-child),\n            th:not(:first-child) {\n                padding: 0.5em;\n            }\n            pre {\n                margin: 0;\n            }\n        </style>\n    </head>\n    <body style=\"margin: 0\">\n        <strong>Hi Health Gateway Team,</strong>\n        <p>\n            Find debug information below relating to a failed request to add a\n            dependent due to mismatched data.\n        </p>\n        <table>\n            <tbody>\n                <tr>\n                    <td><strong>Delegate HDID</strong></td>\n                    <td>\n                        <pre>${delegateHdid}</pre>\n                    </td>\n                </tr>\n            </tbody>\n        </table>\n        <table>\n            <thead>\n                <tr>\n                    <th></th>\n                    <th>Request</th>\n                    <th>Response</th>\n                </tr>\n            </thead>\n            <tbody>\n                <tr>\n                    <td><strong>PHN</strong></td>\n                    <td><pre>${requestPhn}</pre></td>\n                    <td><pre>${responsePhn}</pre></td>\n                </tr>\n                <tr>\n                    <td><strong>Given Name(s)</strong></td>\n                    <td><pre>${requestFirstName}</pre></td>\n                    <td><pre>${responseFirstName}</pre></td>\n                </tr>\n                <tr>\n                    <td><strong>Last Name</strong></td>\n                    <td><pre>${requestLastName}</pre></td>\n                    <td><pre>${responseLastName}</pre></td>\n                </tr>\n                <tr>\n                    <td><strong>Birthdate</strong></td>\n                    <td><pre>${requestBirthdate}</pre></td>\n                    <td><pre>${responseBirthdate}</pre></td>\n                </tr>\n            </tbody>\n        </table>\n    </body>\n</html>\n", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "HTML", "HG_Donotreply@gov.bc.ca", "AdminAddDependentMismatch", 1, "Health Gateway Debug Info: Add Dependent Mismatch", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("491dabc6-f799-427c-ace4-b49ece2d612c"));
        }
    }
}
