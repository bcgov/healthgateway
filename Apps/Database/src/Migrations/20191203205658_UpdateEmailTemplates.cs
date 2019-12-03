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
                value: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
