using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class CommunicationTypeEmailvsBanner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommunicationTypeCode",
                schema: "gateway",
                table: "Communication",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CommunicationTypeCode",
                schema: "gateway",
                columns: table => new
                {
                    StatusCode = table.Column<string>(maxLength: 10, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Description = table.Column<string>(maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationTypeCode", x => x.StatusCode);
                });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "CommunicationTypeCode",
                columns: new[] { "StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[,]
                {
                    { "Banner", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Banner communication type", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "Email", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Email communication type", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

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

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("2ab5d4aa-c4c9-4324-a753-cde4e21e7612"),
                column: "Body",
                value: @"<!doctype html>
    <html lang=""en"">
    <head></head>
    <body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo""/>
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
                <p>Thank you for joining the wait list to be an early user of the Health Gateway.</p>
                <p>You will receive an email in the near future with a registration link.</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("2fe8c825-d4de-4884-be6a-01a97b466425"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head>
</head>
<body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo"" />
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
                <h1 style=""font-size:18px;"">Hi,</h1>
                <p>
                    You have successfully recovered your Health Gateway account. You may continue to use the service as you did before.
                </p>
                <p>Thanks,</p>
                <p>Health Gateway Team</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("79503a38-c14a-4992-b2fe-5586629f552e"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head>
</head>
<body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo"" />
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
                <h1 style=""font-size:18px;"">Hi,</h1>
                <p>
                    You have closed your Health Gateway account. If you would like to recover your account, please login to Health Gateway within the next 30 days and click “Recover Account”. No further action is required if you want your account and personally entered information to be removed from the Health Gateway after this time period.
                </p>
                <p>Thanks,</p>
                <p>Health Gateway Team</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("896f8f2e-3bed-400b-acaf-51dd6082b4bd"),
                column: "Body",
                value: @"<!doctype html>
    <html lang=""en"">
    <head></head>
    <body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo""/>
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
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("d9898318-4e53-4074-9979-5d24bd370055"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head>
</head>
<body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo"" />
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
                <h1 style=""font-size:18px;"">Hi,</h1>
                <p>
                    Your Health Gateway account closure has been completed. Your account and personally entered information have been removed from the application. You are welcome to register again for the Health Gateway in the future.
                </p>
                <p>Thanks,</p>
                <p>Health Gateway Team</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("eb695050-e2fb-4933-8815-3d4656e4541d"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head>
</head>
<body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo"" />
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
                <h1 style=""font-size:18px;"">Hi,</h1>
                <p>
                    You are receiving this email as a user of the Health Gateway. We have updated our Terms of Service, effective ${effectivedate}.
                </p>
                <p>For more information, we encourage you to review the full <a href=""${host}/${path}"">Terms of Service</a> and check out the <a href=""https://github.com/bcgov/healthgateway/wiki"">release notes</a> for a summary of new features.</p>
                <p>If you have any questions or wish to provide any feedback, please contact <a href=""mailto:${contactemail}"">${contactemail}</a>.</p>
                <p>Regards,</p>
                <p>Health Gateway Team</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "LegalAgreement",
                keyColumn: "LegalAgreementsId",
                keyValue: new Guid("ec438d12-f8e2-4719-8444-28e35d34674c"),
                column: "LegalText",
                value: @"<p><strong>HealthGateway Terms of Service</strong></p>
<p>
    Use of the Health Gateway service (the “Service”) is governed by the following terms and conditions. Please read
    these terms and conditions carefully, as by using the Service you will be deemed to have agreed to them. If you do
    not agree with these terms and conditions, please do not use the Service.
</p>
<p>
    <p><strong>1. The Health Gateway Service</strong></p>
    The Service provides residents of British Columbia with access to their health information (<strong>""Health
        Information""</strong>). It allows users to, in one place, view their Health Information from various Provincial
    health information systems, empowering patients and their families to manage their health care.
</p>
<p><strong>2. Your use of the Service </strong></p>
<p>
    You may only access your own Health Information using the Service.
</p>
<p>
    If you choose to share the Health Information accessed through this Service with others (e.g. with a family member
    or caregiver), you are responsible for all the actions they take with respect to the use of your Health Information.
</p>
<p>
    You must follow any additional terms and conditions made available to you in relation to the Service.
</p>
<p>
    Do not misuse the Service, for example by trying to access or use it using a method other than the interface and
    instructions we provide. You may use the Service only as permitted by law. We may suspend or stop providing the
    Service to you if you do not comply with these terms and conditions, or if we are investigating a suspected misuse
    of the Service.
</p>
<p>
    Using the Service does not give you ownership of any intellectual property rights in the Service or the content you
    access. Don’t remove, obscure, or alter any legal notices displayed in connection with the Service.
</p>
<p>
    We may stop providing the Service to you, or may add or create new limits on the Service, for any reason and at any
    time.
</p>
<p><strong>3. Service is not a comprehensive health record or medical advice</strong></p>
<p>
    The Health Information accessed through this Service is not a comprehensive record of your health care in BC.
</p>
<p>
    This Service is not intended to provide you with medical advice or replace the care provided by qualified health
    care professionals. If you have questions or concerns about your health, please contact your care provider.
</p>
<p><strong>4. Privacy Notice</strong></p>
<p>
    The personal information you provide the Service (Name and Email) will be used for the purpose of connecting your
    Health Gateway account to your BC Services Card account under the authority of section 26(c) of the Freedom of
    Information and Protection of Privacy Act. Once your BC Services Card is verified by the Service, you will be able
    to view your Health Information using the Service. The Service’s collection of your personal information is under
    the authority of section 26(c) of the Freedom of Information and Protection of Privacy Act.
</p>
<p>
    The Service’s notes feature allows you to enter your own notes to provide more information related to your health
    care. Use of this feature is entirely voluntary. Any notes will be stored in the Health Gateway in perpetuity, or
    until you choose to delete your account or remove specific notes. Any notes that you create can only be accessed by
    you securely using your BC Services Card.
</p>
<p>
    If you have any questions about our collection or use of personal information, please direct your inquiries to the
    Health Gateway team:
</p>
<p>
    <i>
        <div>Nino Samson</div>
        <div>Product Owner, Health Gateway</div>
        <div>Telephone: <a href=""tel:778-974-2712"">778-974-2712</a></div>
        <div>Email: <a href=""mailto:nino.samson@gov.bc.ca"">nino.samson@gov.bc.ca</a></div>
    </i>
</p>
<p><strong>5. Warranty Disclaimer</strong></p>
<p>
    The Service and all of the information it contains are provided "" as is"" without warranty of any kind, whether
    express or implied. All implied warranties, including, without limitation, implied warranties of
    merchantability, fitness for a particular purpose, and non-infringement, are hereby expressly
    disclaimed. </p>
<p><strong>6. Limitation of Liabilities</strong></p>
<p>
    Under no circumstances will the Government of British Columbia be liable to any person or business entity for any
    direct, indirect, special, incidental, consequential, or other damages based on any use of the Service or any
    website or system to which this Service may be linked, including, without limitation, any lost profits, business
    interruption, or loss of programs or information, even if the Government of British Columbia has been specifically
    advised of the possibility of such damages.
</p>
<p><strong>7. About these Terms and Conditions</strong></p>
<p>
    We may modify these terms and conditions, or any additional terms and conditions that apply to the Service, at any
    time, for example to reflect changes to the law or changes to the Service. You should review these terms and
    conditions regularly. Changes to these terms and conditions will be effective immediately after they are posted. If
    you do not agree to any changes to these terms, you should discontinue your use of the Service immediately.
    If there is any conflict between these terms and conditions and any additional terms and conditions, the additional
    terms and conditions will prevail.
    These terms and conditions are governed by and to be construed in accordance with the laws of British Columbia and
    the federal laws of Canada applicable therein.
</p>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "LegalAgreement",
                keyColumn: "LegalAgreementsId",
                keyValue: new Guid("f5acf1de-2f5f-431e-955d-a837d5854182"),
                column: "LegalText",
                value: @"<p><strong>HealthGateway Terms of Service</strong></p>
<p>
    Use of this service is governed by the following terms and conditions. Please read these terms and conditions
    carefully, as by using this website you will be deemed to have agreed to them. If you do not agree with these terms
    and conditions, do not use this service.
</p>
<p>
    The Health Gateway provides BC residents with access to their health information empowering patients and their
    families to manage their health care. In accessing your health information through this service, you acknowledge
    that the information within does not represent a comprehensive record of your health care in BC. No personal health
    information will be stored within the Health Gateway application. Each time you login, your health information will
    be fetched from information systems within BC and purged upon logout. If you choose to share your health information
    accessed through the website with a family member or caregiver, you are responsible for all the actions they take
    with respect to the use of your information.
</p>
<p>
    This service is not intended to provide you with medical advice nor replace the care provided by qualified health
    care professionals. If you have questions or concerns about your health, please contact your care provider.
</p>
<p>
    The personal information you provide (Name and Email) will be used for the purpose of connecting your Health Gateway
    account to your BC Services Card account under the authority of section 33(a) of the Freedom of Information and
    Protection of Privacy Act. This will be done through the BC Services Identity Assurance Service. Once your identity
    is verified using your BC Services Card, you will be able to view your health records from various health
    information systems in one place. Health Gateway’s collection of your personal information is under the authority of
    section 26(c) of the Freedom of Information and Protection of Privacy Act.
</p>
<p>
    If you have any questions about our collection or use of personal information, please direct your inquiries to the
    Health Gateway team:
</p>
<p>
    <i
        ><div>Nino Samson</div>
        <div>Product Owner, Health Gateway</div>
        <div>Telephone: 778-974-2712</div>
        <div>Email: nino.samson@gov.bc.ca</div>
    </i>
</p>

<p><strong>Limitation of Liabilities</strong></p>
<p>
    Under no circumstances will the Government of British Columbia be liable to any person or business entity for any
    direct, indirect, special, incidental, consequential, or other damages based on any use of this website or any other
    website to which this site is linked, including, without limitation, any lost profits, business interruption, or
    loss of programs or information, even if the Government of British Columbia has been specifically advised of the
    possibility of such damages.
</p>");

            migrationBuilder.CreateIndex(
                name: "IX_Communication_CommunicationTypeCode",
                schema: "gateway",
                table: "Communication",
                column: "CommunicationTypeCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Communication_CommunicationTypeCode_CommunicationTypeCode",
                schema: "gateway",
                table: "Communication",
                column: "CommunicationTypeCode",
                principalSchema: "gateway",
                principalTable: "CommunicationTypeCode",
                principalColumn: "StatusCode",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Communication_CommunicationTypeCode_CommunicationTypeCode",
                schema: "gateway",
                table: "Communication");

            migrationBuilder.DropTable(
                name: "CommunicationTypeCode",
                schema: "gateway");

            migrationBuilder.DropIndex(
                name: "IX_Communication_CommunicationTypeCode",
                schema: "gateway",
                table: "Communication");

            migrationBuilder.DropColumn(
                name: "CommunicationTypeCode",
                schema: "gateway",
                table: "Communication");

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

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("2ab5d4aa-c4c9-4324-a753-cde4e21e7612"),
                column: "Body",
                value: @"<!doctype html>
    <html lang=""en"">
    <head></head>
    <body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo""/>
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
                <p>Thank you for joining the wait list to be an early user of the Health Gateway.</p>
                <p>You will receive an email in the near future with a registration link.</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("2fe8c825-d4de-4884-be6a-01a97b466425"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head>
</head>
<body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo"" />
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
                <h1 style=""font-size:18px;"">Hi,</h1>
                <p>
                    You have successfully recovered your Health Gateway account. You may continue to use the service as you did before.
                </p>
                <p>Thanks,</p>
                <p>Health Gateway Team</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("79503a38-c14a-4992-b2fe-5586629f552e"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head>
</head>
<body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo"" />
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
                <h1 style=""font-size:18px;"">Hi,</h1>
                <p>
                    You have closed your Health Gateway account. If you would like to recover your account, please login to Health Gateway within the next 30 days and click “Recover Account”. No further action is required if you want your account and personally entered information to be removed from the Health Gateway after this time period.
                </p>
                <p>Thanks,</p>
                <p>Health Gateway Team</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("896f8f2e-3bed-400b-acaf-51dd6082b4bd"),
                column: "Body",
                value: @"<!doctype html>
    <html lang=""en"">
    <head></head>
    <body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo""/>
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
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("d9898318-4e53-4074-9979-5d24bd370055"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head>
</head>
<body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo"" />
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
                <h1 style=""font-size:18px;"">Hi,</h1>
                <p>
                    Your Health Gateway account closure has been completed. Your account and personally entered information have been removed from the application. You are welcome to register again for the Health Gateway in the future.
                </p>
                <p>Thanks,</p>
                <p>Health Gateway Team</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "EmailTemplate",
                keyColumn: "EmailTemplateId",
                keyValue: new Guid("eb695050-e2fb-4933-8815-3d4656e4541d"),
                column: "Body",
                value: @"<!doctype html>
<html lang=""en"">
<head>
</head>
<body style=""margin:0"">
    <table cellspacing=""0"" align=""left"" width=""100%"" style=""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style=""background:#036;"">
            <th width=""45""></th>
            <th width=""350"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria-label=""Health Gateway Logo"">
                    <img src=""${host}/Logo.png"" alt=""Health Gateway Logo"" />
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
                <h1 style=""font-size:18px;"">Hi,</h1>
                <p>
                    You are receiving this email as a user of the Health Gateway. We have updated our Terms of Service, effective ${effectivedate}.
                </p>
                <p>For more information, we encourage you to review the full <a href=""${host}/${path}"">Terms of Service</a> and check out the <a href=""https://github.com/bcgov/healthgateway/wiki"">release notes</a> for a summary of new features.</p>
                <p>If you have any questions or wish to provide any feedback, please contact <a href=""mailto:${contactemail}"">${contactemail}</a>.</p>
                <p>Regards,</p>
                <p>Health Gateway Team</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "LegalAgreement",
                keyColumn: "LegalAgreementsId",
                keyValue: new Guid("ec438d12-f8e2-4719-8444-28e35d34674c"),
                column: "LegalText",
                value: @"<p><strong>HealthGateway Terms of Service</strong></p>
<p>
    Use of the Health Gateway service (the “Service”) is governed by the following terms and conditions. Please read
    these terms and conditions carefully, as by using the Service you will be deemed to have agreed to them. If you do
    not agree with these terms and conditions, please do not use the Service.
</p>
<p>
    <p><strong>1. The Health Gateway Service</strong></p>
    The Service provides residents of British Columbia with access to their health information (<strong>""Health
        Information""</strong>). It allows users to, in one place, view their Health Information from various Provincial
    health information systems, empowering patients and their families to manage their health care.
</p>
<p><strong>2. Your use of the Service </strong></p>
<p>
    You may only access your own Health Information using the Service.
</p>
<p>
    If you choose to share the Health Information accessed through this Service with others (e.g. with a family member
    or caregiver), you are responsible for all the actions they take with respect to the use of your Health Information.
</p>
<p>
    You must follow any additional terms and conditions made available to you in relation to the Service.
</p>
<p>
    Do not misuse the Service, for example by trying to access or use it using a method other than the interface and
    instructions we provide. You may use the Service only as permitted by law. We may suspend or stop providing the
    Service to you if you do not comply with these terms and conditions, or if we are investigating a suspected misuse
    of the Service.
</p>
<p>
    Using the Service does not give you ownership of any intellectual property rights in the Service or the content you
    access. Don’t remove, obscure, or alter any legal notices displayed in connection with the Service.
</p>
<p>
    We may stop providing the Service to you, or may add or create new limits on the Service, for any reason and at any
    time.
</p>
<p><strong>3. Service is not a comprehensive health record or medical advice</strong></p>
<p>
    The Health Information accessed through this Service is not a comprehensive record of your health care in BC.
</p>
<p>
    This Service is not intended to provide you with medical advice or replace the care provided by qualified health
    care professionals. If you have questions or concerns about your health, please contact your care provider.
</p>
<p><strong>4. Privacy Notice</strong></p>
<p>
    The personal information you provide the Service (Name and Email) will be used for the purpose of connecting your
    Health Gateway account to your BC Services Card account under the authority of section 26(c) of the Freedom of
    Information and Protection of Privacy Act. Once your BC Services Card is verified by the Service, you will be able
    to view your Health Information using the Service. The Service’s collection of your personal information is under
    the authority of section 26(c) of the Freedom of Information and Protection of Privacy Act.
</p>
<p>
    The Service’s notes feature allows you to enter your own notes to provide more information related to your health
    care. Use of this feature is entirely voluntary. Any notes will be stored in the Health Gateway in perpetuity, or
    until you choose to delete your account or remove specific notes. Any notes that you create can only be accessed by
    you securely using your BC Services Card.
</p>
<p>
    If you have any questions about our collection or use of personal information, please direct your inquiries to the
    Health Gateway team:
</p>
<p>
    <i>
        <div>Nino Samson</div>
        <div>Product Owner, Health Gateway</div>
        <div>Telephone: <a href=""tel:778-974-2712"">778-974-2712</a></div>
        <div>Email: <a href=""mailto:nino.samson@gov.bc.ca"">nino.samson@gov.bc.ca</a></div>
    </i>
</p>
<p><strong>5. Warranty Disclaimer</strong></p>
<p>
    The Service and all of the information it contains are provided "" as is"" without warranty of any kind, whether
    express or implied. All implied warranties, including, without limitation, implied warranties of
    merchantability, fitness for a particular purpose, and non-infringement, are hereby expressly
    disclaimed. </p>
<p><strong>6. Limitation of Liabilities</strong></p>
<p>
    Under no circumstances will the Government of British Columbia be liable to any person or business entity for any
    direct, indirect, special, incidental, consequential, or other damages based on any use of the Service or any
    website or system to which this Service may be linked, including, without limitation, any lost profits, business
    interruption, or loss of programs or information, even if the Government of British Columbia has been specifically
    advised of the possibility of such damages.
</p>
<p><strong>7. About these Terms and Conditions</strong></p>
<p>
    We may modify these terms and conditions, or any additional terms and conditions that apply to the Service, at any
    time, for example to reflect changes to the law or changes to the Service. You should review these terms and
    conditions regularly. Changes to these terms and conditions will be effective immediately after they are posted. If
    you do not agree to any changes to these terms, you should discontinue your use of the Service immediately.
    If there is any conflict between these terms and conditions and any additional terms and conditions, the additional
    terms and conditions will prevail.
    These terms and conditions are governed by and to be construed in accordance with the laws of British Columbia and
    the federal laws of Canada applicable therein.
</p>");

            migrationBuilder.UpdateData(
                schema: "gateway",
                table: "LegalAgreement",
                keyColumn: "LegalAgreementsId",
                keyValue: new Guid("f5acf1de-2f5f-431e-955d-a837d5854182"),
                column: "LegalText",
                value: @"<p><strong>HealthGateway Terms of Service</strong></p>
<p>
    Use of this service is governed by the following terms and conditions. Please read these terms and conditions
    carefully, as by using this website you will be deemed to have agreed to them. If you do not agree with these terms
    and conditions, do not use this service.
</p>
<p>
    The Health Gateway provides BC residents with access to their health information empowering patients and their
    families to manage their health care. In accessing your health information through this service, you acknowledge
    that the information within does not represent a comprehensive record of your health care in BC. No personal health
    information will be stored within the Health Gateway application. Each time you login, your health information will
    be fetched from information systems within BC and purged upon logout. If you choose to share your health information
    accessed through the website with a family member or caregiver, you are responsible for all the actions they take
    with respect to the use of your information.
</p>
<p>
    This service is not intended to provide you with medical advice nor replace the care provided by qualified health
    care professionals. If you have questions or concerns about your health, please contact your care provider.
</p>
<p>
    The personal information you provide (Name and Email) will be used for the purpose of connecting your Health Gateway
    account to your BC Services Card account under the authority of section 33(a) of the Freedom of Information and
    Protection of Privacy Act. This will be done through the BC Services Identity Assurance Service. Once your identity
    is verified using your BC Services Card, you will be able to view your health records from various health
    information systems in one place. Health Gateway’s collection of your personal information is under the authority of
    section 26(c) of the Freedom of Information and Protection of Privacy Act.
</p>
<p>
    If you have any questions about our collection or use of personal information, please direct your inquiries to the
    Health Gateway team:
</p>
<p>
    <i
        ><div>Nino Samson</div>
        <div>Product Owner, Health Gateway</div>
        <div>Telephone: 778-974-2712</div>
        <div>Email: nino.samson@gov.bc.ca</div>
    </i>
</p>

<p><strong>Limitation of Liabilities</strong></p>
<p>
    Under no circumstances will the Government of British Columbia be liable to any person or business entity for any
    direct, indirect, special, incidental, consequential, or other damages based on any use of this website or any other
    website to which this site is linked, including, without limitation, any lost profits, business interruption, or
    loss of programs or information, even if the Government of British Columbia has been specifically advised of the
    possibility of such damages.
</p>");
        }
    }
}
