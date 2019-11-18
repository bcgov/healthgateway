﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gateway");

            migrationBuilder.CreateSequence(
                name: "trace_seq",
                schema: "gateway",
                minValue: 1L,
                maxValue: 999999L,
                cyclic: true);

            migrationBuilder.CreateTable(
                name: "AuditTransactionResultCode",
                schema: "gateway",
                columns: table => new
                {
                    ResultCode = table.Column<string>(maxLength: 10, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Description = table.Column<string>(maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTransactionResultCode", x => x.ResultCode);
                });

            migrationBuilder.CreateTable(
                name: "EmailFormatCode",
                schema: "gateway",
                columns: table => new
                {
                    FormatCode = table.Column<string>(maxLength: 4, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailFormatCode", x => x.FormatCode);
                });

            migrationBuilder.CreateTable(
                name: "EmailStatusCode",
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
                    table.PrimaryKey("PK_EmailStatusCode", x => x.StatusCode);
                });

            migrationBuilder.CreateTable(
                name: "ProgramTypeCode",
                schema: "gateway",
                columns: table => new
                {
                    ProgramCode = table.Column<string>(maxLength: 10, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Description = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramTypeCode", x => x.ProgramCode);
                });

            migrationBuilder.CreateTable(
                name: "Schedule",
                schema: "gateway",
                columns: table => new
                {
                    ScheduleId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    ScheduleDesc = table.Column<string>(maxLength: 40, nullable: true),
                    ScheduleDescFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.ScheduleId);
                });

            migrationBuilder.CreateTable(
                name: "UserProfile",
                schema: "gateway",
                columns: table => new
                {
                    UserProfileId = table.Column<string>(maxLength: 52, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    AcceptedTermsOfService = table.Column<bool>(nullable: false),
                    Email = table.Column<string>(maxLength: 254, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfile", x => x.UserProfileId);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplate",
                schema: "gateway",
                columns: table => new
                {
                    EmailTemplateId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Name = table.Column<string>(maxLength: 30, nullable: false),
                    From = table.Column<string>(maxLength: 254, nullable: false),
                    Subject = table.Column<string>(maxLength: 100, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    FormatCode = table.Column<string>(maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplate", x => x.EmailTemplateId);
                    table.ForeignKey(
                        name: "FK_EmailTemplate_EmailFormatCode_FormatCode",
                        column: x => x.FormatCode,
                        principalSchema: "gateway",
                        principalTable: "EmailFormatCode",
                        principalColumn: "FormatCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Email",
                schema: "gateway",
                columns: table => new
                {
                    EmailId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    From = table.Column<string>(maxLength: 254, nullable: false),
                    To = table.Column<string>(maxLength: 254, nullable: false),
                    Subject = table.Column<string>(maxLength: 100, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    FormatCode = table.Column<string>(maxLength: 4, nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    SentDateTime = table.Column<DateTime>(nullable: true),
                    LastRetryDateTime = table.Column<DateTime>(nullable: true),
                    Attempts = table.Column<int>(nullable: false),
                    SmtpStatusCode = table.Column<int>(nullable: false),
                    EmailStatusCode = table.Column<string>(maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Email", x => x.EmailId);
                    table.ForeignKey(
                        name: "FK_Email_EmailStatusCode_EmailStatusCode",
                        column: x => x.EmailStatusCode,
                        principalSchema: "gateway",
                        principalTable: "EmailStatusCode",
                        principalColumn: "StatusCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Email_EmailFormatCode_FormatCode",
                        column: x => x.FormatCode,
                        principalSchema: "gateway",
                        principalTable: "EmailFormatCode",
                        principalColumn: "FormatCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvent",
                schema: "gateway",
                columns: table => new
                {
                    AuditEventId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    AuditEventDateTime = table.Column<DateTime>(nullable: false),
                    ClientIP = table.Column<string>(maxLength: 15, nullable: false),
                    ApplicationSubject = table.Column<string>(maxLength: 100, nullable: true),
                    ApplicationType = table.Column<string>(maxLength: 10, nullable: false),
                    TransactionName = table.Column<string>(maxLength: 100, nullable: false),
                    TransactionVersion = table.Column<string>(maxLength: 5, nullable: true),
                    Trace = table.Column<string>(maxLength: 200, nullable: true),
                    TransactionResultCode = table.Column<string>(maxLength: 10, nullable: false),
                    TransactionDuration = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvent", x => x.AuditEventId);
                    table.ForeignKey(
                        name: "FK_AuditEvent_ProgramTypeCode_ApplicationType",
                        column: x => x.ApplicationType,
                        principalSchema: "gateway",
                        principalTable: "ProgramTypeCode",
                        principalColumn: "ProgramCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditEvent_AuditTransactionResultCode_TransactionResultCode",
                        column: x => x.TransactionResultCode,
                        principalSchema: "gateway",
                        principalTable: "AuditTransactionResultCode",
                        principalColumn: "ResultCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileDownload",
                schema: "gateway",
                columns: table => new
                {
                    FileDownloadId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Name = table.Column<string>(maxLength: 35, nullable: false),
                    Hash = table.Column<string>(maxLength: 44, nullable: false),
                    ProgramCode = table.Column<string>(maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDownload", x => x.FileDownloadId);
                    table.ForeignKey(
                        name: "FK_FileDownload_ProgramTypeCode_ProgramCode",
                        column: x => x.ProgramCode,
                        principalSchema: "gateway",
                        principalTable: "ProgramTypeCode",
                        principalColumn: "ProgramCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailInvite",
                schema: "gateway",
                columns: table => new
                {
                    EmailInviteId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    HdId = table.Column<string>(maxLength: 52, nullable: false),
                    Validated = table.Column<bool>(nullable: false),
                    EmailId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailInvite", x => x.EmailInviteId);
                    table.ForeignKey(
                        name: "FK_EmailInvite_Email_EmailId",
                        column: x => x.EmailId,
                        principalSchema: "gateway",
                        principalTable: "Email",
                        principalColumn: "EmailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugProduct",
                schema: "gateway",
                columns: table => new
                {
                    DrugProductId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    DrugCode = table.Column<string>(maxLength: 8, nullable: false),
                    ProductCategorization = table.Column<string>(maxLength: 80, nullable: true),
                    DrugClass = table.Column<string>(maxLength: 40, nullable: true),
                    DrugClassFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DrugIdentificationNumber = table.Column<string>(maxLength: 29, nullable: true),
                    BrandName = table.Column<string>(maxLength: 200, nullable: true),
                    BrandNameFrench = table.Column<string>(maxLength: 300, nullable: true),
                    Descriptor = table.Column<string>(maxLength: 150, nullable: true),
                    DescriptorFrench = table.Column<string>(maxLength: 200, nullable: true),
                    PediatricFlag = table.Column<string>(maxLength: 1, nullable: true),
                    AccessionNumber = table.Column<string>(maxLength: 5, nullable: true),
                    NumberOfAis = table.Column<string>(maxLength: 10, nullable: true),
                    LastUpdate = table.Column<DateTime>(nullable: false),
                    AiGroupNumber = table.Column<string>(maxLength: 10, nullable: true),
                    FileDownloadId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugProduct", x => x.DrugProductId);
                    table.ForeignKey(
                        name: "FK_DrugProduct_FileDownload_FileDownloadId",
                        column: x => x.FileDownloadId,
                        principalSchema: "gateway",
                        principalTable: "FileDownload",
                        principalColumn: "FileDownloadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmaCareDrug",
                schema: "gateway",
                columns: table => new
                {
                    PharmaCareDrugId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    DINPIN = table.Column<string>(maxLength: 8, nullable: false),
                    Plan = table.Column<string>(maxLength: 2, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "Date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "Date", nullable: false),
                    BenefitGroupList = table.Column<string>(maxLength: 60, nullable: true),
                    LCAIndicator = table.Column<string>(maxLength: 2, nullable: true),
                    PayGenericIndicator = table.Column<string>(maxLength: 1, nullable: true),
                    BrandName = table.Column<string>(maxLength: 80, nullable: true),
                    Manufacturer = table.Column<string>(maxLength: 6, nullable: true),
                    GenericName = table.Column<string>(maxLength: 60, nullable: true),
                    DosageForm = table.Column<string>(maxLength: 20, nullable: true),
                    TrialFlag = table.Column<string>(maxLength: 1, nullable: true),
                    MaximumPrice = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    LCAPrice = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    RDPCategory = table.Column<string>(maxLength: 4, nullable: true),
                    RDPSubCategory = table.Column<string>(maxLength: 4, nullable: true),
                    RDPPrice = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    RDPExcludedPlans = table.Column<string>(maxLength: 20, nullable: true),
                    CFRCode = table.Column<string>(maxLength: 1, nullable: true),
                    PharmaCarePlanDescription = table.Column<string>(maxLength: 80, nullable: true),
                    MaximumDaysSupply = table.Column<int>(nullable: true),
                    QuantityLimit = table.Column<int>(nullable: true),
                    FormularyListDate = table.Column<DateTime>(type: "Date", nullable: false),
                    LimitedUseFlag = table.Column<string>(maxLength: 1, nullable: true),
                    FileDownloadId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaCareDrug", x => x.PharmaCareDrugId);
                    table.ForeignKey(
                        name: "FK_PharmaCareDrug_FileDownload_FileDownloadId",
                        column: x => x.FileDownloadId,
                        principalSchema: "gateway",
                        principalTable: "FileDownload",
                        principalColumn: "FileDownloadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveIngredient",
                schema: "gateway",
                columns: table => new
                {
                    ActiveIngredientId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    ActiveIngredientCode = table.Column<int>(nullable: false),
                    Ingredient = table.Column<string>(maxLength: 240, nullable: true),
                    IngredientFrench = table.Column<string>(maxLength: 400, nullable: true),
                    IngredientSuppliedInd = table.Column<string>(maxLength: 1, nullable: true),
                    Strength = table.Column<string>(maxLength: 20, nullable: true),
                    StrengthUnit = table.Column<string>(maxLength: 40, nullable: true),
                    StrengthUnitFrench = table.Column<string>(maxLength: 80, nullable: true),
                    StrengthType = table.Column<string>(maxLength: 40, nullable: true),
                    StrengthTypeFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DosageValue = table.Column<string>(maxLength: 20, nullable: true),
                    Base = table.Column<string>(maxLength: 1, nullable: true),
                    DosageUnit = table.Column<string>(maxLength: 40, nullable: true),
                    DosageUnitFrench = table.Column<string>(maxLength: 80, nullable: true),
                    Notes = table.Column<string>(maxLength: 2000, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveIngredient", x => x.ActiveIngredientId);
                    table.ForeignKey(
                        name: "FK_ActiveIngredient_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalSchema: "gateway",
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                schema: "gateway",
                columns: table => new
                {
                    CompanyId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    ManufacturerCode = table.Column<string>(maxLength: 5, nullable: true),
                    CompanyCode = table.Column<int>(nullable: false),
                    CompanyName = table.Column<string>(maxLength: 80, nullable: true),
                    CompanyType = table.Column<string>(maxLength: 40, nullable: true),
                    AddressMailingFlag = table.Column<string>(maxLength: 1, nullable: true),
                    AddressBillingFlag = table.Column<string>(maxLength: 1, nullable: true),
                    AddressNotificationFlag = table.Column<string>(maxLength: 1, nullable: true),
                    AddressOther = table.Column<string>(maxLength: 1, nullable: true),
                    SuiteNumber = table.Column<string>(maxLength: 20, nullable: true),
                    StreetName = table.Column<string>(maxLength: 80, nullable: true),
                    CityName = table.Column<string>(maxLength: 60, nullable: true),
                    Province = table.Column<string>(maxLength: 40, nullable: true),
                    ProvinceFrench = table.Column<string>(maxLength: 100, nullable: true),
                    Country = table.Column<string>(maxLength: 40, nullable: true),
                    CountryFrench = table.Column<string>(maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(maxLength: 20, nullable: true),
                    PostOfficeBox = table.Column<string>(maxLength: 15, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_Company_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalSchema: "gateway",
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Form",
                schema: "gateway",
                columns: table => new
                {
                    FormId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    PharmaceuticalFormCode = table.Column<int>(nullable: false),
                    PharmaceuticalForm = table.Column<string>(maxLength: 40, nullable: true),
                    PharmaceuticalFormFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form", x => x.FormId);
                    table.ForeignKey(
                        name: "FK_Form_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalSchema: "gateway",
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Packaging",
                schema: "gateway",
                columns: table => new
                {
                    PackagingId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    UPC = table.Column<string>(maxLength: 12, nullable: true),
                    PackageType = table.Column<string>(maxLength: 40, nullable: true),
                    PackageTypeFrench = table.Column<string>(maxLength: 80, nullable: true),
                    PackageSizeUnit = table.Column<string>(maxLength: 40, nullable: true),
                    PackageSizeUnitFrench = table.Column<string>(maxLength: 80, nullable: true),
                    PackageSize = table.Column<string>(maxLength: 5, nullable: true),
                    ProductInformation = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packaging", x => x.PackagingId);
                    table.ForeignKey(
                        name: "FK_Packaging_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalSchema: "gateway",
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmaceuticalStd",
                schema: "gateway",
                columns: table => new
                {
                    PharmaceuticalStdId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    PharmaceuticalStdDesc = table.Column<string>(nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaceuticalStd", x => x.PharmaceuticalStdId);
                    table.ForeignKey(
                        name: "FK_PharmaceuticalStd_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalSchema: "gateway",
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Route",
                schema: "gateway",
                columns: table => new
                {
                    RouteId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    AdministrationCode = table.Column<int>(nullable: false),
                    Administration = table.Column<string>(maxLength: 40, nullable: true),
                    AdministrationFrench = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Route", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_Route_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalSchema: "gateway",
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Status",
                schema: "gateway",
                columns: table => new
                {
                    StatusId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    CurrentStatusFlag = table.Column<string>(maxLength: 1, nullable: true),
                    StatusDesc = table.Column<string>(maxLength: 40, nullable: true),
                    StatusDescFrench = table.Column<string>(maxLength: 80, nullable: true),
                    HistoryDate = table.Column<DateTime>(nullable: true),
                    LotNumber = table.Column<string>(maxLength: 80, nullable: true),
                    ExpirationDate = table.Column<DateTime>(nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.StatusId);
                    table.ForeignKey(
                        name: "FK_Status_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalSchema: "gateway",
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TherapeuticClass",
                schema: "gateway",
                columns: table => new
                {
                    TherapeuticClassId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    AtcNumber = table.Column<string>(maxLength: 8, nullable: true),
                    Atc = table.Column<string>(maxLength: 120, nullable: true),
                    AtcFrench = table.Column<string>(maxLength: 240, nullable: true),
                    AhfsNumber = table.Column<string>(maxLength: 20, nullable: true),
                    Ahfs = table.Column<string>(maxLength: 80, nullable: true),
                    AhfsFrench = table.Column<string>(maxLength: 160, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapeuticClass", x => x.TherapeuticClassId);
                    table.ForeignKey(
                        name: "FK_TherapeuticClass_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalSchema: "gateway",
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VeterinarySpecies",
                schema: "gateway",
                columns: table => new
                {
                    VeterinarySpeciesId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 60, nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", nullable: false),
                    Species = table.Column<string>(maxLength: 80, nullable: true),
                    SpeciesFrench = table.Column<string>(maxLength: 160, nullable: true),
                    SubSpecies = table.Column<string>(maxLength: 80, nullable: true),
                    DrugProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeterinarySpecies", x => x.VeterinarySpeciesId);
                    table.ForeignKey(
                        name: "FK_VeterinarySpecies_DrugProduct_DrugProductId",
                        column: x => x.DrugProductId,
                        principalSchema: "gateway",
                        principalTable: "DrugProduct",
                        principalColumn: "DrugProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "AuditTransactionResultCode",
                columns: new[] { "ResultCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "Ok", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Success", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "AuditTransactionResultCode",
                columns: new[] { "ResultCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "Fail", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Failure", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "AuditTransactionResultCode",
                columns: new[] { "ResultCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "NotAuth", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Unauthorized", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "AuditTransactionResultCode",
                columns: new[] { "ResultCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "Err", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System Error", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "EmailFormatCode",
                columns: new[] { "FormatCode", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "Text", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "EmailFormatCode",
                columns: new[] { "FormatCode", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "HTML", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "EmailStatusCode",
                columns: new[] { "StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "Processed", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "An email that has been sent", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "EmailStatusCode",
                columns: new[] { "StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "Error", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "An Email that will not be sent", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "EmailStatusCode",
                columns: new[] { "StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "New", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A newly created email", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "EmailStatusCode",
                columns: new[] { "StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "Pending", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "An email pending batch pickup", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "PAT", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patient Service", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "FED-DRUG-A", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Federal Approved Drug Load", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "FED-DRUG-M", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Federal Marketed Drug Load", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "FED-DRUG-C", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Federal Cancelled Drug Load", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "FED-DRUG-D", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Federal Dormant Drug Load", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "PROV-DRUG", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Provincial Pharmacare Drug Load", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "CFG", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Configuration Service", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "WEB", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Web Client", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "IMM", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Immunization Service", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "ProgramTypeCode",
                columns: new[] { "ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { "MED", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Medication Service", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                schema: "gateway",
                table: "EmailTemplate",
                columns: new[] { "EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime" },
                values: new object[] { new Guid("040c2ec3-d6c0-4199-9e4b-ebe6da48d52a"), @"<!doctype html>
<html lang=""en"">
<head></head>
<body style = ""margin:0"">
    <table cellspacing = ""0"" align = ""left"" width = ""100%"" style = ""margin:0;color:#707070;font-family:Helvetica;font-size:12px;"">
        <tr style = ""background:#003366;"">
            <th width=""45"" ></th>
            <th width=""450"" align=""left"" style=""text-align:left;"">
                <div role=""img"" aria - label=""Health Gateway Logo"">
                    <img src="""" alt=""Health Gateway Logo""/>
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
                <a style = ""color:#1292c5;font-weight:600;"" href = """" > Health Gateway account verification </a>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "HTML", "donotreply@gov.bc.ca", "Registration", 10, "{ENVIRONMENT} Health Gateway Email Verification", "System", new DateTime(2019, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveIngredient_DrugProductId",
                schema: "gateway",
                table: "ActiveIngredient",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvent_ApplicationType",
                schema: "gateway",
                table: "AuditEvent",
                column: "ApplicationType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvent_TransactionResultCode",
                schema: "gateway",
                table: "AuditEvent",
                column: "TransactionResultCode");

            migrationBuilder.CreateIndex(
                name: "IX_Company_DrugProductId",
                schema: "gateway",
                table: "Company",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrugProduct_FileDownloadId",
                schema: "gateway",
                table: "DrugProduct",
                column: "FileDownloadId");

            migrationBuilder.CreateIndex(
                name: "IX_Email_EmailStatusCode",
                schema: "gateway",
                table: "Email",
                column: "EmailStatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_Email_FormatCode",
                schema: "gateway",
                table: "Email",
                column: "FormatCode");

            migrationBuilder.CreateIndex(
                name: "IX_EmailInvite_EmailId",
                schema: "gateway",
                table: "EmailInvite",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplate_FormatCode",
                schema: "gateway",
                table: "EmailTemplate",
                column: "FormatCode");

            migrationBuilder.CreateIndex(
                name: "IX_FileDownload_Hash",
                schema: "gateway",
                table: "FileDownload",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileDownload_ProgramCode",
                schema: "gateway",
                table: "FileDownload",
                column: "ProgramCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Form_DrugProductId",
                schema: "gateway",
                table: "Form",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packaging_DrugProductId",
                schema: "gateway",
                table: "Packaging",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmaCareDrug_FileDownloadId",
                schema: "gateway",
                table: "PharmaCareDrug",
                column: "FileDownloadId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmaceuticalStd_DrugProductId",
                schema: "gateway",
                table: "PharmaceuticalStd",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Route_DrugProductId",
                schema: "gateway",
                table: "Route",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Status_DrugProductId",
                schema: "gateway",
                table: "Status",
                column: "DrugProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TherapeuticClass_DrugProductId",
                schema: "gateway",
                table: "TherapeuticClass",
                column: "DrugProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VeterinarySpecies_DrugProductId",
                schema: "gateway",
                table: "VeterinarySpecies",
                column: "DrugProductId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveIngredient",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "AuditEvent",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "Company",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "EmailInvite",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "EmailTemplate",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "Form",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "Packaging",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "PharmaCareDrug",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "PharmaceuticalStd",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "Route",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "Schedule",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "Status",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "TherapeuticClass",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "UserProfile",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "VeterinarySpecies",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "AuditTransactionResultCode",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "Email",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "DrugProduct",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "EmailStatusCode",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "EmailFormatCode",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "FileDownload",
                schema: "gateway");

            migrationBuilder.DropTable(
                name: "ProgramTypeCode",
                schema: "gateway");

            migrationBuilder.DropSequence(
                name: "trace_seq",
                schema: "gateway");
        }
    }
}
