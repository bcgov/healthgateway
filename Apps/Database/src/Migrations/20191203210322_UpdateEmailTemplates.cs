﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class UpdateEmailTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("040c2ec3-d6c0-4199-9e4b-ebe6da48d52a"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head></head>
<body style = ""margin:0"">
    <table cellspacing = ""0"" align = ""left"" width = ""100%"" style = ""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style = ""background:#003366;"">
            <th width=""45"" ></th>
            <th width=""450"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria - label=""Health Gateway Logo"">
                    <img src=""${ActivationHost}/Logo"" alt=""Health Gateway Logo""/>
                </div>
            </th>
            <th width=""""></th>
        </tr>
        <tr>
            <td colspan=""3"" height=""20""></td>
        </tr>
        <tr>
            <td></td>
            <td>
                <h1 style = ""font-size:18px;"">Almost there!</h1>
                <p>We've received a request to register your email address for a Ministry of Health Gateway account.</p>
                <p>To activate your account, please verify your email by clicking the link:</p>
                <a style = ""color:#1292c5;font-weight:600;"" href = ""${ActivationHost}/ValidateEmail/${InviteKey}""> Health Gateway account verification </a>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "EmailTemplate",
                columns: new[] { "EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { new Guid("896f8f2e-3bed-400b-acaf-51dd6082b4bd"), @"<!doctype html>
    <html lang=""en"">
    <head></head>
    <body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo"" alt=""Health Gateway Logo""/>
                </div>
            </th>
            <th width=""""></th>
        </tr>
        <tr>
            <td colspan=""3"" height=""20""></td>
        </tr>
        <tr>
            <td></td>
            <td>
                <h1 style=""font-size:18px;"">Good day,</h1>
                <p>You are receiving this email as a Health Gateway patient partner. We welcome your feedback and suggestions as one of the first users of the application.</p>
                <p>Please click on the link below which will take you to your registration for the Health Gateway service. This registration link is valid for your one-time use only. We kindly ask that you do not share your link with anyone else.</p>
                <a style = ""font-weight:600;"" href=""${host}/registrationInfo?inviteKey=${inviteKey}&email=${emailTo}"">Register Now</a>
                <p>If you have any questions about the registration process, including signing up to use your BC Services Card for authentication, please contact Nino Samson at nino.samson@gov.bc.ca.</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "HTML", "HG_Donotreply@gov.bc.ca", "Invite", 1, "Health Gateway Private Invitation", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("896f8f2e-3bed-400b-acaf-51dd6082b4bd"));

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("040c2ec3-d6c0-4199-9e4b-ebe6da48d52a"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head></head>
<body style = ""margin:0"">
    <table cellspacing = ""0"" align = ""left"" width = ""100%"" style = ""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style = ""background:#003366;"">
            <th width=""45"" ></th>
            <th width=""450"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria - label=""Health Gateway Logo"">
                    <img src=""${ActivationHost}/Logo"" alt=""Health Gateway Logo""/>
                </div>
            </th>
            <th width=""""></th>
        </tr>
        <tr>
            <td colspan=""3"" height=""20""></td>
        </tr>
        <tr>
            <td></td>
            <td>
                <h1 style = ""font-size:18px;"">Almost there!</h1>
                <p>We've received a request to register your email address for a Ministry of Health Gateway account.</p>
                <p>To activate your account, please verify your email by clicking the link:</p>
                <a style = ""color:#1292c5;font-weight:600;"" href = ""${ActivationHost}/ValidateEmail/${InviteKey}"" > Health Gateway account verification </a>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");
        }
    }
}
