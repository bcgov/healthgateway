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
using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class UpdateVerifyEmailTemplate : Migration
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
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style = ""background:#003366;"">
            <th width=""45"" ></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria - label=""Health Gateway Logo"">
                    <img src=""${ActivationHost}/Logo.png"" alt=""Health Gateway Logo""/>
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
                <p>We've received a request to register your email address for a Health Gateway account.</p>
                <p>To activate your account, please verify your email by clicking the link:</p>
                <a style = ""color:#1292c5;font-weight:600;"" href = ""${ActivationHost}/ValidateEmail/${InviteKey}""> Health Gateway Account Verification </a>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");
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
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style = ""background:#003366;"">
            <th width=""45"" ></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria - label=""Health Gateway Logo"">
                    <img src=""${ActivationHost}/Logo.png"" alt=""Health Gateway Logo""/>
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
        }
    }
}
