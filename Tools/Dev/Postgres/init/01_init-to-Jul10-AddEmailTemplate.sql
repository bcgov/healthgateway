﻿\c hglocal
SET ROLE hglocal;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'gateway') THEN
        CREATE SCHEMA gateway;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS gateway."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'gateway') THEN
            CREATE SCHEMA gateway;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE SEQUENCE gateway.trace_seq START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 999999 CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."AuditTransactionResultCode" (
        "ResultCode" character varying(10) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "Description" character varying(30) NOT NULL,
        CONSTRAINT "PK_AuditTransactionResultCode" PRIMARY KEY ("ResultCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."EmailFormatCode" (
        "FormatCode" character varying(4) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_EmailFormatCode" PRIMARY KEY ("FormatCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."EmailStatusCode" (
        "StatusCode" character varying(10) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "Description" character varying(30) NOT NULL,
        CONSTRAINT "PK_EmailStatusCode" PRIMARY KEY ("StatusCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."ProgramTypeCode" (
        "ProgramCode" character varying(10) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "Description" character varying(50) NOT NULL,
        CONSTRAINT "PK_ProgramTypeCode" PRIMARY KEY ("ProgramCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."Schedule" (
        "ScheduleId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "ScheduleDesc" character varying(40),
        "ScheduleDescFrench" character varying(80),
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_Schedule" PRIMARY KEY ("ScheduleId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."UserFeedback" (
        "UserFeedbackId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "IsSatisfied" boolean NOT NULL,
        "Comment" character varying(500),
        CONSTRAINT "PK_UserFeedback" PRIMARY KEY ("UserFeedbackId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."UserProfile" (
        "UserProfileId" character varying(52) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "AcceptedTermsOfService" boolean NOT NULL,
        "Email" character varying(254),
        CONSTRAINT "PK_UserProfile" PRIMARY KEY ("UserProfileId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."EmailTemplate" (
        "EmailTemplateId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "Name" character varying(30) NOT NULL,
        "From" character varying(254) NOT NULL,
        "Subject" character varying(100) NOT NULL,
        "Body" text NOT NULL,
        "Priority" integer NOT NULL,
        "EffectiveDate" timestamp with time zone NOT NULL,
        "ExpiryDate" timestamp with time zone,
        "FormatCode" character varying(4) NOT NULL,
        CONSTRAINT "PK_EmailTemplate" PRIMARY KEY ("EmailTemplateId"),
        CONSTRAINT "FK_EmailTemplate_EmailFormatCode_FormatCode" FOREIGN KEY ("FormatCode") REFERENCES gateway."EmailFormatCode" ("FormatCode") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."Email" (
        "EmailId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "From" character varying(254) NOT NULL,
        "To" character varying(254) NOT NULL,
        "Subject" character varying(100) NOT NULL,
        "Body" text NOT NULL,
        "FormatCode" character varying(4) NOT NULL,
        "Priority" integer NOT NULL,
        "SentDateTime" timestamp with time zone,
        "LastRetryDateTime" timestamp with time zone,
        "Attempts" integer NOT NULL,
        "SmtpStatusCode" integer NOT NULL,
        "EmailStatusCode" character varying(10) NOT NULL,
        CONSTRAINT "PK_Email" PRIMARY KEY ("EmailId"),
        CONSTRAINT "FK_Email_EmailStatusCode_EmailStatusCode" FOREIGN KEY ("EmailStatusCode") REFERENCES gateway."EmailStatusCode" ("StatusCode") ON DELETE CASCADE,
        CONSTRAINT "FK_Email_EmailFormatCode_FormatCode" FOREIGN KEY ("FormatCode") REFERENCES gateway."EmailFormatCode" ("FormatCode") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."AuditEvent" (
        "AuditEventId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "AuditEventDateTime" timestamp with time zone NOT NULL,
        "ClientIP" character varying(15) NOT NULL,
        "ApplicationSubject" character varying(100),
        "ApplicationType" character varying(10) NOT NULL,
        "TransactionName" character varying(100) NOT NULL,
        "TransactionVersion" character varying(5),
        "Trace" character varying(200),
        "TransactionResultCode" character varying(10) NOT NULL,
        "TransactionDuration" bigint,
        CONSTRAINT "PK_AuditEvent" PRIMARY KEY ("AuditEventId"),
        CONSTRAINT "FK_AuditEvent_ProgramTypeCode_ApplicationType" FOREIGN KEY ("ApplicationType") REFERENCES gateway."ProgramTypeCode" ("ProgramCode") ON DELETE CASCADE,
        CONSTRAINT "FK_AuditEvent_AuditTransactionResultCode_TransactionResultCode" FOREIGN KEY ("TransactionResultCode") REFERENCES gateway."AuditTransactionResultCode" ("ResultCode") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."FileDownload" (
        "FileDownloadId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "Name" character varying(35) NOT NULL,
        "Hash" character varying(44) NOT NULL,
        "ProgramCode" character varying(10) NOT NULL,
        CONSTRAINT "PK_FileDownload" PRIMARY KEY ("FileDownloadId"),
        CONSTRAINT "FK_FileDownload_ProgramTypeCode_ProgramCode" FOREIGN KEY ("ProgramCode") REFERENCES gateway."ProgramTypeCode" ("ProgramCode") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."EmailInvite" (
        "EmailInviteId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "HdId" character varying(52) NOT NULL,
        "Validated" boolean NOT NULL,
        "EmailId" uuid NOT NULL,
        "InviteKey" uuid NOT NULL,
        CONSTRAINT "PK_EmailInvite" PRIMARY KEY ("EmailInviteId"),
        CONSTRAINT "FK_EmailInvite_Email_EmailId" FOREIGN KEY ("EmailId") REFERENCES gateway."Email" ("EmailId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."DrugProduct" (
        "DrugProductId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "DrugCode" character varying(8) NOT NULL,
        "ProductCategorization" character varying(80),
        "DrugClass" character varying(40),
        "DrugClassFrench" character varying(80),
        "DrugIdentificationNumber" character varying(29),
        "BrandName" character varying(200),
        "BrandNameFrench" character varying(300),
        "Descriptor" character varying(150),
        "DescriptorFrench" character varying(200),
        "PediatricFlag" character varying(1),
        "AccessionNumber" character varying(5),
        "NumberOfAis" character varying(10),
        "LastUpdate" timestamp with time zone NOT NULL,
        "AiGroupNumber" character varying(10),
        "FileDownloadId" uuid NOT NULL,
        CONSTRAINT "PK_DrugProduct" PRIMARY KEY ("DrugProductId"),
        CONSTRAINT "FK_DrugProduct_FileDownload_FileDownloadId" FOREIGN KEY ("FileDownloadId") REFERENCES gateway."FileDownload" ("FileDownloadId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."PharmaCareDrug" (
        "PharmaCareDrugId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "DINPIN" character varying(8) NOT NULL,
        "Plan" character varying(2),
        "EffectiveDate" Date NOT NULL,
        "EndDate" Date NOT NULL,
        "BenefitGroupList" character varying(60),
        "LCAIndicator" character varying(2),
        "PayGenericIndicator" character varying(1),
        "BrandName" character varying(80),
        "Manufacturer" character varying(6),
        "GenericName" character varying(60),
        "DosageForm" character varying(20),
        "TrialFlag" character varying(1),
        "MaximumPrice" decimal(8,4),
        "LCAPrice" decimal(8,4),
        "RDPCategory" character varying(4),
        "RDPSubCategory" character varying(4),
        "RDPPrice" decimal(8,4),
        "RDPExcludedPlans" character varying(20),
        "CFRCode" character varying(1),
        "PharmaCarePlanDescription" character varying(80),
        "MaximumDaysSupply" integer,
        "QuantityLimit" integer,
        "FormularyListDate" Date NOT NULL,
        "LimitedUseFlag" character varying(1),
        "FileDownloadId" uuid NOT NULL,
        CONSTRAINT "PK_PharmaCareDrug" PRIMARY KEY ("PharmaCareDrugId"),
        CONSTRAINT "FK_PharmaCareDrug_FileDownload_FileDownloadId" FOREIGN KEY ("FileDownloadId") REFERENCES gateway."FileDownload" ("FileDownloadId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."ActiveIngredient" (
        "ActiveIngredientId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "ActiveIngredientCode" integer NOT NULL,
        "Ingredient" character varying(240),
        "IngredientFrench" character varying(400),
        "IngredientSuppliedInd" character varying(1),
        "Strength" character varying(20),
        "StrengthUnit" character varying(40),
        "StrengthUnitFrench" character varying(80),
        "StrengthType" character varying(40),
        "StrengthTypeFrench" character varying(80),
        "DosageValue" character varying(20),
        "Base" character varying(1),
        "DosageUnit" character varying(40),
        "DosageUnitFrench" character varying(80),
        "Notes" character varying(2000),
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_ActiveIngredient" PRIMARY KEY ("ActiveIngredientId"),
        CONSTRAINT "FK_ActiveIngredient_DrugProduct_DrugProductId" FOREIGN KEY ("DrugProductId") REFERENCES gateway."DrugProduct" ("DrugProductId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."Company" (
        "CompanyId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "ManufacturerCode" character varying(5),
        "CompanyCode" integer NOT NULL,
        "CompanyName" character varying(80),
        "CompanyType" character varying(40),
        "AddressMailingFlag" character varying(1),
        "AddressBillingFlag" character varying(1),
        "AddressNotificationFlag" character varying(1),
        "AddressOther" character varying(1),
        "SuiteNumber" character varying(20),
        "StreetName" character varying(80),
        "CityName" character varying(60),
        "Province" character varying(40),
        "ProvinceFrench" character varying(100),
        "Country" character varying(40),
        "CountryFrench" character varying(100),
        "PostalCode" character varying(20),
        "PostOfficeBox" character varying(15),
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_Company" PRIMARY KEY ("CompanyId"),
        CONSTRAINT "FK_Company_DrugProduct_DrugProductId" FOREIGN KEY ("DrugProductId") REFERENCES gateway."DrugProduct" ("DrugProductId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."Form" (
        "FormId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "PharmaceuticalFormCode" integer NOT NULL,
        "PharmaceuticalForm" character varying(40),
        "PharmaceuticalFormFrench" character varying(80),
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_Form" PRIMARY KEY ("FormId"),
        CONSTRAINT "FK_Form_DrugProduct_DrugProductId" FOREIGN KEY ("DrugProductId") REFERENCES gateway."DrugProduct" ("DrugProductId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."Packaging" (
        "PackagingId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "UPC" character varying(12),
        "PackageType" character varying(40),
        "PackageTypeFrench" character varying(80),
        "PackageSizeUnit" character varying(40),
        "PackageSizeUnitFrench" character varying(80),
        "PackageSize" character varying(5),
        "ProductInformation" character varying(80),
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_Packaging" PRIMARY KEY ("PackagingId"),
        CONSTRAINT "FK_Packaging_DrugProduct_DrugProductId" FOREIGN KEY ("DrugProductId") REFERENCES gateway."DrugProduct" ("DrugProductId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."PharmaceuticalStd" (
        "PharmaceuticalStdId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "PharmaceuticalStdDesc" text,
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_PharmaceuticalStd" PRIMARY KEY ("PharmaceuticalStdId"),
        CONSTRAINT "FK_PharmaceuticalStd_DrugProduct_DrugProductId" FOREIGN KEY ("DrugProductId") REFERENCES gateway."DrugProduct" ("DrugProductId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."Route" (
        "RouteId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "AdministrationCode" integer NOT NULL,
        "Administration" character varying(40),
        "AdministrationFrench" character varying(80),
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_Route" PRIMARY KEY ("RouteId"),
        CONSTRAINT "FK_Route_DrugProduct_DrugProductId" FOREIGN KEY ("DrugProductId") REFERENCES gateway."DrugProduct" ("DrugProductId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."Status" (
        "StatusId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "CurrentStatusFlag" character varying(1),
        "StatusDesc" character varying(40),
        "StatusDescFrench" character varying(80),
        "HistoryDate" timestamp with time zone,
        "LotNumber" character varying(80),
        "ExpirationDate" timestamp with time zone,
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_Status" PRIMARY KEY ("StatusId"),
        CONSTRAINT "FK_Status_DrugProduct_DrugProductId" FOREIGN KEY ("DrugProductId") REFERENCES gateway."DrugProduct" ("DrugProductId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."TherapeuticClass" (
        "TherapeuticClassId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "AtcNumber" character varying(8),
        "Atc" character varying(120),
        "AtcFrench" character varying(240),
        "AhfsNumber" character varying(20),
        "Ahfs" character varying(80),
        "AhfsFrench" character varying(160),
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_TherapeuticClass" PRIMARY KEY ("TherapeuticClassId"),
        CONSTRAINT "FK_TherapeuticClass_DrugProduct_DrugProductId" FOREIGN KEY ("DrugProductId") REFERENCES gateway."DrugProduct" ("DrugProductId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE TABLE gateway."VeterinarySpecies" (
        "VeterinarySpeciesId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        "Species" character varying(80),
        "SpeciesFrench" character varying(160),
        "SubSpecies" character varying(80),
        "DrugProductId" uuid NOT NULL,
        CONSTRAINT "PK_VeterinarySpecies" PRIMARY KEY ("VeterinarySpeciesId"),
        CONSTRAINT "FK_VeterinarySpecies_DrugProduct_DrugProductId" FOREIGN KEY ("DrugProductId") REFERENCES gateway."DrugProduct" ("DrugProductId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."AuditTransactionResultCode" ("ResultCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Ok', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Success', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."AuditTransactionResultCode" ("ResultCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Fail', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Failure', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."AuditTransactionResultCode" ("ResultCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('NotAuth', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Unauthorized', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."AuditTransactionResultCode" ("ResultCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Err', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'System Error', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."EmailFormatCode" ("FormatCode", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Text', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."EmailFormatCode" ("FormatCode", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime")
    VALUES ('HTML', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."EmailStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Processed', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'An email that has been sent', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."EmailStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Error', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'An Email that will not be sent', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."EmailStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('New', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'A newly created email', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."EmailStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Pending', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'An email pending batch pickup', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('PAT', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Patient Service', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('FED-DRUG-A', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Federal Approved Drug Load', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('FED-DRUG-M', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Federal Marketed Drug Load', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('FED-DRUG-C', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Federal Cancelled Drug Load', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('FED-DRUG-D', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Federal Dormant Drug Load', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('PROV-DRUG', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Provincial Pharmacare Drug Load', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('CFG', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Configuration Service', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('WEB', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Web Client', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('IMM', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Immunization Service', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('MED', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Medication Service', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."EmailTemplate" ("EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime")
    VALUES ('040c2ec3-d6c0-4199-9e4b-ebe6da48d52a', '<!doctype html>
    <html lang="en">
    <head></head>
    <body style = "margin:0">
        <table cellspacing = "0" align = "left" width = "100%" style = "margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style = "background:#003366;">
                <th width="45" ></th>
                <th width="450" align="left" style="text-align:left;">
                    <div role="img" aria - label="Health Gateway Logo">
                        <img src="" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style = "font-size:18px;">Almost there!</h1>
                    <p>We''ve received a request to register your email address for a Ministry of Health Gateway account.</p>
                    <p>To activate your account, please verify your email by clicking the link:</p>
                    <a style = "color:#1292c5;font-weight:600;" href = "${ActivationHost}/ValidateEmail/${InviteKey}" > Health Gateway account verification </a>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', TIMESTAMPTZ '2019-05-01T00:00:00Z', NULL, 'HTML', 'donotreply@gov.bc.ca', 'Registration', 10, 'Health Gateway Email Verification ${Environment}', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ActiveIngredient_DrugProductId" ON gateway."ActiveIngredient" ("DrugProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE INDEX "IX_AuditEvent_ApplicationType" ON gateway."AuditEvent" ("ApplicationType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE INDEX "IX_AuditEvent_TransactionResultCode" ON gateway."AuditEvent" ("TransactionResultCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Company_DrugProductId" ON gateway."Company" ("DrugProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE INDEX "IX_DrugProduct_FileDownloadId" ON gateway."DrugProduct" ("FileDownloadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE INDEX "IX_Email_EmailStatusCode" ON gateway."Email" ("EmailStatusCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE INDEX "IX_Email_FormatCode" ON gateway."Email" ("FormatCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE INDEX "IX_EmailInvite_EmailId" ON gateway."EmailInvite" ("EmailId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE INDEX "IX_EmailTemplate_FormatCode" ON gateway."EmailTemplate" ("FormatCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_FileDownload_Hash" ON gateway."FileDownload" ("Hash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_FileDownload_ProgramCode" ON gateway."FileDownload" ("ProgramCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Form_DrugProductId" ON gateway."Form" ("DrugProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Packaging_DrugProductId" ON gateway."Packaging" ("DrugProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE INDEX "IX_PharmaCareDrug_FileDownloadId" ON gateway."PharmaCareDrug" ("FileDownloadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_PharmaceuticalStd_DrugProductId" ON gateway."PharmaceuticalStd" ("DrugProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Route_DrugProductId" ON gateway."Route" ("DrugProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE INDEX "IX_Status_DrugProductId" ON gateway."Status" ("DrugProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_TherapeuticClass_DrugProductId" ON gateway."TherapeuticClass" ("DrugProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_VeterinarySpecies_DrugProductId" ON gateway."VeterinarySpecies" ("DrugProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191119083157_InitialCreate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20191119083157_InitialCreate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191120010543_UpdateRegistrationEmail') THEN
    UPDATE gateway."EmailTemplate" SET "From" = 'HG_Donotreply@gov.bc.ca'
    WHERE "EmailTemplateId" = '040c2ec3-d6c0-4199-9e4b-ebe6da48d52a';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191120010543_UpdateRegistrationEmail') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20191120010543_UpdateRegistrationEmail', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191128232437_ChangeEmailInviteToNullableHdid') THEN
    ALTER TABLE gateway."EmailInvite" ALTER COLUMN "HdId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191128232437_ChangeEmailInviteToNullableHdid') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20191128232437_ChangeEmailInviteToNullableHdid', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191203232803_AddUpdateEmailTemplate') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head></head>
    <body style = "margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style = "background:#003366;">
                <th width="45" ></th>
                <th width="450" align="left" style="text-align:left;">
                    <div role="img" aria - label="Health Gateway Logo">
                        <img src="${ActivationHost}/Logo" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style = "font-size:18px;">Almost there!</h1>
                    <p>We''ve received a request to register your email address for a Ministry of Health Gateway account.</p>
                    <p>To activate your account, please verify your email by clicking the link:</p>
                    <a style = "color:#1292c5;font-weight:600;" href = "${ActivationHost}/ValidateEmail/${InviteKey}"> Health Gateway account verification </a>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = '040c2ec3-d6c0-4199-9e4b-ebe6da48d52a';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191203232803_AddUpdateEmailTemplate') THEN
    INSERT INTO gateway."EmailTemplate" ("EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime")
    VALUES ('896f8f2e-3bed-400b-acaf-51dd6082b4bd', '<!doctype html>
        <html lang="en">
        <head></head>
        <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style="background:#036;">
                <th width="45"></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Good day,</h1>
                    <p>You are receiving this email as a Health Gateway patient partner. We welcome your feedback and suggestions as one of the first users of the application.</p>
                    <p>Please click on the link below which will take you to your registration for the Health Gateway service. This registration link is valid for your one-time use only. We kindly ask that you do not share your link with anyone else.</p>
                    <a style = "font-weight:600;" href="${host}/registrationInfo?inviteKey=${inviteKey}&email=${emailTo}">Register Now</a>
                    <p>If you have any questions about the registration process, including signing up to use your BC Services Card for authentication, please contact Nino Samson at nino.samson@gov.bc.ca.</p>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', TIMESTAMPTZ '2019-05-01T00:00:00Z', NULL, 'HTML', 'HG_Donotreply@gov.bc.ca', 'Invite', 1, 'Health Gateway Private Invitation', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191203232803_AddUpdateEmailTemplate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20191203232803_AddUpdateEmailTemplate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191204191457_UpdateEmailVerificationTemplate') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head></head>
    <body style = "margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style = "background:#003366;">
                <th width="45" ></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria - label="Health Gateway Logo">
                        <img src="${ActivationHost}/Logo" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style = "font-size:18px;">Almost there!</h1>
                    <p>We''ve received a request to register your email address for a Ministry of Health Gateway account.</p>
                    <p>To activate your account, please verify your email by clicking the link:</p>
                    <a style = "color:#1292c5;font-weight:600;" href = "${ActivationHost}/ValidateEmail/${InviteKey}"> Health Gateway account verification </a>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = '040c2ec3-d6c0-4199-9e4b-ebe6da48d52a';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191204191457_UpdateEmailVerificationTemplate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20191204191457_UpdateEmailVerificationTemplate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191204211020_UpdateEmailTemplateLogo') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head></head>
    <body style = "margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style = "background:#003366;">
                <th width="45" ></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria - label="Health Gateway Logo">
                        <img src="${ActivationHost}/Logo.png" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style = "font-size:18px;">Almost there!</h1>
                    <p>We''ve received a request to register your email address for a Ministry of Health Gateway account.</p>
                    <p>To activate your account, please verify your email by clicking the link:</p>
                    <a style = "color:#1292c5;font-weight:600;" href = "${ActivationHost}/ValidateEmail/${InviteKey}"> Health Gateway account verification </a>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = '040c2ec3-d6c0-4199-9e4b-ebe6da48d52a';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191204211020_UpdateEmailTemplateLogo') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
        <html lang="en">
        <head></head>
        <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style="background:#036;">
                <th width="45"></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo.png" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Good day,</h1>
                    <p>You are receiving this email as a Health Gateway patient partner. We welcome your feedback and suggestions as one of the first users of the application.</p>
                    <p>Please click on the link below which will take you to your registration for the Health Gateway service. This registration link is valid for your one-time use only. We kindly ask that you do not share your link with anyone else.</p>
                    <a style = "font-weight:600;" href="${host}/registrationInfo?inviteKey=${inviteKey}&email=${emailTo}">Register Now</a>
                    <p>If you have any questions about the registration process, including signing up to use your BC Services Card for authentication, please contact Nino Samson at nino.samson@gov.bc.ca.</p>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = '896f8f2e-3bed-400b-acaf-51dd6082b4bd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20191204211020_UpdateEmailTemplateLogo') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20191204211020_UpdateEmailTemplateLogo', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200107171825_NullableReferenceTypes') THEN
    ALTER TABLE gateway."PharmaCareDrug" ALTER COLUMN "BrandName" SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200107171825_NullableReferenceTypes') THEN
    ALTER TABLE gateway."DrugProduct" ALTER COLUMN "DrugIdentificationNumber" SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200107171825_NullableReferenceTypes') THEN
    ALTER TABLE gateway."DrugProduct" ALTER COLUMN "BrandName" SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200107171825_NullableReferenceTypes') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200107171825_NullableReferenceTypes', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200115203341_AddBetaRequest') THEN
    CREATE TABLE gateway."BetaRequest" (
        "BetaRequestId" character varying(52) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "EmailAddress" character varying(254) NOT NULL,
        CONSTRAINT "PK_BetaRequest" PRIMARY KEY ("BetaRequestId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200115203341_AddBetaRequest') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200115203341_AddBetaRequest', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200117220253_UpdateEmailInviteExpire') THEN
    ALTER TABLE gateway."EmailInvite" ADD "ExpireDate" timestamp without time zone NOT NULL DEFAULT TIMESTAMP 'infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200117220253_UpdateEmailInviteExpire') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200117220253_UpdateEmailInviteExpire', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200123030013_UpdateBetaConfirmationTemplate') THEN
    INSERT INTO gateway."EmailTemplate" ("EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime")
    VALUES ('2ab5d4aa-c4c9-4324-a753-cde4e21e7612', '<!doctype html>
        <html lang="en">
        <head></head>
        <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style="background:#036;">
                <th width="45"></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo.png" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Good day,</h1>
                    <p>Thank you for joining the wait list to be an early user of the Health Gateway.</p>
                    <p>You will receive an email in the near future with a registration link.</p>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>', 'System', TIMESTAMP '2019-05-01T00:00:00', TIMESTAMP '2019-05-01T00:00:00', NULL, 'HTML', 'HG_Donotreply@gov.bc.ca', 'BetaConfirmation', 1, 'Health Gateway Waitlist Confirmation', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200123030013_UpdateBetaConfirmationTemplate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200123030013_UpdateBetaConfirmationTemplate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200129233249_UpdateProgramTypes') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('ADMIN', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Admin Client', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200129233249_UpdateProgramTypes') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200129233249_UpdateProgramTypes', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200218200745_UpdateUserFeedbackReviewed') THEN
    ALTER TABLE gateway."UserFeedback" ADD "IsReviewed" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200218200745_UpdateUserFeedbackReviewed') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200218200745_UpdateUserFeedbackReviewed', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200221211918_LegalAgreements') THEN
    CREATE TABLE gateway."LegalAgreementTypeCode" (
        "LegalAgreementCode" character varying(10) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "Description" character varying(50) NOT NULL,
        CONSTRAINT "PK_LegalAgreementTypeCode" PRIMARY KEY ("LegalAgreementCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200221211918_LegalAgreements') THEN
    CREATE TABLE gateway."LegalAgreement" (
        "LegalAgreementsId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "LegalAgreementCode" character varying(10) NOT NULL,
        "LegalText" text NOT NULL,
        "EffectiveDate" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_LegalAgreement" PRIMARY KEY ("LegalAgreementsId"),
        CONSTRAINT "FK_LegalAgreement_LegalAgreementTypeCode_LegalAgreementCode" FOREIGN KEY ("LegalAgreementCode") REFERENCES gateway."LegalAgreementTypeCode" ("LegalAgreementCode") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200221211918_LegalAgreements') THEN
    INSERT INTO gateway."LegalAgreementTypeCode" ("LegalAgreementCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('ToS', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Terms of Service', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200221211918_LegalAgreements') THEN
    INSERT INTO gateway."LegalAgreement" ("LegalAgreementsId", "CreatedBy", "CreatedDateTime", "EffectiveDate", "LegalAgreementCode", "LegalText", "UpdatedBy", "UpdatedDateTime")
    VALUES ('f5acf1de-2f5f-431e-955d-a837d5854182', 'System', TIMESTAMP '2019-05-01T00:00:00', TIMESTAMP '2019-12-06T00:00:00', 'ToS', '<p><strong>HealthGateway Terms of Service</strong></p>
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
    </p>', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200221211918_LegalAgreements') THEN
    CREATE INDEX "IX_LegalAgreement_LegalAgreementCode" ON gateway."LegalAgreement" ("LegalAgreementCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200221211918_LegalAgreements') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200221211918_LegalAgreements', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200224231036_ApplicationSettings') THEN
    CREATE TABLE gateway."ApplicationSetting" (
        "ApplicationSettingsId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "Application" character varying(10) NOT NULL,
        "Component" character varying(50) NOT NULL,
        "Key" text NOT NULL,
        "Value" text,
        CONSTRAINT "PK_ApplicationSetting" PRIMARY KEY ("ApplicationSettingsId"),
        CONSTRAINT "FK_ApplicationSetting_ProgramTypeCode_Application" FOREIGN KEY ("Application") REFERENCES gateway."ProgramTypeCode" ("ProgramCode") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200224231036_ApplicationSettings') THEN
    UPDATE gateway."LegalAgreement" SET "EffectiveDate" = TIMESTAMP '2019-05-01T00:00:00'
    WHERE "LegalAgreementsId" = 'f5acf1de-2f5f-431e-955d-a837d5854182';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200224231036_ApplicationSettings') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('JOBS', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Job Scheduler', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200224231036_ApplicationSettings') THEN
    INSERT INTO gateway."ApplicationSetting" ("ApplicationSettingsId", "Application", "Component", "CreatedBy", "CreatedDateTime", "Key", "UpdatedBy", "UpdatedDateTime", "Value")
    VALUES ('5f279ba2-8e7b-4b1d-8c69-467d94dcb7fb', 'JOBS', 'NotifyUpdatedLegalAgreementsJob', 'System', TIMESTAMP '2019-05-01T00:00:00', 'ToS-Last-Checked', 'System', TIMESTAMP '2019-05-01T00:00:00', '05/01/2019');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200224231036_ApplicationSettings') THEN
    CREATE INDEX "IX_ApplicationSetting_Application" ON gateway."ApplicationSetting" ("Application");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200224231036_ApplicationSettings') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200224231036_ApplicationSettings', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226012259_UserProfileHistory') THEN
    ALTER TABLE gateway."UserProfile" ADD "ClosedDateTime" timestamp without time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226012259_UserProfileHistory') THEN
    ALTER TABLE gateway."UserProfile" ADD "IdentityManagementId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226012259_UserProfileHistory') THEN
    ALTER TABLE gateway."UserProfile" ADD "LastLoginDateTime" timestamp without time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226012259_UserProfileHistory') THEN
    CREATE TABLE gateway."UserProfileHistory" (
        "UserProfileHistoryId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "UserProfileId" character varying(52) NOT NULL,
        "AcceptedTermsOfService" boolean NOT NULL,
        "Email" character varying(254),
        "ClosedDateTime" timestamp without time zone,
        "IdentityManagementId" uuid,
        "LastLoginDateTime" timestamp without time zone,
        "Operation" text NOT NULL,
        "OperationDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_UserProfileHistory" PRIMARY KEY ("UserProfileHistoryId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226012259_UserProfileHistory') THEN

    CREATE FUNCTION gateway."UserProfileHistoryFunction"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    BEGIN
        IF(TG_OP = 'DELETE') THEN
            INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                        "UserProfileId", "AcceptedTermsOfService", "Email", "ClosedDateTime", "IdentityManagementId",						 
    				    "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime") 
    		VALUES(uuid_generate_v4(), TG_OP, now(),
                   old."UserProfileId", old."AcceptedTermsOfService", old."Email", old."ClosedDateTime", old."IdentityManagementId",
                   old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime");
            RETURN old;
        END IF;
    END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226012259_UserProfileHistory') THEN

    CREATE TRIGGER "UserProfileHistoryTrigger"
        AFTER DELETE
        ON gateway."UserProfile"
        FOR EACH ROW
        EXECUTE PROCEDURE gateway."UserProfileHistoryFunction"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226012259_UserProfileHistory') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200226012259_UserProfileHistory', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226200434_ToSTemplate') THEN
    DROP INDEX gateway."IX_ApplicationSetting_Application";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226200434_ToSTemplate') THEN
    ALTER TABLE gateway."ApplicationSetting" ALTER COLUMN "Key" TYPE character varying(250);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226200434_ToSTemplate') THEN
    INSERT INTO gateway."EmailTemplate" ("EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime")
    VALUES ('eb695050-e2fb-4933-8815-3d4656e4541d', '<!doctype html>
    <html lang="en">
    <head>
    </head>
    <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style="background:#036;">
                <th width="45"></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Hi,</h1>
                    <p>
                        You are receiving this email as a user of the Health Gateway. We have updated our Terms of Service, effective ${effectivedate}.
                    </p>
                    <p>For more information, we encourage you to review the full <a href="${host}/${path}">Terms of Service</a> and check out the <a href="https://github.com/bcgov/healthgateway/wiki">release notes</a> for a summary of new features.</p>
                    <p>If you have any questions or wish to provide any feedback, please contact <a href="mailto:${contactemail}">${contactemail}.</a></p>
                    <p>Regards,</p>
                    <p>Health Gateway Team</p>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>', 'System', TIMESTAMP '2019-05-01T00:00:00', TIMESTAMP '2019-05-01T00:00:00', NULL, 'HTML', 'HG_Donotreply@gov.bc.ca', 'TermsOfService', 1, 'Health Gateway Updated Terms of Service ', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226200434_ToSTemplate') THEN
    CREATE UNIQUE INDEX "IX_ApplicationSetting_Application_Component_Key" ON gateway."ApplicationSetting" ("Application", "Component", "Key");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226200434_ToSTemplate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200226200434_ToSTemplate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226230220_AccountClosureTemplates') THEN
    INSERT INTO gateway."EmailTemplate" ("EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime")
    VALUES ('79503a38-c14a-4992-b2fe-5586629f552e', '<!doctype html>
                    <html lang="en">
                    <head>
                    </head>
                    <body style="margin:0">
                        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
                            <tr style="background:#036;">
                                <th width="45"></th>
                                <th width="350" align="left" style="text-align:left;">
                                    <div role="img" aria-label="Health Gateway Logo">
                                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                                    </div>
                                </th>
                                <th width=""></th>
                            </tr>
                            <tr>
                                <td colspan="3" height="20"></td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>
                                    <h1 style="font-size:18px;">Hi,</h1>
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
                    </html>', 'System', TIMESTAMP '2019-05-01T00:00:00', TIMESTAMP '2019-05-01T00:00:00', NULL, 'HTML', 'HG_Donotreply@gov.bc.ca', 'AccountClosed', 10, 'Health Gateway Account Closed ', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."EmailTemplate" ("EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime")
    VALUES ('2fe8c825-d4de-4884-be6a-01a97b466425', '<!doctype html>
                    <html lang="en">
                    <head>
                    </head>
                    <body style="margin:0">
                        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
                            <tr style="background:#036;">
                                <th width="45"></th>
                                <th width="350" align="left" style="text-align:left;">
                                    <div role="img" aria-label="Health Gateway Logo">
                                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                                    </div>
                                </th>
                                <th width=""></th>
                            </tr>
                            <tr>
                                <td colspan="3" height="20"></td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>
                                    <h1 style="font-size:18px;">Hi,</h1>
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
                    </html>', 'System', TIMESTAMP '2019-05-01T00:00:00', TIMESTAMP '2019-05-01T00:00:00', NULL, 'HTML', 'HG_Donotreply@gov.bc.ca', 'AccountRecovered', 10, 'Health Gateway Account Recovered', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."EmailTemplate" ("EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime")
    VALUES ('d9898318-4e53-4074-9979-5d24bd370055', '<!doctype html>
                    <html lang="en">
                    <head>
                    </head>
                    <body style="margin:0">
                        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
                            <tr style="background:#036;">
                                <th width="45"></th>
                                <th width="350" align="left" style="text-align:left;">
                                    <div role="img" aria-label="Health Gateway Logo">
                                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                                    </div>
                                </th>
                                <th width=""></th>
                            </tr>
                            <tr>
                                <td colspan="3" height="20"></td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>
                                    <h1 style="font-size:18px;">Hi,</h1>
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
                    </html>', 'System', TIMESTAMP '2019-05-01T00:00:00', TIMESTAMP '2019-05-01T00:00:00', NULL, 'HTML', 'HG_Donotreply@gov.bc.ca', 'AccountRemoved', 10, 'Health Gateway Account Closure Complete', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200226230220_AccountClosureTemplates') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200226230220_AccountClosureTemplates', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200227234222_ChangeEmailTemplatePriority') THEN
    UPDATE gateway."EmailTemplate" SET "Priority" = 1
    WHERE "EmailTemplateId" = '2fe8c825-d4de-4884-be6a-01a97b466425';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200227234222_ChangeEmailTemplatePriority') THEN
    UPDATE gateway."EmailTemplate" SET "Priority" = 1
    WHERE "EmailTemplateId" = '79503a38-c14a-4992-b2fe-5586629f552e';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200227234222_ChangeEmailTemplatePriority') THEN
    UPDATE gateway."EmailTemplate" SET "Priority" = 1
    WHERE "EmailTemplateId" = 'd9898318-4e53-4074-9979-5d24bd370055';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200227234222_ChangeEmailTemplatePriority') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200227234222_ChangeEmailTemplatePriority', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200228193314_ToSTemplateUpdate') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head>
    </head>
    <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style="background:#036;">
                <th width="45"></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Hi,</h1>
                    <p>
                        You are receiving this email as a user of the Health Gateway. We have updated our Terms of Service, effective ${effectivedate}.
                    </p>
                    <p>For more information, we encourage you to review the full <a href="${host}/${path}">Terms of Service</a> and check out the <a href="https://github.com/bcgov/healthgateway/wiki">release notes</a> for a summary of new features.</p>
                    <p>If you have any questions or wish to provide any feedback, please contact <a href="mailto:${contactemail}">${contactemail}</a>.</p>
                    <p>Regards,</p>
                    <p>Health Gateway Team</p>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = 'eb695050-e2fb-4933-8815-3d4656e4541d';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200228193314_ToSTemplateUpdate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200228193314_ToSTemplateUpdate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200304212452_Notes') THEN
    CREATE TABLE gateway."Note" (
        "NoteId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "UserProfileId" character varying(52) NOT NULL,
        "Title" character varying(100),
        "Text" character varying(1000),
        "JournalDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_Note" PRIMARY KEY ("NoteId"),
        CONSTRAINT "FK_Note_UserProfile_UserProfileId" FOREIGN KEY ("UserProfileId") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200304212452_Notes') THEN
    CREATE INDEX "IX_Note_UserProfileId" ON gateway."Note" ("UserProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200304212452_Notes') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200304212452_Notes', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200304214641_UpdateVerifyEmailTemplate') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head></head>
    <body style = "margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style = "background:#003366;">
                <th width="45" ></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria - label="Health Gateway Logo">
                        <img src="${ActivationHost}/Logo.png" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style = "font-size:18px;">Almost there!</h1>
                    <p>We''ve received a request to register your email address for a Health Gateway account.</p>
                    <p>To activate your account, please verify your email by clicking the link:</p>
                    <a style = "color:#1292c5;font-weight:600;" href = "${ActivationHost}/ValidateEmail/${InviteKey}"> Health Gateway Account Verification </a>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = '040c2ec3-d6c0-4199-9e4b-ebe6da48d52a';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200304214641_UpdateVerifyEmailTemplate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200304214641_UpdateVerifyEmailTemplate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200312180332_CompositeNoteKeys') THEN
    ALTER TABLE gateway."Note" DROP CONSTRAINT "PK_Note";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200312180332_CompositeNoteKeys') THEN
    ALTER TABLE gateway."Note" ADD CONSTRAINT "PK_Note" PRIMARY KEY ("NoteId", "UserProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200312180332_CompositeNoteKeys') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200312180332_CompositeNoteKeys', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200317223800_ToSUpdate4Notes') THEN
    INSERT INTO gateway."LegalAgreement" ("LegalAgreementsId", "CreatedBy", "CreatedDateTime", "EffectiveDate", "LegalAgreementCode", "LegalText", "UpdatedBy", "UpdatedDateTime")
    VALUES ('ec438d12-f8e2-4719-8444-28e35d34674c', 'System', TIMESTAMP '2020-03-18T00:00:00', TIMESTAMP '2020-03-18T00:00:00', 'ToS', '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the “Service”) is governed by the following terms and conditions. Please read
        these terms and conditions carefully, as by using the Service you will be deemed to have agreed to them. If you do
        not agree with these terms and conditions, please do not use the Service.
    </p>
    <p>
        <p><strong>1. The Health Gateway Service</strong></p>
        The Service provides residents of British Columbia with access to their health information (<strong>"Health
            Information"</strong>). It allows users to, in one place, view their Health Information from various Provincial
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
            <div>Telephone: <a href="tel:778-974-2712">778-974-2712</a></div>
            <div>Email: <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a></div>
        </i>
    </p>
    <p><strong>5. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided " as is" without warranty of any kind, whether
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
    </p>', 'System', TIMESTAMP '2020-03-18T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200317223800_ToSUpdate4Notes') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200317223800_ToSUpdate4Notes', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200330230502_GenericCache') THEN
    CREATE TABLE gateway."GenericCache" (
        "GenericCacheId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "HdId" character varying(54) NOT NULL,
        "Domain" character varying(250) NOT NULL,
        "ExpiryDateTime" timestamp without time zone NOT NULL,
        "JSONType" text NOT NULL,
        "JSON" jsonb NOT NULL,
        CONSTRAINT "PK_GenericCache" PRIMARY KEY ("GenericCacheId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200330230502_GenericCache') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200330230502_GenericCache', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200413234129_AddHDID2Feedback') THEN
    ALTER TABLE gateway."UserFeedback" ADD "UserProfileId" character varying(52);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200413234129_AddHDID2Feedback') THEN
    CREATE INDEX "IX_UserFeedback_UserProfileId" ON gateway."UserFeedback" ("UserProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200413234129_AddHDID2Feedback') THEN
    ALTER TABLE gateway."UserFeedback" ADD CONSTRAINT "FK_UserFeedback_UserProfile_UserProfileId" FOREIGN KEY ("UserProfileId") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200413234129_AddHDID2Feedback') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200413234129_AddHDID2Feedback', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200421223141_Comment') THEN
    CREATE TABLE gateway."Comment" (
        "CommentId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "UserProfileId" character varying(52) NOT NULL,
        "Text" character varying(1000),
        "EntryTypeCode" character varying(3) NOT NULL,
        "ParentEntryId" character varying(32) NOT NULL,
        CONSTRAINT "PK_Comment" PRIMARY KEY ("CommentId"),
        CONSTRAINT "FK_Comment_UserProfile_UserProfileId" FOREIGN KEY ("UserProfileId") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200421223141_Comment') THEN
    CREATE INDEX "IX_Comment_UserProfileId" ON gateway."Comment" ("UserProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200421223141_Comment') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200421223141_Comment', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200422234729_SupportEncryption') THEN
    ALTER TABLE gateway."UserProfileHistory" ADD "EncryptionKey" character varying(44);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200422234729_SupportEncryption') THEN
    ALTER TABLE gateway."UserProfile" ADD "EncryptionKey" character varying(44);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200422234729_SupportEncryption') THEN
    ALTER TABLE gateway."Note" ALTER COLUMN "Title" TYPE character varying(152);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200422234729_SupportEncryption') THEN
    ALTER TABLE gateway."Note" ALTER COLUMN "Text" TYPE character varying(1344);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200422234729_SupportEncryption') THEN
    ALTER TABLE gateway."Comment" ALTER COLUMN "Text" TYPE character varying(1344);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200422234729_SupportEncryption') THEN

    CREATE or REPLACE FUNCTION gateway."UserProfileHistoryFunction"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    BEGIN
        IF(TG_OP = 'DELETE') THEN
            INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                        "UserProfileId", "AcceptedTermsOfService", "Email", "ClosedDateTime", "IdentityManagementId",
                        "EncryptionKey", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime") 
    		VALUES(uuid_generate_v4(), TG_OP, now(),
                   old."UserProfileId", old."AcceptedTermsOfService", old."Email", old."ClosedDateTime", old."IdentityManagementId",
                   old."EncryptionKey", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime");
            RETURN old;
        END IF;
    END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200422234729_SupportEncryption') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200422234729_SupportEncryption', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200423173644_Communication') THEN
    CREATE TABLE gateway."Communication" (
        "CommunicationId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "Text" character varying(1000),
        "Subject" character varying(1000),
        "EffectiveDateTime" timestamp without time zone NOT NULL,
        "ExpiryDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_Communication" PRIMARY KEY ("CommunicationId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200423173644_Communication') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200423173644_Communication', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200512223446_AddLaboratoryApplication') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('LAB', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Laboratory Service', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200512223446_AddLaboratoryApplication') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200512223446_AddLaboratoryApplication', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200514220748_UpdateCommentModel') THEN
    ALTER TABLE gateway."Comment" ALTER COLUMN "ParentEntryId" TYPE character varying(36);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200514220748_UpdateCommentModel') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200514220748_UpdateCommentModel', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200515154544_UpdateProfile') THEN
    ALTER TABLE gateway."UserProfile" ADD "PhoneNumber" character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200515154544_UpdateProfile') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200515154544_UpdateProfile', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200521185337_RenameEmailInviteDB') THEN
    ALTER TABLE gateway."EmailInvite" DROP CONSTRAINT "FK_EmailInvite_Email_EmailId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200521185337_RenameEmailInviteDB') THEN
    ALTER TABLE gateway."EmailInvite" DROP CONSTRAINT "PK_EmailInvite";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200521185337_RenameEmailInviteDB') THEN
    ALTER TABLE gateway."EmailInvite" RENAME TO "MessagingVerification";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200521185337_RenameEmailInviteDB') THEN
    ALTER INDEX gateway."IX_EmailInvite_EmailId" RENAME TO "IX_MessagingVerification_EmailId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200521185337_RenameEmailInviteDB') THEN
    ALTER TABLE gateway."MessagingVerification" ADD CONSTRAINT "PK_MessagingVerification" PRIMARY KEY ("EmailInviteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200521185337_RenameEmailInviteDB') THEN
    ALTER TABLE gateway."MessagingVerification" ADD CONSTRAINT "FK_MessagingVerification_Email_EmailId" FOREIGN KEY ("EmailId") REFERENCES gateway."Email" ("EmailId") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200521185337_RenameEmailInviteDB') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200521185337_RenameEmailInviteDB', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200521185607_RenameEmailInviteModel') THEN
    ALTER TABLE gateway."MessagingVerification" RENAME COLUMN "EmailInviteId" TO "MessagingVerificationId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200521185607_RenameEmailInviteModel') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200521185607_RenameEmailInviteModel', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN
    ALTER TABLE gateway."MessagingVerification" ADD "SMSNumber" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN
    ALTER TABLE gateway."MessagingVerification" ADD "SMSValidationCode" character varying(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN
    ALTER TABLE gateway."MessagingVerification" ADD "VerificationType" character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN
    CREATE TABLE gateway."MessagingVerificationTypeCode" (
        "MessagingVerificationCode" character varying(10) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "Description" character varying(50) NOT NULL,
        CONSTRAINT "PK_MessagingVerificationTypeCode" PRIMARY KEY ("MessagingVerificationCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN
    INSERT INTO gateway."MessagingVerificationTypeCode" ("MessagingVerificationCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Email', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Email Verification Type Code', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."MessagingVerificationTypeCode" ("MessagingVerificationCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('SMS', 'System', TIMESTAMP '2019-05-01T00:00:00', 'SMS Verification Type Code', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN
    CREATE INDEX "IX_MessagingVerification_VerificationType" ON gateway."MessagingVerification" ("VerificationType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN
    ALTER TABLE gateway."MessagingVerification" ADD CONSTRAINT "FK_MessagingVerification_MessagingVerificationTypeCode_Verific~" FOREIGN KEY ("VerificationType") REFERENCES gateway."MessagingVerificationTypeCode" ("MessagingVerificationCode") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN

    UPDATE  gateway."MessagingVerification" 
        SET "VerificationType"='Email', "UpdatedBy"='SMSVerificationMigration', "UpdatedDateTime"=now();

    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "VerificationType" SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526063610_SMSVerification') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200526063610_SMSVerification', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526203427_MissingMigrationNullable') THEN
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "VerificationType" SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200526203427_MissingMigrationNullable') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200526203427_MissingMigrationNullable', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200529202233_PhoneBugfix') THEN
    ALTER TABLE gateway."UserProfile" RENAME COLUMN "PhoneNumber" TO "SMSNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200529202233_PhoneBugfix') THEN
    ALTER TABLE gateway."UserProfile" ALTER COLUMN "SMSNumber" TYPE character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200529202233_PhoneBugfix') THEN
    ALTER TABLE gateway."UserProfileHistory" ADD "SMSNumber" character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200529202233_PhoneBugfix') THEN

    CREATE or REPLACE FUNCTION gateway."UserProfileHistoryFunction"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    BEGIN
        IF(TG_OP = 'DELETE') THEN
            INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                        "UserProfileId", "AcceptedTermsOfService", "Email", "ClosedDateTime", "IdentityManagementId",
                        "EncryptionKey", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "SMSNumber") 
    		VALUES(uuid_generate_v4(), TG_OP, now(),
                   old."UserProfileId", old."AcceptedTermsOfService", old."Email", old."ClosedDateTime", old."IdentityManagementId",
                   old."EncryptionKey", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime", old."SMSNumber");
            RETURN old;
        END IF;
    END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200529202233_PhoneBugfix') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200529202233_PhoneBugfix', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200601194243_UpdateMessagingVerification') THEN
    ALTER TABLE gateway."MessagingVerification" DROP CONSTRAINT "FK_MessagingVerification_Email_EmailId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200601194243_UpdateMessagingVerification') THEN
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "EmailId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200601194243_UpdateMessagingVerification') THEN
    ALTER TABLE gateway."MessagingVerification" ADD CONSTRAINT "FK_MessagingVerification_Email_EmailId" FOREIGN KEY ("EmailId") REFERENCES gateway."Email" ("EmailId") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200601194243_UpdateMessagingVerification') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200601194243_UpdateMessagingVerification', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200609144240_UpdateMessagingVerificationAttempts') THEN
    ALTER TABLE gateway."MessagingVerification" ADD "Deleted" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200609144240_UpdateMessagingVerificationAttempts') THEN
    ALTER TABLE gateway."MessagingVerification" ADD "VerificationAttempts" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200609144240_UpdateMessagingVerificationAttempts') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200609144240_UpdateMessagingVerificationAttempts', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200619221413_CommunicationDateConstraint') THEN

                    ALTER TABLE gateway."Communication" ADD CONSTRAINT unique_date_range EXCLUDE USING gist (tsrange("EffectiveDateTime", "ExpiryDateTime") WITH &&);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200619221413_CommunicationDateConstraint') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200619221413_CommunicationDateConstraint', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200622183501_ToSUpdateCovid') THEN
    INSERT INTO gateway."LegalAgreement" ("LegalAgreementsId", "CreatedBy", "CreatedDateTime", "EffectiveDate", "LegalAgreementCode", "LegalText", "UpdatedBy", "UpdatedDateTime")
    VALUES ('1d94c170-5118-4aa6-ba31-e3e07274ccbd', 'System', TIMESTAMP '2020-06-22T00:00:00', TIMESTAMP '2020-06-22T00:00:00', 'ToS', '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the <strong>"Service"</strong>) is governed by the following terms and
        conditions. Please read these terms and conditions carefully, as by using the Service you will
        be deemed to have agreed to them. If you do not agree with these terms and conditions,
        please do not use the Service.
    </p>
    <p>
    <p><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides residents of British Columbia with access to their health information
        (<strong>"Health Information"</strong>).  It allows users to, in one place, view their Health Information from
        various Provincial health information systems, empowering patients and their families to manage their health care.
    </p>
    <p><strong>2. Your use of the Service</strong></p>
    <p>
        You may only access your own Health Information using the Service.
    </p>
    <p>
        If you choose to share the Health Information accessed through this Service with others (e.g.
        with a family member or caregiver), you are responsible for all the actions they take with
        respect to the use of your Health Information.
    </p>
    <p>
        You must follow any additional terms and conditions made available to you in relation to the
        Service.
    </p>
    <p>
        Do not misuse the Service, for example by trying to access or use it using a method other
        than the interface and instructions we provide.  You may use the Service only as permitted
        by law.  We may suspend or stop providing the Service to you if you do not comply with
        these terms and conditions, or if we are investigating a suspected misuse of the Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property rights in the
        Service or the content you access.  Don’t remove, obscure, or alter any legal notices
        displayed in connection with the Service.
    </p>
    <p>
        We may stop providing the Service to you, or may add or create new limits on the Service,
        for any reason and at any time.
    </p>
    <p><strong>3. Service is not a comprehensive health record or medical advice</strong></p>
    <p>
        The Health Information accessed through this Service is not a comprehensive record of your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice or replace the care provided 
        by qualified health care professionals. If you have questions or concerns about your health, 
        please contact your care provider.
    </p>
    <p><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by the Health Gateway (Ministry of Health) and 
        Service BC under the authority of section 26(c) of the Freedom of Information and Protection 
        of Privacy Act for the purpose of providing access to your health records. Your personal 
        information such as name, email and cell phone number will be shared with other public 
        health service agencies to query your health information and notify you of updates. Your 
        personal information will not be used or disclosed for any other purposes.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own notes to provide 
        more information related to your health care. Use of these features is entirely voluntary. Any 
        notes will be stored in the Health Gateway until you choose to delete your account or 
        remove specific notes. Any notes that you create can only be accessed by you securely using 
        your BC Services Card.
    </p>
    <p>
        If you have any questions about our collection or use of personal information, please direct 
        your inquiries to the Health Gateway team:
    </p>
    <p>
        <i>
            Nino Samson<br />
            Product Owner, Health Gateway<br />
            Telephone: <a href="tel:778-974-2712">778-974-2712</a><br />
            Email: <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a><br />
            Address: 1483 Douglas Street; PO BOX 9635 STN PROV GOVT, Victoria BC<br />
        </i>
    </p>
    <p><strong>5. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided "as is" without warranty of 
        any kind, whether express or implied. All implied warranties, including, without limitation, 
        implied warranties of merchantability, fitness for a particular purpose, and non-infringement, 
        are hereby expressly disclaimed.
    </p>
    <p><strong>6. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to any person or 
        business entity for any direct, indirect, special, incidental, consequential, or other damages 
        based on any use of the Service or any website or system to which this Service may be linked, 
        including, without limitation, any lost profits, business interruption, or loss of programs or 
        information, even if the Government of British Columbia has been specifically advised of the 
        possibility of such damages.
    </p>
    <p><strong>7. About these Terms and Conditions</strong></p>
    <p>
        We may modify these terms of service, or any additional terms that apply to the Service, at 
        any time, for example to reflect changes to the law or changes to the Service.  You should 
        review these terms of service regularly.  Changes to these terms of service will be effective 
        immediately after they are posted.  If you do not agree to any changes to these terms, you 
        should discontinue your use of the Service immediately.
    </p>
    <p>
        If there is any conflict between these terms of service and any additional terms of service, 
        the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be construed in accordance with the laws of 
        British Columbia and the federal laws of Canada applicable therein.
    </p>', 'System', TIMESTAMP '2020-06-22T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200622183501_ToSUpdateCovid') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200622183501_ToSUpdateCovid', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200630213031_MvToSEffDt') THEN
    UPDATE gateway."LegalAgreement" SET "EffectiveDate" = TIMESTAMP '2020-07-31T00:00:00'
    WHERE "LegalAgreementsId" = '1d94c170-5118-4aa6-ba31-e3e07274ccbd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200630213031_MvToSEffDt') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200630213031_MvToSEffDt', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200710203326_AddUserPreference') THEN
    CREATE TABLE gateway."UserPreference" (
        "UserProfileId" character varying(52) NOT NULL,
        "Preference" text NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "Value" text NOT NULL,
        CONSTRAINT "PK_UserPreference" PRIMARY KEY ("UserProfileId", "Preference")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200710203326_AddUserPreference') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200710203326_AddUserPreference', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    ALTER TABLE gateway."Communication" ALTER COLUMN "Text" SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    ALTER TABLE gateway."Communication" ALTER COLUMN "Subject" SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    ALTER TABLE gateway."Communication" ADD "CommunicationStatusCode" character varying(10) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    ALTER TABLE gateway."Communication" ADD "CommunicationTypeCode" character varying(10) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    ALTER TABLE gateway."Communication" ADD "Priority" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    CREATE TABLE gateway."CommunicationEmail" (
        "CommunicationEmailId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "CommunicationId" uuid NOT NULL,
        "UserProfileHdId" character varying(52) NOT NULL,
        "EmailId" uuid NOT NULL,
        CONSTRAINT "PK_CommunicationEmail" PRIMARY KEY ("CommunicationEmailId"),
        CONSTRAINT "FK_CommunicationEmail_Communication_CommunicationId" FOREIGN KEY ("CommunicationId") REFERENCES gateway."Communication" ("CommunicationId") ON DELETE CASCADE,
        CONSTRAINT "FK_CommunicationEmail_Email_EmailId" FOREIGN KEY ("EmailId") REFERENCES gateway."Email" ("EmailId") ON DELETE CASCADE,
        CONSTRAINT "FK_CommunicationEmail_UserProfile_UserProfileHdId" FOREIGN KEY ("UserProfileHdId") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    CREATE TABLE gateway."CommunicationStatusCode" (
        "StatusCode" character varying(10) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "Description" character varying(50) NOT NULL,
        CONSTRAINT "PK_CommunicationStatusCode" PRIMARY KEY ("StatusCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    CREATE TABLE gateway."CommunicationTypeCode" (
        "StatusCode" character varying(10) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "Description" character varying(30) NOT NULL,
        CONSTRAINT "PK_CommunicationTypeCode" PRIMARY KEY ("StatusCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    INSERT INTO gateway."CommunicationStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('New', 'System', TIMESTAMP '2019-05-01T00:00:00', 'A newly created Communication', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."CommunicationStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Pending', 'System', TIMESTAMP '2019-05-01T00:00:00', 'A Communication pending batch pickup', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."CommunicationStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Processing', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Communication is being processed', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."CommunicationStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Processed', 'System', TIMESTAMP '2019-05-01T00:00:00', 'A Communication which has been sent', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."CommunicationStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Error', 'System', TIMESTAMP '2019-05-01T00:00:00', 'A Communication that will not be sent', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    INSERT INTO gateway."CommunicationTypeCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Banner', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Banner communication type', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."CommunicationTypeCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Email', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Email communication type', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    CREATE INDEX "IX_Communication_CommunicationStatusCode" ON gateway."Communication" ("CommunicationStatusCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    CREATE INDEX "IX_Communication_CommunicationTypeCode" ON gateway."Communication" ("CommunicationTypeCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    CREATE INDEX "IX_CommunicationEmail_CommunicationId" ON gateway."CommunicationEmail" ("CommunicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    CREATE INDEX "IX_CommunicationEmail_EmailId" ON gateway."CommunicationEmail" ("EmailId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    CREATE INDEX "IX_CommunicationEmail_UserProfileHdId" ON gateway."CommunicationEmail" ("UserProfileHdId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    ALTER TABLE gateway."Communication" ADD CONSTRAINT "FK_Communication_CommunicationStatusCode_CommunicationStatusCo~" FOREIGN KEY ("CommunicationStatusCode") REFERENCES gateway."CommunicationStatusCode" ("StatusCode") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    ALTER TABLE gateway."Communication" ADD CONSTRAINT "FK_Communication_CommunicationTypeCode_CommunicationTypeCode" FOREIGN KEY ("CommunicationTypeCode") REFERENCES gateway."CommunicationTypeCode" ("StatusCode") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724222252_NewComms') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200724222252_NewComms', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724230921_UpdateCommunicationAddScheduledDate') THEN
    ALTER TABLE gateway."Communication" ADD "ScheduledDateTime" timestamp without time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200724230921_UpdateCommunicationAddScheduledDate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200724230921_UpdateCommunicationAddScheduledDate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200803222317_PushBannerChange') THEN

    CREATE OR REPLACE FUNCTION gateway."PushBannerChange"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    DECLARE
        data JSON;
        notification JSON;
        send boolean;
        BEGIN
            send = false;
            IF((TG_OP = 'INSERT' OR TG_OP = 'UPDATE') and
                NEW."CommunicationTypeCode" = 'Banner')
            THEN
                data = row_to_json(NEW);
                send = true;
            ELSEIF(TG_OP = 'DELETE' and OLD."CommunicationTypeCode" = 'Banner') THEN
                data = row_to_json(OLD);
                send = true;
            END IF;
            IF(send) THEN
                notification = json_build_object(
                    'Table', TG_TABLE_NAME,
                    'Action', TG_OP,
                    'Data', data);
                RAISE LOG 'Sending Banner Change notification';
                PERFORM pg_notify('BannerChange', notification::TEXT);
            END IF;
            RETURN NEW;
        END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200803222317_PushBannerChange') THEN

    DROP TRIGGER IF EXISTS "PushBannerChange" ON gateway."Communication";
    CREATE TRIGGER "PushBannerChange"
        AFTER INSERT or UPDATE or DELETE
        ON gateway."Communication"
        FOR EACH ROW
        EXECUTE PROCEDURE gateway."PushBannerChange"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200803222317_PushBannerChange') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200803222317_PushBannerChange', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200817213421_AddRatingsTable') THEN
    CREATE TABLE gateway."Rating" (
        "RatingId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "RatingValue" integer NOT NULL,
        "Skip" boolean NOT NULL,
        CONSTRAINT "PK_Rating" PRIMARY KEY ("RatingId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200817213421_AddRatingsTable') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200817213421_AddRatingsTable', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200901174715_UpdateProgramTypeCode') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('ENC', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Encounter Service', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200901174715_UpdateProgramTypeCode') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200901174715_UpdateProgramTypeCode', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200922192259_AddCommunicationStatusCodeDraft') THEN
    INSERT INTO gateway."CommunicationStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Draft', 'System', TIMESTAMP '2019-05-01T00:00:00', 'A draft Communication which has not been published', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200922192259_AddCommunicationStatusCodeDraft') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200922192259_AddCommunicationStatusCodeDraft', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200926222603_ModifyUniqueDateRangeConstraint') THEN

                    ALTER TABLE gateway."Communication" DROP CONSTRAINT unique_date_range;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200926222603_ModifyUniqueDateRangeConstraint') THEN

                    ALTER TABLE gateway."Communication" ADD CONSTRAINT unique_date_range EXCLUDE USING gist (tsrange("EffectiveDateTime", "ExpiryDateTime") WITH &&) WHERE ("CommunicationTypeCode" = 'Banner');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20200926222603_ModifyUniqueDateRangeConstraint') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200926222603_ModifyUniqueDateRangeConstraint', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201008222443_UserDelegate') THEN
    CREATE TABLE gateway."UserDelegate" (
        "OwnerId" character varying(52) NOT NULL,
        "DelegateId" character varying(52) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_UserDelegate" PRIMARY KEY ("OwnerId", "DelegateId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201008222443_UserDelegate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20201008222443_UserDelegate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201017085954_GenericCacheIndexes') THEN
    CREATE INDEX "IX_GenericCache_HdId_Domain" ON gateway."GenericCache" ("HdId", "Domain");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201017085954_GenericCacheIndexes') THEN
    CREATE INDEX "IX_GenericCache_JSON_GIN" on gateway."GenericCache" USING gin("JSON");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201017085954_GenericCacheIndexes') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20201017085954_GenericCacheIndexes', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201018001236_GenericCacheUnique') THEN
    TRUNCATE gateway."GenericCache";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201018001236_GenericCacheUnique') THEN
    DROP INDEX gateway."IX_GenericCache_HdId_Domain";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201018001236_GenericCacheUnique') THEN
    CREATE UNIQUE INDEX "IX_GenericCache_HdId_Domain" ON gateway."GenericCache" ("HdId", "Domain");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201018001236_GenericCacheUnique') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20201018001236_GenericCacheUnique', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201021193628_AddUserDelegateHistory') THEN
    CREATE TABLE gateway."UserDelegateHistory" (
        "UserDelegateHistoryId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "OwnerId" character varying(52) NOT NULL,
        "DelegateId" character varying(52) NOT NULL,
        "Operation" text NOT NULL,
        "OperationDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_UserDelegateHistory" PRIMARY KEY ("UserDelegateHistoryId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201021193628_AddUserDelegateHistory') THEN

    CREATE FUNCTION gateway."UserDelegateHistoryFunction"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    BEGIN
        IF(TG_OP = 'DELETE') THEN
            INSERT INTO gateway."UserDelegateHistory"("UserDelegateHistoryId", "Operation", "OperationDateTime",
                        "OwnerId", "DelegateId", 						 
    				    "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime") 
    		VALUES(uuid_generate_v4(), TG_OP, now(),
                   old."OwnerId", old."DelegateId",
                   old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime");
            RETURN old;
        END IF;
    END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201021193628_AddUserDelegateHistory') THEN

    CREATE TRIGGER "UserDelegateHistoryTrigger"
        AFTER DELETE
        ON gateway."UserDelegate"
        FOR EACH ROW
        EXECUTE PROCEDURE gateway."UserDelegateHistoryFunction"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201021193628_AddUserDelegateHistory') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20201021193628_AddUserDelegateHistory', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    DROP TRIGGER IF EXISTS "UserDelegateHistoryTrigger" ON gateway."UserDelegate";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    DROP FUNCTION IF EXISTS gateway."UserDelegateHistoryFunction"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    DROP TABLE IF EXISTS gateway."UserDelegate";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    DROP TABLE IF EXISTS gateway."UserDelegateHistory";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    CREATE TABLE gateway."ResourceDelegateHistory" (
        "ResourceDelegateHistoryId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "ResourceOwnerHdid" character varying(52) NOT NULL,
        "ProfileHdid" character varying(52) NOT NULL,
        "ReasonCode" character varying(10) NOT NULL,
        "ReasonObjectType" text NOT NULL,
        "ReasonObject" jsonb NOT NULL,
        "Operation" text NOT NULL,
        "OperationDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_ResourceDelegateHistory" PRIMARY KEY ("ResourceDelegateHistoryId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    CREATE TABLE gateway."ResourceDelegateReasonCode" (
        "ReasonTypeCode" character varying(10) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "Description" character varying(50) NOT NULL,
        CONSTRAINT "PK_ResourceDelegateReasonCode" PRIMARY KEY ("ReasonTypeCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    CREATE TABLE gateway."ResourceDelegate" (
        "ResourceOwnerHdid" character varying(52) NOT NULL,
        "ProfileHdid" character varying(52) NOT NULL,
        "ReasonCode" character varying(10) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        "ReasonObjectType" text NOT NULL,
        "ReasonObject" jsonb NOT NULL,
        CONSTRAINT "PK_ResourceDelegate" PRIMARY KEY ("ResourceOwnerHdid", "ProfileHdid", "ReasonCode"),
        CONSTRAINT "FK_ResourceDelegate_UserProfile_ProfileHdid" FOREIGN KEY ("ProfileHdid") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE CASCADE,
        CONSTRAINT "FK_ResourceDelegate_ResourceDelegateReasonCode_ReasonCode" FOREIGN KEY ("ReasonCode") REFERENCES gateway."ResourceDelegateReasonCode" ("ReasonTypeCode") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    INSERT INTO gateway."ResourceDelegateReasonCode" ("ReasonTypeCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('COVIDLab', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Resource Delegation for Covid Laboratory', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    CREATE INDEX "IX_ResourceDelegate_ProfileHdid" ON gateway."ResourceDelegate" ("ProfileHdid");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    CREATE INDEX "IX_ResourceDelegate_ReasonCode" ON gateway."ResourceDelegate" ("ReasonCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN

    CREATE FUNCTION gateway."ResourceDelegateHistoryFunction"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    BEGIN
        IF(TG_OP = 'DELETE') THEN
            INSERT INTO gateway."ResourceDelegateHistory"("ResourceDelegateHistoryId", "Operation", "OperationDateTime",
                        "ResourceOwnerHdid", "ProfileHdid", "ReasonCode", "ReasonObjectType", "ReasonObject",                    
                        "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime") 
            VALUES(uuid_generate_v4(), TG_OP, now(),
                   old."ResourceOwnerHdid", old."ProfileHdid", old."ReasonCode", old."ReasonObjectType", old."ReasonObject",
                   old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime");
            RETURN old;
        END IF;
    END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN

    CREATE TRIGGER "ResourceDelegateHistoryTrigger"
        AFTER DELETE
        ON gateway."ResourceDelegate"
        FOR EACH ROW
        EXECUTE PROCEDURE gateway."ResourceDelegateHistoryFunction"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201118002050_AddResourceDelegate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20201118002050_AddResourceDelegate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201208231819_RemoveBetaRequest') THEN
    DROP TABLE gateway."BetaRequest";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201208231819_RemoveBetaRequest') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20201208231819_RemoveBetaRequest', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201224092706_UpdatedTermsOfService') THEN
    INSERT INTO gateway."LegalAgreement" ("LegalAgreementsId", "CreatedBy", "CreatedDateTime", "EffectiveDate", "LegalAgreementCode", "LegalText", "UpdatedBy", "UpdatedDateTime")
    VALUES ('2947456b-2b67-42b9-b239-13c4ed86060b', 'System', TIMESTAMP '2020-12-24T00:00:00', TIMESTAMP '2020-12-24T00:00:00', 'ToS', '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the <strong>"Service"</strong>) is
        governed by the following terms and conditions. Please read these terms and
        conditions carefully, as by using the Service you will be deemed to have
        agreed to them. If you do not agree with these terms and conditions, please
        do not use the Service.
    </p>
    <p></p>
    <p><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides residents of British Columbia with access to their
        health information (<strong>"Health Information"</strong>). It allows users
        to, in one place, view their Health Information from various Provincial
        health information systems, empowering patients and their families to manage
        their health care.
    </p>
    <p><strong>2. Your use of the Service</strong></p>
    <p>You may only access your own Health Information using the Service.</p>
    <p>
        If you choose to share the Health Information accessed through this Service
        with others (e.g. with a family member or caregiver), you are responsible
        for all the actions they take with respect to the use of your Health
        Information.
    </p>
    <p>
        You must follow any additional terms and conditions made available to you in
        relation to the Service.
    </p>
    <p>
        Do not misuse the Service, for example by trying to access or use it using a
        method other than the interface and instructions we provide. You may use the
        Service only as permitted by law. We may suspend or stop providing the
        Service to you if you do not comply with these terms and conditions, or if
        we are investigating a suspected misuse of the Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property
        rights in the Service or the content you access. Don’t remove, obscure, or
        alter any legal notices displayed in connection with the Service.
    </p>
    <p>
        We may stop providing the Service to you, or may add or create new limits on
        the Service, for any reason and at any time.
    </p>
    <p>
        <strong
            >3. Service is not a comprehensive health record or medical
            advice</strong
        >
    </p>
    <p>
        The Health Information accessed through this Service is not a comprehensive
        record of your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice or replace
        the care provided by qualified health care professionals. If you have
        questions or concerns about your health, please contact your care provider.
    </p>
    <p><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by the Health Gateway (Ministry
        of Health) and Service BC under the authority of section 26(c) of the
        Freedom of Information and Protection of Privacy Act for the purpose of
        providing access to your health records. Your personal information such as
        name, email and cell phone number will be shared with other public health
        service agencies to query your health information and notify you of updates.
        Your personal information will not be used or disclosed for any other
        purposes.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own notes
        to provide more information related to your health care. Use of these
        features is entirely voluntary. Any notes will be stored in the Health
        Gateway until you choose to delete your account or remove specific notes.
        Any notes that you create can only be accessed by you securely using your BC
        Services Card.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please direct your inquiries to the Health Gateway team:
    </p>
    <p>
        <i>
            Nino Samson<br />
            Product Owner, Health Gateway<br />
            Telephone: <a href="tel:778-974-2712">778-974-2712</a><br />
            Email: <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a
            ><br />
            Address: 1483 Douglas Street; PO BOX 9635 STN PROV GOVT, Victoria BC<br />
        </i>
    </p>
    <p><strong>5. Privacy Protection Guidance for Minors</strong></p>
    <p>
        Health Gateway takes the protection of your privacy very seriously and will
        make sure that your information is secure in the Health Gateway portal,
        however you will also need to take steps to protect your information. There
        may be consequences to sharing your device with others, for example people
        may be able to open your Health Gateway account and see whether you are
        taking any prescriptions, such as birth control medication or whether you
        have a positive COVID-19 test result.
    </p>
    <p>Protect your privacy by doing the following:</p>
    <p>
        It is not recommended that you download the BC Services Card app and set up
        a Health Gateway account if you cannot fully control who has access to your
        device. If you are unable to control who has access to your device, for
        example you do not own your phone or you share your phone or password with
        others, then the security of your information could be at risk. If this is a
        shared device, or one you do not fully control, there are other options to
        access your information. For example, to access your COVID-19 test results,
        please visit the BC Centre for Disease Control (BCCDC) website.
    </p>
    <p>
        Before downloading the BC Services Card app and setting up a Health Gateway
        account, make sure you have a secure password on your device that no one but
        you knows. For tips on how to create a secure password and password
        protection, please go to
        https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins.
    </p>
    <p>
        Make sure to log out of your account and close out of the screen once you
        are finished using the Health Gateway portal and lock and store your device
        securely when it is not in use.
    </p>
    <p>
        If you suspect your personal information has been viewed without your
        permission, please reach out to a trusted adult for help on what to do next.
        For more information, adults may go to
        https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/.
    </p>
    <p>
        Be suspicious of any unusual requests for your information. For example,
        Health Gateway will never ask you for any of your personal health
        information such as medical diagnosis.
    </p>
    <p>
        For more tips on how to protect your privacy go to
        https://www.getcybersafe.gc.ca/en/home.
    </p>
    <p>
        If you notice anything unusual in your account or have any privacy
        questions, please contact the Health Gateway team below for help.
    </p>
    <p><strong>6. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided "as is"
        without warranty of any kind, whether express or implied. All implied
        warranties, including, without limitation, implied warranties of
        merchantability, fitness for a particular purpose, and non-infringement, are
        hereby expressly disclaimed.
    </p>
    <p><strong>7. Attestation of Guardianship</strong></p>
    <p>
        By providing the child’s name, date of birth, personal health number and
        date of COVID-19 test, I declare that I am the child’s legal guardian as per
        the Family Law Act, the Adoption Act and/or the Child, Family and Community
        Services Act, and am attesting that I have the authority to request and
        receive health information respecting the child from third parties.
    </p>
    <p>
        If I either: (a) cease to be guardian of this child; (b) or lose the right
        to request or receive health information from third parties respecting this
        child, I will remove them as a dependent under my Health Gateway account
        immediately.
    </p>
    <p>
        I understand that I will no longer be able to access my child’s COVID-19
        test results once they are 12 years of age. I understand it is a legal
        offence to falsely claim guardianship or access another individual’s
        personal health information without legal authority or consent.
    </p>
    <p><strong>8. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of the Service or any
        website or system to which this Service may be linked, including, without
        limitation, any lost profits, business interruption, or loss of programs or
        information, even if the Government of British Columbia has been
        specifically advised of the possibility of such damages.
    </p>
    <p><strong>9. About these Terms and Conditions</strong></p>
    <p>
        We may modify these terms of service, or any additional terms that apply to
        the Service, at any time, for example to reflect changes to the law or
        changes to the Service. You should review these terms of service regularly.
        Changes to these terms of service will be effective immediately after they
        are posted. If you do not agree to any changes to these terms, you should
        discontinue your use of the Service immediately.
    </p>
    <p>
        If there is any conflict between these terms of service and any additional
        terms of service, the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be construed in accordance
        with the laws of British Columbia and the federal laws of Canada applicable
        therein.
    </p>
    ', 'System', TIMESTAMP '2020-12-24T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201224092706_UpdatedTermsOfService') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20201224092706_UpdatedTermsOfService', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201224191731_FixTermsOfService') THEN
    DELETE FROM gateway."LegalAgreement"
    WHERE "LegalAgreementsId" = '2947456b-2b67-42b9-b239-13c4ed86060b';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201224191731_FixTermsOfService') THEN
    INSERT INTO gateway."LegalAgreement" ("LegalAgreementsId", "CreatedBy", "CreatedDateTime", "EffectiveDate", "LegalAgreementCode", "LegalText", "UpdatedBy", "UpdatedDateTime")
    VALUES ('c99fd839-b4a2-40f9-b103-529efccd0dcd', 'System', TIMESTAMP '2020-12-24T00:00:00', TIMESTAMP '2020-12-24T00:00:00', 'ToS', '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the <strong>"Service"</strong>) is
        governed by the following terms and conditions. Please read these terms and
        conditions carefully, as by using the Service you will be deemed to have
        agreed to them. If you do not agree with these terms and conditions, please
        do not use the Service.
    </p>
    <p></p>
    <p><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides residents of British Columbia with access to their
        health information (<strong>"Health Information"</strong>). It allows users
        to, in one place, view their Health Information from various Provincial
        health information systems, empowering patients and their families to manage
        their health care.
    </p>
    <p><strong>2. Your use of the Service</strong></p>
    <p>You may only access your own Health Information using the Service.</p>
    <p>
        If you choose to share the Health Information accessed through this Service
        with others (e.g. with a family member or caregiver), you are responsible
        for all the actions they take with respect to the use of your Health
        Information.
    </p>
    <p>
        You must follow any additional terms and conditions made available to you in
        relation to the Service.
    </p>
    <p>
        Do not misuse the Service, for example by trying to access or use it using a
        method other than the interface and instructions we provide. You may use the
        Service only as permitted by law. We may suspend or stop providing the
        Service to you if you do not comply with these terms and conditions, or if
        we are investigating a suspected misuse of the Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property
        rights in the Service or the content you access. Don’t remove, obscure, or
        alter any legal notices displayed in connection with the Service.
    </p>
    <p>
        We may stop providing the Service to you, or may add or create new limits on
        the Service, for any reason and at any time.
    </p>
    <p>
        <strong
            >3. Service is not a comprehensive health record or medical
            advice</strong
        >
    </p>
    <p>
        The Health Information accessed through this Service is not a comprehensive
        record of your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice or replace
        the care provided by qualified health care professionals. If you have
        questions or concerns about your health, please contact your care provider.
    </p>
    <p><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by the Health Gateway (Ministry
        of Health) and Service BC under the authority of section 26(c) of the
        Freedom of Information and Protection of Privacy Act for the purpose of
        providing access to your health records. Your personal information such as
        name, email and cell phone number will be shared with other public health
        service agencies to query your health information and notify you of updates.
        Your personal information will not be used or disclosed for any other
        purposes.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own notes
        to provide more information related to your health care. Use of these
        features is entirely voluntary. Any notes will be stored in the Health
        Gateway until you choose to delete your account or remove specific notes.
        Any notes that you create can only be accessed by you securely using your BC
        Services Card.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please direct your inquiries to the Health Gateway team:
    </p>
    <p>
        <i>
            Nino Samson<br />
            Product Owner, Health Gateway<br />
            Telephone: <a href="tel:778-974-2712">778-974-2712</a><br />
            Email: <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a
            ><br />
            Address: 1483 Douglas Street; PO BOX 9635 STN PROV GOVT, Victoria BC<br />
        </i>
    </p>
    <p><strong>5. Privacy Protection Guidance for Minors</strong></p>
    <p>
        Health Gateway takes the protection of your privacy very seriously and will
        make sure that your information is secure in the Health Gateway portal,
        however you will also need to take steps to protect your information. There
        may be consequences to sharing your device with others, for example people
        may be able to open your Health Gateway account and see whether you are
        taking any prescriptions, such as birth control medication or whether you
        have a positive COVID-19 test result.
    </p>
    <p>Protect your privacy by doing the following:</p>
    <p>
        It is not recommended that you download the BC Services Card app and set up
        a Health Gateway account if you cannot fully control who has access to your
        device. If you are unable to control who has access to your device, for
        example you do not own your phone or you share your phone or password with
        others, then the security of your information could be at risk. If this is a
        shared device, or one you do not fully control, there are other options to
        access your information. For example, to access your COVID-19 test results,
        please visit the BC Centre for Disease Control (BCCDC) website.
    </p>
    <p>
        Before downloading the BC Services Card app and setting up a Health Gateway
        account, make sure you have a secure password on your device that no one but
        you knows. For tips on how to create a secure password and password
        protection, please go to
        <a
            href="https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins"
            >https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins</a
        >.
    </p>
    <p>
        Make sure to log out of your account and close out of the screen once you
        are finished using the Health Gateway portal and lock and store your device
        securely when it is not in use.
    </p>
    <p>
        If you suspect your personal information has been viewed without your
        permission, please reach out to a trusted adult for help on what to do next.
        For more information, adults may go to
        <a
            href="https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/"
            >https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/</a
        >.
    </p>
    <p>
        Be suspicious of any unusual requests for your information. For example,
        Health Gateway will never ask you for any of your personal health
        information such as medical diagnosis.
    </p>
    <p>
        For more tips on how to protect your privacy go to
        <a href="https://www.getcybersafe.gc.ca/en/home"
            >https://www.getcybersafe.gc.ca/en/home</a
        >.
    </p>
    <p>
        If you notice anything unusual in your account or have any privacy
        questions, please contact the Health Gateway team below for help.
    </p>
    <p>
        Email: <a href="mailto:HealthGateway@gov.bc.ca">HealthGateway@gov.bc.ca</a>
    </p>
    <p><strong>6. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided "as is"
        without warranty of any kind, whether express or implied. All implied
        warranties, including, without limitation, implied warranties of
        merchantability, fitness for a particular purpose, and non-infringement, are
        hereby expressly disclaimed.
    </p>
    <p><strong>7. Attestation of Guardianship</strong></p>
    <p>
        By providing the child’s name, date of birth, personal health number and
        date of COVID-19 test, I declare that I am the child’s legal guardian as per
        the Family Law Act, the Adoption Act and/or the Child, Family and Community
        Services Act, and am attesting that I have the authority to request and
        receive health information respecting the child from third parties.
    </p>
    <p>
        If I either: (a) cease to be guardian of this child; (b) or lose the right
        to request or receive health information from third parties respecting this
        child, I will remove them as a dependent under my Health Gateway account
        immediately.
    </p>
    <p>
        I understand that I will no longer be able to access my child’s COVID-19
        test results once they are 12 years of age. I understand it is a legal
        offence to falsely claim guardianship or access another individual’s
        personal health information without legal authority or consent.
    </p>
    <p><strong>8. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of the Service or any
        website or system to which this Service may be linked, including, without
        limitation, any lost profits, business interruption, or loss of programs or
        information, even if the Government of British Columbia has been
        specifically advised of the possibility of such damages.
    </p>
    <p><strong>9. About these Terms and Conditions</strong></p>
    <p>
        We may modify these terms of service, or any additional terms that apply to
        the Service, at any time, for example to reflect changes to the law or
        changes to the Service. You should review these terms of service regularly.
        Changes to these terms of service will be effective immediately after they
        are posted. If you do not agree to any changes to these terms, you should
        discontinue your use of the Service immediately.
    </p>
    <p>
        If there is any conflict between these terms of service and any additional
        terms of service, the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be construed in accordance
        with the laws of British Columbia and the federal laws of Canada applicable
        therein.
    </p>
    ', 'System', TIMESTAMP '2020-12-24T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20201224191731_FixTermsOfService') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20201224191731_FixTermsOfService', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210105213714_UpdateToSDate') THEN
    UPDATE gateway."LegalAgreement" SET "EffectiveDate" = TIMESTAMP '2020-01-06T00:00:00', "UpdatedDateTime" = TIMESTAMP '2020-01-05T00:00:00'
    WHERE "LegalAgreementsId" = 'c99fd839-b4a2-40f9-b103-529efccd0dcd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210105213714_UpdateToSDate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210105213714_UpdateToSDate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210105220604_UpdateFeedbackFK') THEN
    ALTER TABLE gateway."UserFeedback" DROP CONSTRAINT "FK_UserFeedback_UserProfile_UserProfileId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210105220604_UpdateFeedbackFK') THEN
    DROP INDEX gateway."IX_UserFeedback_UserProfileId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210105220604_UpdateFeedbackFK') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210105220604_UpdateFeedbackFK', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210106170417_UpdateToSDate2') THEN
    UPDATE gateway."LegalAgreement" SET "EffectiveDate" = TIMESTAMP '2021-01-07T00:00:00', "UpdatedDateTime" = TIMESTAMP '2021-01-06T00:00:00'
    WHERE "LegalAgreementsId" = 'c99fd839-b4a2-40f9-b103-529efccd0dcd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210106170417_UpdateToSDate2') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210106170417_UpdateToSDate2', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210107205552_UserProfileHistoryLogin') THEN

                    UPDATE gateway."UserProfile" AS up1
                    SET "LastLoginDateTime" = up2."CreatedDateTime", "UpdatedBy" = 'UserProfileHistoryLoginMigration', "UpdatedDateTime"=now()
                    FROM gateway."UserProfile" AS up2
                    WHERE up1."LastLoginDateTime" is NULL AND up1."UserProfileId" = up2."UserProfileId";
                    
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210107205552_UserProfileHistoryLogin') THEN
    UPDATE gateway."UserProfile" SET "LastLoginDateTime" = TIMESTAMP '-infinity' WHERE "LastLoginDateTime" IS NULL;
    ALTER TABLE gateway."UserProfile" ALTER COLUMN "LastLoginDateTime" SET NOT NULL;
    ALTER TABLE gateway."UserProfile" ALTER COLUMN "LastLoginDateTime" SET DEFAULT TIMESTAMP '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210107205552_UserProfileHistoryLogin') THEN

    CREATE or REPLACE FUNCTION gateway."UserProfileHistoryFunction"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    BEGIN
        IF(TG_OP = 'DELETE') THEN
            INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                        "UserProfileId", "AcceptedTermsOfService", "Email", "ClosedDateTime", "IdentityManagementId",
                        "EncryptionKey", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "SMSNumber") 
    		VALUES(uuid_generate_v4(), TG_OP, now(),
                   old."UserProfileId", old."AcceptedTermsOfService", old."Email", old."ClosedDateTime", old."IdentityManagementId",
                   old."EncryptionKey", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime", old."SMSNumber");
            RETURN old;
        ELSEIF(TG_OP = 'UPDATE') THEN
            INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                        "UserProfileId", "AcceptedTermsOfService", "Email", "ClosedDateTime", "IdentityManagementId",
                        "EncryptionKey", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "SMSNumber") 
    		VALUES(uuid_generate_v4(), TG_OP || '_LOGIN', now(),
                   old."UserProfileId", old."AcceptedTermsOfService", old."Email", old."ClosedDateTime", old."IdentityManagementId",
                   old."EncryptionKey", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime", old."SMSNumber");
            RETURN old;
        END IF;
    END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210107205552_UserProfileHistoryLogin') THEN

        CREATE TRIGGER "UserProfileHistoryLoginTrigger"
        AFTER UPDATE OF "LastLoginDateTime"
        ON gateway."UserProfile"
        FOR EACH ROW
        EXECUTE PROCEDURE gateway."UserProfileHistoryFunction"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210107205552_UserProfileHistoryLogin') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210107205552_UserProfileHistoryLogin', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210107212709_UpdateToSEmail') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head>
    </head>
    <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style="background:#036;">
                <th width="45"></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Hi,</h1>
                    <p>
                        You are receiving this email as a user of the Health Gateway. We have updated our Terms of Service, effective ${effectivedate}.
                    </p>
                    <p>For more information, we encourage you to review the full <a href="${host}/${path}">Terms of Service</a> and check out the <a href="${host}/release-notes">release notes</a> for a summary of new features.</p>
                    <p>If you have any questions or wish to provide any feedback, please contact <a href="mailto:${contactemail}">${contactemail}</a>.</p>
                    <p>Regards,</p>
                    <p>Health Gateway Team</p>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = 'eb695050-e2fb-4933-8815-3d4656e4541d';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210107212709_UpdateToSEmail') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210107212709_UpdateToSEmail', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210111203912_UserProfileHistoryNullValue') THEN
    UPDATE gateway."UserProfileHistory" SET "LastLoginDateTime" = TIMESTAMP '-infinity' WHERE "LastLoginDateTime" IS NULL;
    ALTER TABLE gateway."UserProfileHistory" ALTER COLUMN "LastLoginDateTime" SET NOT NULL;
    ALTER TABLE gateway."UserProfileHistory" ALTER COLUMN "LastLoginDateTime" SET DEFAULT TIMESTAMP '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210111203912_UserProfileHistoryNullValue') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210111203912_UserProfileHistoryNullValue', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210119080944_EventLog') THEN
    CREATE TABLE gateway."EventLog" (
        "EventLogId" uuid NOT NULL,
        "EventName" character varying(200) NOT NULL,
        "EventSource" character varying(300) NOT NULL,
        "EventDescription" character varying(1000) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_EventLog" PRIMARY KEY ("EventLogId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210119080944_EventLog') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210119080944_EventLog', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210122215516_AddCommentEntryTypeCode') THEN
     
                    UPDATE gateway."Comment" 
                    SET "EntryTypeCode" = 'NA', "UpdatedBy" = 'AddCommentEntryTypeCodeMigration', "UpdatedDateTime"=now()
                    WHERE "EntryTypeCode" = '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210122215516_AddCommentEntryTypeCode') THEN
    CREATE TABLE gateway."CommentEntryTypeCode" (
        "CommentEntryCode" character varying(3) NOT NULL,
        "Description" character varying(50) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_CommentEntryTypeCode" PRIMARY KEY ("CommentEntryCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210122215516_AddCommentEntryTypeCode') THEN
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('NA', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Comment for an Unknown type Entry', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Med', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Comment for a Medication Entry', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Imm', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Comment for an Immunization Entry', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Lab', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Comment for a Laboratory Entry', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Enc', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Comment for an Encounter Entry', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210122215516_AddCommentEntryTypeCode') THEN
    CREATE INDEX "IX_Comment_EntryTypeCode" ON gateway."Comment" ("EntryTypeCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210122215516_AddCommentEntryTypeCode') THEN
    ALTER TABLE gateway."Comment" ADD CONSTRAINT "FK_Comment_CommentEntryTypeCode_EntryTypeCode" FOREIGN KEY ("EntryTypeCode") REFERENCES gateway."CommentEntryTypeCode" ("CommentEntryCode") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210122215516_AddCommentEntryTypeCode') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210122215516_AddCommentEntryTypeCode', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210211202508_UpdateEmailValidationTemplate') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head></head>
    <body style = "margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
            <tr style = "background:#003366;">
                <th width="45" ></th>
                <th width="350" align="left" style="text-align:left;">
                    <div role="img" aria - label="Health Gateway Logo">
                        <img src="${ActivationHost}/Logo.png" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style = "font-size:18px;">Almost there!</h1>
                    <p>We''ve received a request to register your email address for a Health Gateway account.</p>
                    <p>To activate your account, please verify your email by clicking the link:</p>
                    <a style = "color:#1292c5;font-weight:600;" href = "${ActivationHost}/ValidateEmail/${InviteKey}"> Health Gateway Account Verification </a>
                    <p>This email verification link will expire in ${ExpiryHours} hours.</p>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = '040c2ec3-d6c0-4199-9e4b-ebe6da48d52a';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210211202508_UpdateEmailValidationTemplate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210211202508_UpdateEmailValidationTemplate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210316172822_FixMessagingVerification') THEN

    UPDATE gateway."MessagingVerification"
    SET "SMSNumber" = regexp_replace("SMSNumber", '[^0-9]', '', 'g')
    WHERE "SMSNumber" IS NOT null;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210316172822_FixMessagingVerification') THEN

    UPDATE gateway."MessagingVerification"
    SET "ExpireDate" = "CreatedDateTime" + interval '5' hour
    WHERE "VerificationType" = 'SMS' AND extract(year from "ExpireDate") = 9999;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210316172822_FixMessagingVerification') THEN
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "SMSNumber" TYPE character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210316172822_FixMessagingVerification') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210316172822_FixMessagingVerification', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210317235519_AddSpecialAuthorityCommentEntryTypeCode') THEN
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "Description", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime")
    VALUES ('SAR', 'Comment for a Special Authority Request Entry', 'System', TIMESTAMP '2019-05-01T00:00:00', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210317235519_AddSpecialAuthorityCommentEntryTypeCode') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210317235519_AddSpecialAuthorityCommentEntryTypeCode', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    DELETE FROM gateway."EmailTemplate"
    WHERE "EmailTemplateId" = '2ab5d4aa-c4c9-4324-a753-cde4e21e7612';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    DELETE FROM gateway."EmailTemplate"
    WHERE "EmailTemplateId" = '896f8f2e-3bed-400b-acaf-51dd6082b4bd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head>
        <title>Email Validation</title>
    </head>
    <body style = "margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica, Arial, Verdana, Tahoma, sans-serif;font-size:12px;" aria-describedby="Layout Table">
            <tr style = "background:#003366;">
                <th scope="col" width="45" ></th>
                <th scope="col" width="350" align="left" style="text-align:left;">
                    <div role="img" aria - label="Health Gateway Logo">
                        <img src="${ActivationHost}/Logo.png" alt="Health Gateway Logo"/>
                    </div>
                </th>
                <th scope="col" width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style = "font-size:18px;">Almost there!</h1>
                    <p>We''ve received a request to register your email address for a Health Gateway account.</p>
                    <p>To activate your account, please verify your email by clicking the link:</p>
                    <a style = "color:#1292c5;font-weight:600;" href = "${ActivationHost}/ValidateEmail/${InviteKey}"> Health Gateway Account Verification </a>
                    <p>This email verification link will expire in ${ExpiryHours} hours.</p>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = '040c2ec3-d6c0-4199-9e4b-ebe6da48d52a';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head>
        <title>Account Recovered</title>
    </head>
    <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica, Arial, Verdana, Tahoma, sans-serif;font-size:12px;" aria-describedby="Layout Table">
            <tr style="background:#036;">
                <th scope="col" width="45"></th>
                <th scope="col" width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                    </div>
                </th>
                <th scope="col" width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Hi,</h1>
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
    </html>'
    WHERE "EmailTemplateId" = '2fe8c825-d4de-4884-be6a-01a97b466425';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head>
        <title>Account Closed</title>
    </head>
    <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica, Arial, Verdana, Tahoma, sans-serif;font-size:12px;" aria-describedby="Layout Table">
            <tr style="background:#036;">
                <th scope="col" width="45"></th>
                <th scope="col" width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                    </div>
                </th>
                <th scope="col" width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Hi,</h1>
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
    </html>'
    WHERE "EmailTemplateId" = '79503a38-c14a-4992-b2fe-5586629f552e';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head>
        <title>Account Removed</title>
    </head>
    <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica, Arial, Verdana, Tahoma, sans-serif;font-size:12px;" aria-describedby="Layout Table">
            <tr style="background:#036;">
                <th scope="col" width="45"></th>
                <th scope="col" width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                    </div>
                </th>
                <th scope="col" width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Hi,</h1>
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
    </html>'
    WHERE "EmailTemplateId" = 'd9898318-4e53-4074-9979-5d24bd370055';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!doctype html>
    <html lang="en">
    <head>
        <title>Updated Terms of Service</title>
    </head>
    <body style="margin:0">
        <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica, Arial, Verdana, Tahoma, sans-serif;font-size:12px;" aria-describedby="Layout Table">
            <tr style="background:#036;">
                <th scope="col" width="45"></th>
                <th scope="col" width="350" align="left" style="text-align:left;">
                    <div role="img" aria-label="Health Gateway Logo">
                        <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                    </div>
                </th>
                <th scope="col" width=""></th>
            </tr>
            <tr>
                <td colspan="3" height="20"></td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <h1 style="font-size:18px;">Hi,</h1>
                    <p>
                        You are receiving this email as a user of the Health Gateway. We have updated our Terms of Service, effective ${effectivedate}.
                    </p>
                    <p>For more information, we encourage you to review the full <a href="${host}/${path}">Terms of Service</a> and check out the <a href="${host}/release-notes">release notes</a> for a summary of new features.</p>
                    <p>If you have any questions or wish to provide any feedback, please contact <a href="mailto:${contactemail}">${contactemail}</a>.</p>
                    <p>Regards,</p>
                    <p>Health Gateway Team</p>
                </td>
                <td></td>
            </tr>
        </table>
    </body>
    </html>'
    WHERE "EmailTemplateId" = 'eb695050-e2fb-4933-8815-3d4656e4541d';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    UPDATE gateway."LegalAgreement" SET "LegalText" = '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the <strong>"Service"</strong>) is governed by the following terms and
        conditions. Please read these terms and conditions carefully, as by using the Service you will
        be deemed to have agreed to them. If you do not agree with these terms and conditions,
        please do not use the Service.
    </p>
    <p>
    <p><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides residents of British Columbia with access to their health information
        (<strong>"Health Information"</strong>).  It allows users to, in one place, view their Health Information from
        various Provincial health information systems, empowering patients and their families to manage their health care.
    </p>
    <p><strong>2. Your use of the Service</strong></p>
    <p>
        You may only access your own Health Information using the Service.
    </p>
    <p>
        If you choose to share the Health Information accessed through this Service with others (e.g.
        with a family member or caregiver), you are responsible for all the actions they take with
        respect to the use of your Health Information.
    </p>
    <p>
        You must follow any additional terms and conditions made available to you in relation to the
        Service.
    </p>
    <p>
        Do not misuse the Service, for example by trying to access or use it using a method other
        than the interface and instructions we provide.  You may use the Service only as permitted
        by law.  We may suspend or stop providing the Service to you if you do not comply with
        these terms and conditions, or if we are investigating a suspected misuse of the Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property rights in the
        Service or the content you access.  Don’t remove, obscure, or alter any legal notices
        displayed in connection with the Service.
    </p>
    <p>
        We may stop providing the Service to you, or may add or create new limits on the Service,
        for any reason and at any time.
    </p>
    <p><strong>3. Service is not a comprehensive health record or medical advice</strong></p>
    <p>
        The Health Information accessed through this Service is not a comprehensive record of your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice or replace the care provided 
        by qualified health care professionals. If you have questions or concerns about your health, 
        please contact your care provider.
    </p>
    <p><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by the Health Gateway (Ministry of Health) and 
        Service BC under the authority of section 26(c) of the Freedom of Information and Protection 
        of Privacy Act for the purpose of providing access to your health records. Your personal 
        information such as name, email and cell phone number will be shared with other public 
        health service agencies to query your health information and notify you of updates. Your 
        personal information will not be used or disclosed for any other purposes.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own notes to provide 
        more information related to your health care. Use of these features is entirely voluntary. Any 
        notes will be stored in the Health Gateway until you choose to delete your account or 
        remove specific notes. Any notes that you create can only be accessed by you securely using 
        your BC Services Card.
    </p>
    <p>
        If you have any questions about our collection or use of personal information, please direct 
        your inquiries to the Health Gateway team:
    </p>
    <p>
        <em>
            Nino Samson<br />
            Product Owner, Health Gateway<br />
            Telephone: <a href="tel:778-974-2712">778-974-2712</a><br />
            Email: <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a><br />
            Address: 1483 Douglas Street; PO BOX 9635 STN PROV GOVT, Victoria BC<br />
        </em>
    </p>
    <p><strong>5. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided "as is" without warranty of 
        any kind, whether express or implied. All implied warranties, including, without limitation, 
        implied warranties of merchantability, fitness for a particular purpose, and non-infringement, 
        are hereby expressly disclaimed.
    </p>
    <p><strong>6. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to any person or 
        business entity for any direct, indirect, special, incidental, consequential, or other damages 
        based on any use of the Service or any website or system to which this Service may be linked, 
        including, without limitation, any lost profits, business interruption, or loss of programs or 
        information, even if the Government of British Columbia has been specifically advised of the 
        possibility of such damages.
    </p>
    <p><strong>7. About these Terms and Conditions</strong></p>
    <p>
        We may modify these terms of service, or any additional terms that apply to the Service, at 
        any time, for example to reflect changes to the law or changes to the Service.  You should 
        review these terms of service regularly.  Changes to these terms of service will be effective 
        immediately after they are posted.  If you do not agree to any changes to these terms, you 
        should discontinue your use of the Service immediately.
    </p>
    <p>
        If there is any conflict between these terms of service and any additional terms of service, 
        the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be construed in accordance with the laws of 
        British Columbia and the federal laws of Canada applicable therein.
    </p>'
    WHERE "LegalAgreementsId" = '1d94c170-5118-4aa6-ba31-e3e07274ccbd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    UPDATE gateway."LegalAgreement" SET "LegalText" = '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the <strong>"Service"</strong>) is
        governed by the following terms and conditions. Please read these terms and
        conditions carefully, as by using the Service you will be deemed to have
        agreed to them. If you do not agree with these terms and conditions, please
        do not use the Service.
    </p>
    <p></p>
    <p><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides residents of British Columbia with access to their
        health information (<strong>"Health Information"</strong>). It allows users
        to, in one place, view their Health Information from various Provincial
        health information systems, empowering patients and their families to manage
        their health care.
    </p>
    <p><strong>2. Your use of the Service</strong></p>
    <p>You may only access your own Health Information using the Service.</p>
    <p>
        If you choose to share the Health Information accessed through this Service
        with others (e.g. with a family member or caregiver), you are responsible
        for all the actions they take with respect to the use of your Health
        Information.
    </p>
    <p>
        You must follow any additional terms and conditions made available to you in
        relation to the Service.
    </p>
    <p>
        Do not misuse the Service, for example by trying to access or use it using a
        method other than the interface and instructions we provide. You may use the
        Service only as permitted by law. We may suspend or stop providing the
        Service to you if you do not comply with these terms and conditions, or if
        we are investigating a suspected misuse of the Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property
        rights in the Service or the content you access. Don’t remove, obscure, or
        alter any legal notices displayed in connection with the Service.
    </p>
    <p>
        We may stop providing the Service to you, or may add or create new limits on
        the Service, for any reason and at any time.
    </p>
    <p>
        <strong
            >3. Service is not a comprehensive health record or medical
            advice</strong
        >
    </p>
    <p>
        The Health Information accessed through this Service is not a comprehensive
        record of your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice or replace
        the care provided by qualified health care professionals. If you have
        questions or concerns about your health, please contact your care provider.
    </p>
    <p><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by the Health Gateway (Ministry
        of Health) and Service BC under the authority of section 26(c) of the
        Freedom of Information and Protection of Privacy Act for the purpose of
        providing access to your health records. Your personal information such as
        name, email and cell phone number will be shared with other public health
        service agencies to query your health information and notify you of updates.
        Your personal information will not be used or disclosed for any other
        purposes.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own notes
        to provide more information related to your health care. Use of these
        features is entirely voluntary. Any notes will be stored in the Health
        Gateway until you choose to delete your account or remove specific notes.
        Any notes that you create can only be accessed by you securely using your BC
        Services Card.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please direct your inquiries to the Health Gateway team:
    </p>
    <p>
        <em>
            Nino Samson
            <br />
            Product Owner, Health Gateway
            <br />
            Telephone: <a href="tel:778-974-2712">778-974-2712</a>
            <br />
            Email: <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a>
            <br />
            Address: 1483 Douglas Street; PO BOX 9635 STN PROV GOVT, Victoria BC
            <br />
        </em>
    </p>
    <p><strong>5. Privacy Protection Guidance for Minors</strong></p>
    <p>
        Health Gateway takes the protection of your privacy very seriously and will
        make sure that your information is secure in the Health Gateway portal,
        however you will also need to take steps to protect your information. There
        may be consequences to sharing your device with others, for example people
        may be able to open your Health Gateway account and see whether you are
        taking any prescriptions, such as birth control medication or whether you
        have a positive COVID-19 test result.
    </p>
    <p>Protect your privacy by doing the following:</p>
    <p>
        It is not recommended that you download the BC Services Card app and set up
        a Health Gateway account if you cannot fully control who has access to your
        device. If you are unable to control who has access to your device, for
        example you do not own your phone or you share your phone or password with
        others, then the security of your information could be at risk. If this is a
        shared device, or one you do not fully control, there are other options to
        access your information. For example, to access your COVID-19 test results,
        please visit the BC Centre for Disease Control (BCCDC) website.
    </p>
    <p>
        Before downloading the BC Services Card app and setting up a Health Gateway
        account, make sure you have a secure password on your device that no one but
        you knows. For tips on how to create a secure password and password
        protection, please go to
        <a
            href="https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins"
            >https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins</a
        >.
    </p>
    <p>
        Make sure to log out of your account and close out of the screen once you
        are finished using the Health Gateway portal and lock and store your device
        securely when it is not in use.
    </p>
    <p>
        If you suspect your personal information has been viewed without your
        permission, please reach out to a trusted adult for help on what to do next.
        For more information, adults may go to
        <a
            href="https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/"
            >https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/</a
        >.
    </p>
    <p>
        Be suspicious of any unusual requests for your information. For example,
        Health Gateway will never ask you for any of your personal health
        information such as medical diagnosis.
    </p>
    <p>
        For more tips on how to protect your privacy go to
        <a href="https://www.getcybersafe.gc.ca/en/home"
            >https://www.getcybersafe.gc.ca/en/home</a
        >.
    </p>
    <p>
        If you notice anything unusual in your account or have any privacy
        questions, please contact the Health Gateway team below for help.
    </p>
    <p>
        Email: <a href="mailto:HealthGateway@gov.bc.ca">HealthGateway@gov.bc.ca</a>
    </p>
    <p><strong>6. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided "as is"
        without warranty of any kind, whether express or implied. All implied
        warranties, including, without limitation, implied warranties of
        merchantability, fitness for a particular purpose, and non-infringement, are
        hereby expressly disclaimed.
    </p>
    <p><strong>7. Attestation of Guardianship</strong></p>
    <p>
        By providing the child’s name, date of birth, personal health number and
        date of COVID-19 test, I declare that I am the child’s legal guardian as per
        the Family Law Act, the Adoption Act and/or the Child, Family and Community
        Services Act, and am attesting that I have the authority to request and
        receive health information respecting the child from third parties.
    </p>
    <p>
        If I either: (a) cease to be guardian of this child; (b) or lose the right
        to request or receive health information from third parties respecting this
        child, I will remove them as a dependent under my Health Gateway account
        immediately.
    </p>
    <p>
        I understand that I will no longer be able to access my child’s COVID-19
        test results once they are 12 years of age. I understand it is a legal
        offence to falsely claim guardianship or access another individual’s
        personal health information without legal authority or consent.
    </p>
    <p><strong>8. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of the Service or any
        website or system to which this Service may be linked, including, without
        limitation, any lost profits, business interruption, or loss of programs or
        information, even if the Government of British Columbia has been
        specifically advised of the possibility of such damages.
    </p>
    <p><strong>9. About these Terms and Conditions</strong></p>
    <p>
        We may modify these terms of service, or any additional terms that apply to
        the Service, at any time, for example to reflect changes to the law or
        changes to the Service. You should review these terms of service regularly.
        Changes to these terms of service will be effective immediately after they
        are posted. If you do not agree to any changes to these terms, you should
        discontinue your use of the Service immediately.
    </p>
    <p>
        If there is any conflict between these terms of service and any additional
        terms of service, the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be construed in accordance
        with the laws of British Columbia and the federal laws of Canada applicable
        therein.
    </p>
    '
    WHERE "LegalAgreementsId" = 'c99fd839-b4a2-40f9-b103-529efccd0dcd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    UPDATE gateway."LegalAgreement" SET "LegalText" = '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the “Service”) is governed by the following terms and conditions. Please read
        these terms and conditions carefully, as by using the Service you will be deemed to have agreed to them. If you do
        not agree with these terms and conditions, please do not use the Service.
    </p>
    <p>
        <p><strong>1. The Health Gateway Service</strong></p>
        The Service provides residents of British Columbia with access to their health information (<strong>"Health
            Information"</strong>). It allows users to, in one place, view their Health Information from various Provincial
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
        <em>
            <div>Nino Samson</div>
            <div>Product Owner, Health Gateway</div>
            <div>Telephone: <a href="tel:778-974-2712">778-974-2712</a></div>
            <div>Email: <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a></div>
        </em>
    </p>
    <p><strong>5. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided " as is" without warranty of any kind, whether
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
    </p>'
    WHERE "LegalAgreementsId" = 'ec438d12-f8e2-4719-8444-28e35d34674c';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    UPDATE gateway."LegalAgreement" SET "LegalText" = '<p><strong>HealthGateway Terms of Service</strong></p>
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
        <em>
            <div>Nino Samson</div>
            <div>Product Owner, Health Gateway</div>
            <div>Telephone: 778-974-2712</div>
            <div>Email: nino.samson@gov.bc.ca</div>
        </em>
    </p>

    <p><strong>Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to any person or business entity for any
        direct, indirect, special, incidental, consequential, or other damages based on any use of this website or any other
        website to which this site is linked, including, without limitation, any lost profits, business interruption, or
        loss of programs or information, even if the Government of British Columbia has been specifically advised of the
        possibility of such damages.
    </p>'
    WHERE "LegalAgreementsId" = 'f5acf1de-2f5f-431e-955d-a837d5854182';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210330184059_UpdatedAssets') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210330184059_UpdatedAssets', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Email Validation</title>
        </head>
        <body style="margin: 0">
            <table
                style="
                    width: 100%;
                    border-spacing: 0px;
                    margin: 0;
                    color: #707070;
                    font-family: Helvetica, Arial, Verdana, Tahoma, sans-serif;
                    font-size: 12px;
                "
                aria-describedby="Layout Table"
            >
                <tr style="background: #003366">
                    <th scope="col" style="width:45px;"></th>
                    <th
                        scope="col"
                        style="text-align: left; width:350px;"
                    >
                        <div role="img" aria - label="Health Gateway Logo">
                            <img
                                src="${ActivationHost}/Logo.png"
                                alt="Health Gateway Logo"
                            />
                        </div>
                    </th>
                    <th scope="col"></th>
                </tr>
                <tr>
                    <td colspan="3" style="height:20px;"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <h1 style="font-size: 18px">Almost there!</h1>
                        <p>
                            We''ve received a request to register your email address
                            for a Health Gateway account.
                        </p>
                        <p>
                            To activate your account, please verify your email by
                            clicking the link:
                        </p>
                        <a
                            style="color: #1292c5; font-weight: 600"
                            href="${ActivationHost}/ValidateEmail/${InviteKey}"
                        >
                            Health Gateway Account Verification
                        </a>
                        <p>
                            This email verification link will expire in
                            ${ExpiryHours} hours.
                        </p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = '040c2ec3-d6c0-4199-9e4b-ebe6da48d52a';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Account Recovered</title>
        </head>
        <body style="margin: 0">
            <table
                style="
                    width: 100%;
                    border-spacing: 0px;
                    margin: 0;
                    color: #707070;
                    font-family: Helvetica, Arial, Verdana, Tahoma, sans-serif;
                    font-size: 12px;
                "
                aria-describedby="Layout Table"
            >
                <tr style="background: #036">
                    <th scope="col" style="width:45px;"></th>
                    <th
                        scope="col"
                        style="text-align: left; width:350px;"
                    >
                        <div role="img" aria-label="Health Gateway Logo">
                            <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                        </div>
                    </th>
                    <th scope="col"></th>
                </tr>
                <tr>
                    <td colspan="3" style="height:20px;"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <h1 style="font-size: 18px">Hi,</h1>
                        <p>
                            You have successfully recovered your Health Gateway
                            account. You may continue to use the service as you did
                            before.
                        </p>
                        <p>Thanks,</p>
                        <p>Health Gateway Team</p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = '2fe8c825-d4de-4884-be6a-01a97b466425';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Account Closed</title>
        </head>
        <body style="margin: 0">
            <table
                style="
                    width: 100%;
                    border-spacing: 0px;
                    margin: 0;
                    color: #707070;
                    font-family: Helvetica, Arial, Verdana, Tahoma, sans-serif;
                    font-size: 12px;
                "
                aria-describedby="Layout Table"
            >
                <tr style="background: #036">
                    <th scope="col" style="width:45px;"></th>
                    <th
                        scope="col"
                        style="text-align: left; width:350px;"
                    >
                        <div role="img" aria-label="Health Gateway Logo">
                            <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                        </div>
                    </th>
                    <th scope="col"></th>
                </tr>
                <tr>
                    <td colspan="3" style="height:20px;"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <h1 style="font-size: 18px">Hi,</h1>
                        <p>
                            You have closed your Health Gateway account. If you
                            would like to recover your account, please login to
                            Health Gateway within the next 30 days and click
                            “Recover Account”. No further action is required if you
                            want your account and personally entered information to
                            be removed from the Health Gateway after this time
                            period.
                        </p>
                        <p>Thanks,</p>
                        <p>Health Gateway Team</p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = '79503a38-c14a-4992-b2fe-5586629f552e';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Account Removed</title>
        </head>
        <body style="margin: 0">
            <table
                style="
                    width: 100%;
                    border-spacing: 0px;
                    margin: 0;
                    color: #707070;
                    font-family: Helvetica, Arial, Verdana, Tahoma, sans-serif;
                    font-size: 12px;
                "
                aria-describedby="Layout Table"
            >
                <tr style="background: #036">
                    <th scope="col" style="width:45px;"></th>
                    <th
                        scope="col"
                        style="text-align: left; width:350px;"
                    >
                        <div role="img" aria-label="Health Gateway Logo">
                            <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                        </div>
                    </th>
                    <th scope="col"></th>
                </tr>
                <tr>
                    <td colspan="3" style="height:20px;"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <h1 style="font-size: 18px">Hi,</h1>
                        <p>
                            Your Health Gateway account closure has been completed.
                            Your account and personally entered information have
                            been removed from the application. You are welcome to
                            register again for the Health Gateway in the future.
                        </p>
                        <p>Thanks,</p>
                        <p>Health Gateway Team</p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = 'd9898318-4e53-4074-9979-5d24bd370055';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Updated Terms of Service</title>
        </head>
        <body style="margin: 0">
            <table
                style="
                    width: 100%;
                    border-spacing: 0px;
                    margin: 0;
                    color: #707070;
                    font-family: Helvetica, Arial, Verdana, Tahoma, sans-serif;
                    font-size: 12px;
                "
                aria-describedby="Layout Table"
            >
                <tr style="background: #036">
                    <th scope="col" style="width:45px;"></th>
                    <th
                        scope="col"
                        style="text-align: left; width:350px;"
                    >
                        <div role="img" aria-label="Health Gateway Logo">
                            <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                        </div>
                    </th>
                    <th scope="col"></th>
                </tr>
                <tr>
                    <td colspan="3" style="height:20px;"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <h1 style="font-size: 18px">Hi,</h1>
                        <p>
                            You are receiving this email as a user of the Health
                            Gateway. We have updated our Terms of Service, effective
                            ${effectivedate}.
                        </p>
                        <p>
                            For more information, we encourage you to review the
                            full <a href="${host}/${path}">Terms of Service</a> and
                            check out the
                            <a href="${host}/release-notes">release notes</a> for a
                            summary of new features.
                        </p>
                        <p>
                            If you have any questions or wish to provide any
                            feedback, please contact
                            <a href="mailto:${contactemail}">${contactemail}</a>.
                        </p>
                        <p>Regards,</p>
                        <p>Health Gateway Team</p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = 'eb695050-e2fb-4933-8815-3d4656e4541d';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    UPDATE gateway."LegalAgreement" SET "LegalText" = '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the <strong>"Service"</strong>) is
        governed by the following terms and conditions. Please read these terms and
        conditions carefully, as by using the Service you will be deemed to have
        agreed to them. If you do not agree with these terms and conditions, please
        do not use the Service.
    </p>
    <p><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides residents of British Columbia with access to their
        health information (<strong>"Health Information"</strong>). It allows users
        to, in one place, view their Health Information from various Provincial
        health information systems, empowering patients and their families to manage
        their health care.
    </p>
    <p><strong>2. Your use of the Service</strong></p>
    <p>You may only access your own Health Information using the Service.</p>
    <p>
        If you choose to share the Health Information accessed through this Service
        with others (e.g. with a family member or caregiver), you are responsible
        for all the actions they take with respect to the use of your Health
        Information.
    </p>
    <p>
        You must follow any additional terms and conditions made available to you in
        relation to the Service.
    </p>
    <p>
        Do not misuse the Service, for example by trying to access or use it using a
        method other than the interface and instructions we provide. You may use the
        Service only as permitted by law. We may suspend or stop providing the
        Service to you if you do not comply with these terms and conditions, or if
        we are investigating a suspected misuse of the Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property
        rights in the Service or the content you access. Don’t remove, obscure, or
        alter any legal notices displayed in connection with the Service.
    </p>
    <p>
        We may stop providing the Service to you, or may add or create new limits on
        the Service, for any reason and at any time.
    </p>
    <p>
        <strong
            >3. Service is not a comprehensive health record or medical
            advice</strong
        >
    </p>
    <p>
        The Health Information accessed through this Service is not a comprehensive
        record of your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice or replace
        the care provided by qualified health care professionals. If you have
        questions or concerns about your health, please contact your care provider.
    </p>
    <p><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by the Health Gateway (Ministry
        of Health) and Service BC under the authority of section 26(c) of the
        Freedom of Information and Protection of Privacy Act for the purpose of
        providing access to your health records. Your personal information such as
        name, email and cell phone number will be shared with other public health
        service agencies to query your health information and notify you of updates.
        Your personal information will not be used or disclosed for any other
        purposes.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own notes
        to provide more information related to your health care. Use of these
        features is entirely voluntary. Any notes will be stored in the Health
        Gateway until you choose to delete your account or remove specific notes.
        Any notes that you create can only be accessed by you securely using your BC
        Services Card.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please direct your inquiries to the Health Gateway team:
    </p>
    <p>
        <em>
            Nino Samson<br />
            Product Owner, Health Gateway<br />
            Telephone: <a href="tel:778-974-2712">778-974-2712</a><br />
            Email: <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a
            ><br />
            Address: 1483 Douglas Street; PO BOX 9635 STN PROV GOVT, Victoria BC<br />
        </em>
    </p>
    <p><strong>5. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided "as is"
        without warranty of any kind, whether express or implied. All implied
        warranties, including, without limitation, implied warranties of
        merchantability, fitness for a particular purpose, and non-infringement, are
        hereby expressly disclaimed.
    </p>
    <p><strong>6. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of the Service or any
        website or system to which this Service may be linked, including, without
        limitation, any lost profits, business interruption, or loss of programs or
        information, even if the Government of British Columbia has been
        specifically advised of the possibility of such damages.
    </p>
    <p><strong>7. About these Terms and Conditions</strong></p>
    <p>
        We may modify these terms of service, or any additional terms that apply to
        the Service, at any time, for example to reflect changes to the law or
        changes to the Service. You should review these terms of service regularly.
        Changes to these terms of service will be effective immediately after they
        are posted. If you do not agree to any changes to these terms, you should
        discontinue your use of the Service immediately.
    </p>
    <p>
        If there is any conflict between these terms of service and any additional
        terms of service, the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be construed in accordance
        with the laws of British Columbia and the federal laws of Canada applicable
        therein.
    </p>
    '
    WHERE "LegalAgreementsId" = '1d94c170-5118-4aa6-ba31-e3e07274ccbd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    UPDATE gateway."LegalAgreement" SET "LegalText" = '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the <strong>"Service"</strong>) is
        governed by the following terms and conditions. Please read these terms and
        conditions carefully, as by using the Service you will be deemed to have
        agreed to them. If you do not agree with these terms and conditions, please
        do not use the Service.
    </p>
    <p><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides residents of British Columbia with access to their
        health information (<strong>"Health Information"</strong>). It allows users
        to, in one place, view their Health Information from various Provincial
        health information systems, empowering patients and their families to manage
        their health care.
    </p>
    <p><strong>2. Your use of the Service</strong></p>
    <p>You may only access your own Health Information using the Service.</p>
    <p>
        If you choose to share the Health Information accessed through this Service
        with others (e.g. with a family member or caregiver), you are responsible
        for all the actions they take with respect to the use of your Health
        Information.
    </p>
    <p>
        You must follow any additional terms and conditions made available to you in
        relation to the Service.
    </p>
    <p>
        Do not misuse the Service, for example by trying to access or use it using a
        method other than the interface and instructions we provide. You may use the
        Service only as permitted by law. We may suspend or stop providing the
        Service to you if you do not comply with these terms and conditions, or if
        we are investigating a suspected misuse of the Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property
        rights in the Service or the content you access. Don’t remove, obscure, or
        alter any legal notices displayed in connection with the Service.
    </p>
    <p>
        We may stop providing the Service to you, or may add or create new limits on
        the Service, for any reason and at any time.
    </p>
    <p>
        <strong
            >3. Service is not a comprehensive health record or medical
            advice</strong
        >
    </p>
    <p>
        The Health Information accessed through this Service is not a comprehensive
        record of your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice or replace
        the care provided by qualified health care professionals. If you have
        questions or concerns about your health, please contact your care provider.
    </p>
    <p><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by the Health Gateway (Ministry
        of Health) and Service BC under the authority of section 26(c) of the
        Freedom of Information and Protection of Privacy Act for the purpose of
        providing access to your health records. Your personal information such as
        name, email and cell phone number will be shared with other public health
        service agencies to query your health information and notify you of updates.
        Your personal information will not be used or disclosed for any other
        purposes.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own notes
        to provide more information related to your health care. Use of these
        features is entirely voluntary. Any notes will be stored in the Health
        Gateway until you choose to delete your account or remove specific notes.
        Any notes that you create can only be accessed by you securely using your BC
        Services Card.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please direct your inquiries to the Health Gateway team:
    </p>
    <p>
        <em>
            Nino Samson
            <br />
            Product Owner, Health Gateway
            <br />
            Telephone: <a href="tel:778-974-2712">778-974-2712</a>
            <br />
            Email: <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a>
            <br />
            Address: 1483 Douglas Street; PO BOX 9635 STN PROV GOVT, Victoria BC
            <br />
        </em>
    </p>
    <p><strong>5. Privacy Protection Guidance for Minors</strong></p>
    <p>
        Health Gateway takes the protection of your privacy very seriously and will
        make sure that your information is secure in the Health Gateway portal,
        however you will also need to take steps to protect your information. There
        may be consequences to sharing your device with others, for example people
        may be able to open your Health Gateway account and see whether you are
        taking any prescriptions, such as birth control medication or whether you
        have a positive COVID-19 test result.
    </p>
    <p>Protect your privacy by doing the following:</p>
    <p>
        It is not recommended that you download the BC Services Card app and set up
        a Health Gateway account if you cannot fully control who has access to your
        device. If you are unable to control who has access to your device, for
        example you do not own your phone or you share your phone or password with
        others, then the security of your information could be at risk. If this is a
        shared device, or one you do not fully control, there are other options to
        access your information. For example, to access your COVID-19 test results,
        please visit the BC Centre for Disease Control (BCCDC) website.
    </p>
    <p>
        Before downloading the BC Services Card app and setting up a Health Gateway
        account, make sure you have a secure password on your device that no one but
        you knows. For tips on how to create a secure password and password
        protection, please go to
        <a
            href="https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins"
            >https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins</a
        >.
    </p>
    <p>
        Make sure to log out of your account and close out of the screen once you
        are finished using the Health Gateway portal and lock and store your device
        securely when it is not in use.
    </p>
    <p>
        If you suspect your personal information has been viewed without your
        permission, please reach out to a trusted adult for help on what to do next.
        For more information, adults may go to
        <a
            href="https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/"
            >https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/</a
        >.
    </p>
    <p>
        Be suspicious of any unusual requests for your information. For example,
        Health Gateway will never ask you for any of your personal health
        information such as medical diagnosis.
    </p>
    <p>
        For more tips on how to protect your privacy go to
        <a href="https://www.getcybersafe.gc.ca/en/home"
            >https://www.getcybersafe.gc.ca/en/home</a
        >.
    </p>
    <p>
        If you notice anything unusual in your account or have any privacy
        questions, please contact the Health Gateway team below for help.
    </p>
    <p>
        Email: <a href="mailto:HealthGateway@gov.bc.ca">HealthGateway@gov.bc.ca</a>
    </p>
    <p><strong>6. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided "as is"
        without warranty of any kind, whether express or implied. All implied
        warranties, including, without limitation, implied warranties of
        merchantability, fitness for a particular purpose, and non-infringement, are
        hereby expressly disclaimed.
    </p>
    <p><strong>7. Attestation of Guardianship</strong></p>
    <p>
        By providing the child’s name, date of birth, personal health number and
        date of COVID-19 test, I declare that I am the child’s legal guardian as per
        the Family Law Act, the Adoption Act and/or the Child, Family and Community
        Services Act, and am attesting that I have the authority to request and
        receive health information respecting the child from third parties.
    </p>
    <p>
        If I either: (a) cease to be guardian of this child; (b) or lose the right
        to request or receive health information from third parties respecting this
        child, I will remove them as a dependent under my Health Gateway account
        immediately.
    </p>
    <p>
        I understand that I will no longer be able to access my child’s COVID-19
        test results once they are 12 years of age. I understand it is a legal
        offence to falsely claim guardianship or access another individual’s
        personal health information without legal authority or consent.
    </p>
    <p><strong>8. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of the Service or any
        website or system to which this Service may be linked, including, without
        limitation, any lost profits, business interruption, or loss of programs or
        information, even if the Government of British Columbia has been
        specifically advised of the possibility of such damages.
    </p>
    <p><strong>9. About these Terms and Conditions</strong></p>
    <p>
        We may modify these terms of service, or any additional terms that apply to
        the Service, at any time, for example to reflect changes to the law or
        changes to the Service. You should review these terms of service regularly.
        Changes to these terms of service will be effective immediately after they
        are posted. If you do not agree to any changes to these terms, you should
        discontinue your use of the Service immediately.
    </p>
    <p>
        If there is any conflict between these terms of service and any additional
        terms of service, the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be construed in accordance
        with the laws of British Columbia and the federal laws of Canada applicable
        therein.
    </p>
    '
    WHERE "LegalAgreementsId" = 'c99fd839-b4a2-40f9-b103-529efccd0dcd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    UPDATE gateway."LegalAgreement" SET "LegalText" = '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the “Service”) is governed by the
        following terms and conditions. Please read these terms and conditions
        carefully, as by using the Service you will be deemed to have agreed to
        them. If you do not agree with these terms and conditions, please do not use
        the Service.
    </p>
    <p><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides residents of British Columbia with access to their
        health information (<strong>"Health Information"</strong>). It allows users
        to, in one place, view their Health Information from various Provincial
        health information systems, empowering patients and their families to manage
        their health care.
    </p>
    <p><strong>2. Your use of the Service </strong></p>
    <p>You may only access your own Health Information using the Service.</p>
    <p>
        If you choose to share the Health Information accessed through this Service
        with others (e.g. with a family member or caregiver), you are responsible
        for all the actions they take with respect to the use of your Health
        Information.
    </p>
    <p>
        You must follow any additional terms and conditions made available to you in
        relation to the Service.
    </p>
    <p>
        Do not misuse the Service, for example by trying to access or use it using a
        method other than the interface and instructions we provide. You may use the
        Service only as permitted by law. We may suspend or stop providing the
        Service to you if you do not comply with these terms and conditions, or if
        we are investigating a suspected misuse of the Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property
        rights in the Service or the content you access. Don’t remove, obscure, or
        alter any legal notices displayed in connection with the Service.
    </p>
    <p>
        We may stop providing the Service to you, or may add or create new limits on
        the Service, for any reason and at any time.
    </p>
    <p>
        <strong
            >3. Service is not a comprehensive health record or medical
            advice</strong
        >
    </p>
    <p>
        The Health Information accessed through this Service is not a comprehensive
        record of your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice or replace
        the care provided by qualified health care professionals. If you have
        questions or concerns about your health, please contact your care provider.
    </p>
    <p><strong>4. Privacy Notice</strong></p>
    <p>
        The personal information you provide the Service (Name and Email) will be
        used for the purpose of connecting your Health Gateway account to your BC
        Services Card account under the authority of section 26(c) of the Freedom of
        Information and Protection of Privacy Act. Once your BC Services Card is
        verified by the Service, you will be able to view your Health Information
        using the Service. The Service’s collection of your personal information is
        under the authority of section 26(c) of the Freedom of Information and
        Protection of Privacy Act.
    </p>
    <p>
        The Service’s notes feature allows you to enter your own notes to provide
        more information related to your health care. Use of this feature is
        entirely voluntary. Any notes will be stored in the Health Gateway in
        perpetuity, or until you choose to delete your account or remove specific
        notes. Any notes that you create can only be accessed by you securely using
        your BC Services Card.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please direct your inquiries to the Health Gateway team:
    </p>
    <p>
        <em>
            <div>Nino Samson</div>
            <div>Product Owner, Health Gateway</div>
            <div>Telephone: <a href="tel:778-974-2712">778-974-2712</a></div>
            <div>
                Email:
                <a href="mailto:nino.samson@gov.bc.ca">nino.samson@gov.bc.ca</a>
            </div>
        </em>
    </p>
    <p><strong>5. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided " as is"
        without warranty of any kind, whether express or implied. All implied
        warranties, including, without limitation, implied warranties of
        merchantability, fitness for a particular purpose, and non-infringement, are
        hereby expressly disclaimed.
    </p>
    <p><strong>6. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of the Service or any
        website or system to which this Service may be linked, including, without
        limitation, any lost profits, business interruption, or loss of programs or
        information, even if the Government of British Columbia has been
        specifically advised of the possibility of such damages.
    </p>
    <p><strong>7. About these Terms and Conditions</strong></p>
    <p>
        We may modify these terms and conditions, or any additional terms and
        conditions that apply to the Service, at any time, for example to reflect
        changes to the law or changes to the Service. You should review these terms
        and conditions regularly. Changes to these terms and conditions will be
        effective immediately after they are posted. If you do not agree to any
        changes to these terms, you should discontinue your use of the Service
        immediately. If there is any conflict between these terms and conditions and
        any additional terms and conditions, the additional terms and conditions
        will prevail. These terms and conditions are governed by and to be construed
        in accordance with the laws of British Columbia and the federal laws of
        Canada applicable therein.
    </p>
    '
    WHERE "LegalAgreementsId" = 'ec438d12-f8e2-4719-8444-28e35d34674c';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    UPDATE gateway."LegalAgreement" SET "LegalText" = '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of this service is governed by the following terms and conditions.
        Please read these terms and conditions carefully, as by using this website
        you will be deemed to have agreed to them. If you do not agree with these
        terms and conditions, do not use this service.
    </p>
    <p>
        The Health Gateway provides BC residents with access to their health
        information empowering patients and their families to manage their health
        care. In accessing your health information through this service, you
        acknowledge that the information within does not represent a comprehensive
        record of your health care in BC. No personal health information will be
        stored within the Health Gateway application. Each time you login, your
        health information will be fetched from information systems within BC and
        purged upon logout. If you choose to share your health information accessed
        through the website with a family member or caregiver, you are responsible
        for all the actions they take with respect to the use of your information.
    </p>
    <p>
        This service is not intended to provide you with medical advice nor replace
        the care provided by qualified health care professionals. If you have
        questions or concerns about your health, please contact your care provider.
    </p>
    <p>
        The personal information you provide (Name and Email) will be used for the
        purpose of connecting your Health Gateway account to your BC Services Card
        account under the authority of section 33(a) of the Freedom of Information
        and Protection of Privacy Act. This will be done through the BC Services
        Identity Assurance Service. Once your identity is verified using your BC
        Services Card, you will be able to view your health records from various
        health information systems in one place. Health Gateway’s collection of your
        personal information is under the authority of section 26(c) of the Freedom
        of Information and Protection of Privacy Act.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please direct your inquiries to the Health Gateway team:
    </p>
    <p>
        <em>
            <div>Nino Samson</div>
            <div>Product Owner, Health Gateway</div>
            <div>Telephone: 778-974-2712</div>
            <div>Email: nino.samson@gov.bc.ca</div>
        </em>
    </p>

    <p><strong>Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of this website or any
        other website to which this site is linked, including, without limitation,
        any lost profits, business interruption, or loss of programs or information,
        even if the Government of British Columbia has been specifically advised of
        the possibility of such damages.
    </p>
    '
    WHERE "LegalAgreementsId" = 'f5acf1de-2f5f-431e-955d-a837d5854182';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210420220008_UpdateAssets') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210420220008_UpdateAssets', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210510213821_CreateAdminTag') THEN
    CREATE TABLE gateway."AdminTag" (
        "AdminTagId" uuid NOT NULL,
        "Name" character varying(20) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_AdminTag" PRIMARY KEY ("AdminTagId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210510213821_CreateAdminTag') THEN
    CREATE TABLE gateway."UserFeedbackTag" (
        "UserFeedbackTagId" uuid NOT NULL,
        "AdminTagId" uuid NOT NULL,
        "UserFeedbackId" uuid NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_UserFeedbackTag" PRIMARY KEY ("UserFeedbackTagId"),
        CONSTRAINT "FK_UserFeedbackTag_AdminTag_AdminTagId" FOREIGN KEY ("AdminTagId") REFERENCES gateway."AdminTag" ("AdminTagId") ON DELETE CASCADE,
        CONSTRAINT "FK_UserFeedbackTag_UserFeedback_UserFeedbackId" FOREIGN KEY ("UserFeedbackId") REFERENCES gateway."UserFeedback" ("UserFeedbackId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210510213821_CreateAdminTag') THEN
    CREATE UNIQUE INDEX "IX_AdminTag_Name" ON gateway."AdminTag" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210510213821_CreateAdminTag') THEN
    CREATE UNIQUE INDEX "IX_UserFeedbackTag_AdminTagId_UserFeedbackId" ON gateway."UserFeedbackTag" ("AdminTagId", "UserFeedbackId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210510213821_CreateAdminTag') THEN
    CREATE INDEX "IX_UserFeedbackTag_UserFeedbackId" ON gateway."UserFeedbackTag" ("UserFeedbackId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210510213821_CreateAdminTag') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210510213821_CreateAdminTag', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    CREATE TABLE gateway."WalletConnectionStatusCode" (
        "StatusCode" character varying(15) NOT NULL,
        "Description" character varying(50) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_WalletConnectionStatusCode" PRIMARY KEY ("StatusCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    CREATE TABLE gateway."WalletCredentialStatusCode" (
        "StatusCode" character varying(10) NOT NULL,
        "Description" character varying(50) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_WalletCredentialStatusCode" PRIMARY KEY ("StatusCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    CREATE TABLE gateway."WalletConnection" (
        "WalletConnectionId" uuid NOT NULL,
        "UserProfileId" character varying(54) NOT NULL,
        "Status" character varying(15) NOT NULL,
        "AgentId" uuid,
        "InvitationEndpoint" text,
        "ConnectedDateTime" timestamp without time zone,
        "DisconnectedDateTime" timestamp without time zone,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_WalletConnection" PRIMARY KEY ("WalletConnectionId"),
        CONSTRAINT "FK_WalletConnection_UserProfile_UserProfileId" FOREIGN KEY ("UserProfileId") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE CASCADE,
        CONSTRAINT "FK_WalletConnection_WalletConnectionStatusCode_Status" FOREIGN KEY ("Status") REFERENCES gateway."WalletConnectionStatusCode" ("StatusCode") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    CREATE TABLE gateway."WalletCredential" (
        "WalletCredentialId" uuid NOT NULL,
        "WalletConnectionId" uuid NOT NULL,
        "ResourceId" text NOT NULL,
        "ResourceType" text NOT NULL,
        "Status" character varying(10) NOT NULL,
        "AddedDateTime" timestamp without time zone,
        "RevokedDateTime" timestamp without time zone,
        "ExchangeId" uuid NOT NULL,
        "RevocationId" text,
        "RevocationRegistryId" text,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_WalletCredential" PRIMARY KEY ("WalletCredentialId"),
        CONSTRAINT "FK_WalletCredential_WalletConnection_WalletConnectionId" FOREIGN KEY ("WalletConnectionId") REFERENCES gateway."WalletConnection" ("WalletConnectionId") ON DELETE CASCADE,
        CONSTRAINT "FK_WalletCredential_WalletCredentialStatusCode_Status" FOREIGN KEY ("Status") REFERENCES gateway."WalletCredentialStatusCode" ("StatusCode") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    INSERT INTO gateway."WalletConnectionStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Pending', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Wallet Connection is Pending', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."WalletConnectionStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Connected', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Wallet Connection has been created and added', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."WalletConnectionStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Disconnected', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Wallet Connection has been revoked', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    INSERT INTO gateway."WalletCredentialStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Created', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Wallet Credential has been created', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."WalletCredentialStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Added', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Credential has been added to Wallet', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."WalletCredentialStatusCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Revoked', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Credential has been revoked', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    CREATE INDEX "IX_WalletConnection_Status" ON gateway."WalletConnection" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    CREATE INDEX "IX_WalletConnection_UserProfileId" ON gateway."WalletConnection" ("UserProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    CREATE INDEX "IX_WalletCredential_Status" ON gateway."WalletCredential" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    CREATE INDEX "IX_WalletCredential_WalletConnectionId" ON gateway."WalletCredential" ("WalletConnectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210601235231_SetupWallet') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210601235231_SetupWallet', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210604003608_AddRequiredCredentialsAttributes') THEN
    UPDATE gateway."WalletCredential" SET "RevocationRegistryId" = '' WHERE "RevocationRegistryId" IS NULL;
    ALTER TABLE gateway."WalletCredential" ALTER COLUMN "RevocationRegistryId" SET NOT NULL;
    ALTER TABLE gateway."WalletCredential" ALTER COLUMN "RevocationRegistryId" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210604003608_AddRequiredCredentialsAttributes') THEN
    UPDATE gateway."WalletCredential" SET "RevocationId" = '' WHERE "RevocationId" IS NULL;
    ALTER TABLE gateway."WalletCredential" ALTER COLUMN "RevocationId" SET NOT NULL;
    ALTER TABLE gateway."WalletCredential" ALTER COLUMN "RevocationId" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210604003608_AddRequiredCredentialsAttributes') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210604003608_AddRequiredCredentialsAttributes', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210607193749_UpdateCredFieldsRegd') THEN
    ALTER TABLE gateway."WalletCredential" ALTER COLUMN "RevocationRegistryId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210607193749_UpdateCredFieldsRegd') THEN
    ALTER TABLE gateway."WalletCredential" ALTER COLUMN "RevocationId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210607193749_UpdateCredFieldsRegd') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210607193749_UpdateCredFieldsRegd', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210618155609_AdminFeedbackEmail') THEN
    INSERT INTO gateway."EmailTemplate" ("EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime")
    VALUES ('75c79b3e-1a61-403b-82ee-fddcda7144af', '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Health Gateway Feedback</title>
        </head>
        <body style="margin: 0">
            <strong>Hi Admin,</strong>
            <p>${feedback}</p>
        </body>
    </html>
    ', 'System', TIMESTAMP '2019-05-01T00:00:00', TIMESTAMP '2019-05-01T00:00:00', NULL, 'HTML', 'HG_Donotreply@gov.bc.ca', 'AdminFeedback', 1, 'Health Gateway Feedback Received', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210618155609_AdminFeedbackEmail') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210618155609_AdminFeedbackEmail', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210618185129_AddInAppCommsType') THEN
    INSERT INTO gateway."CommunicationTypeCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('InApp', 'System', TIMESTAMP '2019-05-01T00:00:00', 'In-App communication type', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210618185129_AddInAppCommsType') THEN

    CREATE OR REPLACE FUNCTION gateway."PushBannerChange"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    DECLARE
        data JSON;
        notification JSON;
        send boolean;
        BEGIN
            send = false;
            IF((TG_OP = 'INSERT' OR TG_OP = 'UPDATE') and 
                (NEW."CommunicationTypeCode" = 'Banner' or NEW."CommunicationTypeCode" = 'InApp'))
            THEN
                data = row_to_json(NEW);
                send = true;
            ELSEIF(TG_OP = 'DELETE' and 
                (OLD."CommunicationTypeCode" = 'Banner' or OLD."CommunicationTypeCode" = 'InApp')) 
            THEN
                data = row_to_json(OLD);
                send = true;
            END IF;
            IF(send) THEN
                notification = json_build_object(
                    'Table', TG_TABLE_NAME,
                    'Action', TG_OP,
                    'Data', data);
                RAISE LOG 'Sending Banner Change notification';
                PERFORM pg_notify('BannerChange', notification::TEXT);
            END IF;
            RETURN NEW;
        END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210618185129_AddInAppCommsType') THEN

    DROP TRIGGER IF EXISTS "PushBannerChange" ON gateway."Communication";
    CREATE TRIGGER "PushBannerChange"
        AFTER INSERT or UPDATE or DELETE
        ON gateway."Communication"
        FOR EACH ROW
        EXECUTE PROCEDURE gateway."PushBannerChange"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210618185129_AddInAppCommsType') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210618185129_AddInAppCommsType', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210621184624_UpdateAdminFeedbackTemplate') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Health Gateway Feedback</title>
        </head>
        <body style="margin: 0">
            <strong>Hi Health Gateway Team,</strong>
            <p>Feedback/Question received: </p>
            <p>${feedback}</p>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = '75c79b3e-1a61-403b-82ee-fddcda7144af';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210621184624_UpdateAdminFeedbackTemplate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210621184624_UpdateAdminFeedbackTemplate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210621225608_UserProfileVerifications') THEN
    CREATE INDEX "IX_MessagingVerification_HdId" ON gateway."MessagingVerification" ("HdId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210621225608_UserProfileVerifications') THEN
    ALTER TABLE gateway."MessagingVerification" ADD CONSTRAINT "FK_MessagingVerification_UserProfile_HdId" FOREIGN KEY ("HdId") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210621225608_UserProfileVerifications') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210621225608_UserProfileVerifications', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210726205526_FixMVConstraint') THEN
    ALTER TABLE gateway."MessagingVerification" DROP CONSTRAINT "FK_MessagingVerification_UserProfile_HdId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210726205526_FixMVConstraint') THEN
    UPDATE gateway."MessagingVerification" SET "HdId" = '' WHERE "HdId" IS NULL;
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "HdId" SET NOT NULL;
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "HdId" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210726205526_FixMVConstraint') THEN
    ALTER TABLE gateway."MessagingVerification" ADD CONSTRAINT "FK_MessagingVerification_UserProfile_HdId" FOREIGN KEY ("HdId") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20210726205526_FixMVConstraint') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210726205526_FixMVConstraint', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211027211258_VaccineProofRequestCache') THEN
    CREATE TABLE gateway."VaccineProofTemplateCode" (
        "TemplateCode" character varying(15) NOT NULL,
        "Description" character varying(75) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_VaccineProofTemplateCode" PRIMARY KEY ("TemplateCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211027211258_VaccineProofRequestCache') THEN
    CREATE TABLE gateway."VaccineProofRequestCache" (
        "VaccineProofRequestCacheId" uuid NOT NULL,
        "PersonIdentifier" character varying(54) NOT NULL,
        "ShcImageHash" text NOT NULL,
        "ProofTemplate" character varying(15) NOT NULL,
        "ExpiryDateTime" timestamp without time zone NOT NULL,
        "VaccineProofResponseId" text NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_VaccineProofRequestCache" PRIMARY KEY ("VaccineProofRequestCacheId"),
        CONSTRAINT "AK_VaccineProofRequestCache_PersonIdentifier_ProofTemplate_Shc~" UNIQUE ("PersonIdentifier", "ProofTemplate", "ShcImageHash"),
        CONSTRAINT "FK_VaccineProofRequestCache_VaccineProofTemplateCode_ProofTemp~" FOREIGN KEY ("ProofTemplate") REFERENCES gateway."VaccineProofTemplateCode" ("TemplateCode") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211027211258_VaccineProofRequestCache') THEN
    INSERT INTO gateway."VaccineProofTemplateCode" ("TemplateCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('BCProvincial', 'System', TIMESTAMP '2019-05-01T00:00:00', 'The Provincial template (single page)', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."VaccineProofTemplateCode" ("TemplateCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Federal', 'System', TIMESTAMP '2019-05-01T00:00:00', 'The Federal template (single page)', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."VaccineProofTemplateCode" ("TemplateCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Combined', 'System', TIMESTAMP '2019-05-01T00:00:00', 'The Combined Federal and Provincial template', 'System', TIMESTAMP '2019-05-01T00:00:00');
    INSERT INTO gateway."VaccineProofTemplateCode" ("TemplateCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('CombinedCover', 'System', TIMESTAMP '2019-05-01T00:00:00', 'The Combined Federal and Provincial template with a cover page', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211027211258_VaccineProofRequestCache') THEN
    CREATE INDEX "IX_VaccineProofRequestCache_ProofTemplate" ON gateway."VaccineProofRequestCache" ("ProofTemplate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211027211258_VaccineProofRequestCache') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20211027211258_VaccineProofRequestCache', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211028175602_VPAddAssetEndpoint') THEN
    ALTER TABLE gateway."VaccineProofRequestCache" ADD "AssetEndpoint" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211028175602_VPAddAssetEndpoint') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20211028175602_VPAddAssetEndpoint', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211103205732_CleanupVCVP') THEN
    DROP TABLE gateway."VaccineProofRequestCache";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211103205732_CleanupVCVP') THEN
    DROP TABLE gateway."WalletCredential";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211103205732_CleanupVCVP') THEN
    DROP TABLE gateway."VaccineProofTemplateCode";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211103205732_CleanupVCVP') THEN
    DROP TABLE gateway."WalletConnection";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211103205732_CleanupVCVP') THEN
    DROP TABLE gateway."WalletCredentialStatusCode";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211103205732_CleanupVCVP') THEN
    DROP TABLE gateway."WalletConnectionStatusCode";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211103205732_CleanupVCVP') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20211103205732_CleanupVCVP', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211109195934_AdminAllowOverlapMultipleStatus') THEN

                    ALTER TABLE IF EXISTS gateway."Communication" DROP CONSTRAINT IF EXISTS unique_date_range;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211109195934_AdminAllowOverlapMultipleStatus') THEN

                    ALTER TABLE gateway."Communication" ADD CONSTRAINT unique_date_range EXCLUDE USING gist (tsrange("EffectiveDateTime", "ExpiryDateTime") WITH &&) WHERE ("CommunicationTypeCode" = 'Banner'  AND "CommunicationStatusCode"  IN ('New' ,'Pending','Processed','Processing'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211109195934_AdminAllowOverlapMultipleStatus') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20211109195934_AdminAllowOverlapMultipleStatus', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211206215849_AddIndexToUserProfileHistory') THEN
    CREATE INDEX "IX_UserProfileHistory_UserProfileId" ON gateway."UserProfileHistory" ("UserProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211206215849_AddIndexToUserProfileHistory') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20211206215849_AddIndexToUserProfileHistory', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211221220853_CreateAdminUserProfile') THEN
    CREATE TABLE gateway."AdminUserProfile" (
        "AdminUserProfileId" uuid NOT NULL,
        "Username" character varying(255) NOT NULL,
        "Email" character varying(255),
        "LastLoginDateTime" timestamp without time zone NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp without time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_AdminUserProfile" PRIMARY KEY ("AdminUserProfileId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211221220853_CreateAdminUserProfile') THEN
    CREATE UNIQUE INDEX "IX_AdminUserProfile_Username" ON gateway."AdminUserProfile" ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20211221220853_CreateAdminUserProfile') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20211221220853_CreateAdminUserProfile', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220112050257_DropEmailFromAdminUserProfile') THEN
    ALTER TABLE gateway."AdminUserProfile" DROP COLUMN "Email";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220112050257_DropEmailFromAdminUserProfile') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220112050257_DropEmailFromAdminUserProfile', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220127215846_AddAllLaboratoryOrdersCommentEntryType') THEN
    UPDATE gateway."CommentEntryTypeCode" SET "Description" = 'Comment for a Covid 19 Laboratory Entry'
    WHERE "CommentEntryCode" = 'Lab';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220127215846_AddAllLaboratoryOrdersCommentEntryType') THEN
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('ALO', 'System', TIMESTAMP '2019-05-01T00:00:00', 'Comment for a Laboratory Entry', 'System', TIMESTAMP '2019-05-01T00:00:00');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220127215846_AddAllLaboratoryOrdersCommentEntryType') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220127215846_AddAllLaboratoryOrdersCommentEntryType', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    DROP TRIGGER IF EXISTS "UserProfileHistoryLoginTrigger" ON gateway."UserProfile";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Communication" DROP CONSTRAINT unique_date_range;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."VeterinarySpecies" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."VeterinarySpecies" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserProfileHistory" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserProfileHistory" ALTER COLUMN "OperationDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserProfileHistory" ALTER COLUMN "LastLoginDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserProfileHistory" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserProfileHistory" ALTER COLUMN "ClosedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserProfile" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserProfile" ALTER COLUMN "LastLoginDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserProfile" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserProfile" ALTER COLUMN "ClosedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserPreference" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserPreference" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserFeedbackTag" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserFeedbackTag" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserFeedback" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."UserFeedback" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."TherapeuticClass" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."TherapeuticClass" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Status" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Status" ALTER COLUMN "HistoryDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Status" ALTER COLUMN "ExpirationDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Status" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Schedule" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Schedule" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Route" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Route" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ResourceDelegateReasonCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ResourceDelegateReasonCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ResourceDelegateHistory" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ResourceDelegateHistory" ALTER COLUMN "OperationDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ResourceDelegateHistory" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ResourceDelegate" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ResourceDelegate" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Rating" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Rating" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ProgramTypeCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ProgramTypeCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."PharmaceuticalStd" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."PharmaceuticalStd" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."PharmaCareDrug" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."PharmaCareDrug" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Packaging" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Packaging" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Note" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Note" ALTER COLUMN "JournalDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Note" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."MessagingVerificationTypeCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."MessagingVerificationTypeCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "ExpireDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."LegalAgreementTypeCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."LegalAgreementTypeCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."LegalAgreement" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."LegalAgreement" ALTER COLUMN "EffectiveDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."LegalAgreement" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."GenericCache" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."GenericCache" ALTER COLUMN "ExpiryDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."GenericCache" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Form" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Form" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."FileDownload" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."FileDownload" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EventLog" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EventLog" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EmailTemplate" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EmailTemplate" ALTER COLUMN "ExpiryDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EmailTemplate" ALTER COLUMN "EffectiveDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EmailTemplate" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EmailStatusCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EmailStatusCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EmailFormatCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."EmailFormatCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Email" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Email" ALTER COLUMN "SentDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Email" ALTER COLUMN "LastRetryDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Email" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."DrugProduct" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."DrugProduct" ALTER COLUMN "LastUpdate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."DrugProduct" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Company" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Company" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."CommunicationTypeCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."CommunicationTypeCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."CommunicationStatusCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."CommunicationStatusCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."CommunicationEmail" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."CommunicationEmail" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Communication" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Communication" ALTER COLUMN "ScheduledDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Communication" ALTER COLUMN "ExpiryDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Communication" ALTER COLUMN "EffectiveDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Communication" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."CommentEntryTypeCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."CommentEntryTypeCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Comment" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."Comment" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AuditTransactionResultCode" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AuditTransactionResultCode" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AuditEvent" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AuditEvent" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AuditEvent" ALTER COLUMN "AuditEventDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ApplicationSetting" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ApplicationSetting" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AdminUserProfile" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AdminUserProfile" ALTER COLUMN "LastLoginDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AdminUserProfile" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AdminTag" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."AdminTag" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ActiveIngredient" ALTER COLUMN "UpdatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    ALTER TABLE gateway."ActiveIngredient" ALTER COLUMN "CreatedDateTime" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN

         CREATE TRIGGER "UserProfileHistoryLoginTrigger"
         AFTER UPDATE OF "LastLoginDateTime"
         ON gateway."UserProfile"
         FOR EACH ROW
         EXECUTE PROCEDURE gateway."UserProfileHistoryFunction"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN

         ALTER TABLE gateway."Communication"
             ADD CONSTRAINT unique_date_range EXCLUDE USING gist (
             tstzrange("EffectiveDateTime", "ExpiryDateTime") WITH &&)
             WHERE ("CommunicationTypeCode" = 'Banner' AND
                    "CommunicationStatusCode" IN ('New' ,'Pending','Processed','Processing'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220419231412_EF6Upgrade') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220419231412_EF6Upgrade', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220420211930_NoteJournalType') THEN
    ALTER TABLE gateway."Note" ALTER COLUMN "JournalDateTime" TYPE date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220420211930_NoteJournalType') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220420211930_NoteJournalType', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220420212201_RenameNoteJournal') THEN
    ALTER TABLE gateway."Note" RENAME COLUMN "JournalDateTime" TO "JournalDate";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220420212201_RenameNoteJournal') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220420212201_RenameNoteJournal', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220503003723_AddDateTruncate') THEN

    CREATE OR REPLACE FUNCTION gateway.date_trunc(
    	text,
    	timestamp with time zone)
        RETURNS timestamp with time zone
        LANGUAGE 'sql'
        IMMUTABLE STRICT PARALLEL UNSAFE
    AS $BODY$
    SELECT date_trunc($1, $2);
    $BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220503003723_AddDateTruncate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220503003723_AddDateTruncate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220510181145_UpdateParentEntryIdPrecision') THEN
    ALTER TABLE gateway."Comment" ALTER COLUMN "ParentEntryId" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220510181145_UpdateParentEntryIdPrecision') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220510181145_UpdateParentEntryIdPrecision', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518183026_UserProfileToSFK') THEN
    DROP TRIGGER IF EXISTS "UserProfileHistoryLoginTrigger" ON gateway."UserProfile";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518183026_UserProfileToSFK') THEN
    ALTER TABLE gateway."UserProfileHistory" ADD "TermsOfServiceId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518183026_UserProfileToSFK') THEN
    ALTER TABLE gateway."UserProfile" ADD "TermsOfServiceId" uuid NOT NULL DEFAULT 'c99fd839-b4a2-40f9-b103-529efccd0dcd';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518183026_UserProfileToSFK') THEN
    CREATE INDEX "IX_UserProfile_TermsOfServiceId" ON gateway."UserProfile" ("TermsOfServiceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518183026_UserProfileToSFK') THEN
    ALTER TABLE gateway."UserProfile" ADD CONSTRAINT "FK_UserProfile_LegalAgreement_TermsOfServiceId" FOREIGN KEY ("TermsOfServiceId") REFERENCES gateway."LegalAgreement" ("LegalAgreementsId") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518183026_UserProfileToSFK') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220518183026_UserProfileToSFK', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518211625_UserRemoveAcceptedToS') THEN
    ALTER TABLE gateway."UserProfile" DROP COLUMN "AcceptedTermsOfService";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518211625_UserRemoveAcceptedToS') THEN
    ALTER TABLE gateway."UserProfileHistory" ALTER COLUMN "AcceptedTermsOfService" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518211625_UserRemoveAcceptedToS') THEN

         CREATE TRIGGER "UserProfileHistoryLoginTrigger"
         AFTER UPDATE OF "LastLoginDateTime"
         ON gateway."UserProfile"
         FOR EACH ROW
         EXECUTE PROCEDURE gateway."UserProfileHistoryFunction"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518211625_UserRemoveAcceptedToS') THEN

    CREATE or REPLACE FUNCTION gateway."UserProfileHistoryFunction"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    BEGIN
        IF(TG_OP = 'DELETE') THEN
            INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                        "UserProfileId", "AcceptedTermsOfService", "TermsOfServiceId", "Email", "ClosedDateTime", "IdentityManagementId",
                        "EncryptionKey", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "SMSNumber")
    		VALUES(uuid_generate_v4(), TG_OP, now(),
                   old."UserProfileId", null, old."TermsOfServiceId", old."Email", old."ClosedDateTime", old."IdentityManagementId",
                   old."EncryptionKey", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime", old."SMSNumber");
            RETURN old;
        ELSEIF(TG_OP = 'UPDATE') THEN
            INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                        "UserProfileId", "AcceptedTermsOfService", "TermsOfServiceId", "Email", "ClosedDateTime", "IdentityManagementId",
                        "EncryptionKey", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "SMSNumber")
    		VALUES(uuid_generate_v4(), TG_OP || '_LOGIN', now(),
                   old."UserProfileId", null, old."TermsOfServiceId", old."Email", old."ClosedDateTime", old."IdentityManagementId",
                   old."EncryptionKey", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime", old."SMSNumber");
            RETURN old;
        END IF;
    END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220518211625_UserRemoveAcceptedToS') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220518211625_UserRemoveAcceptedToS', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220519190920_ToSJobCleanup') THEN
    DELETE FROM gateway."ApplicationSetting"
    WHERE "ApplicationSettingsId" = '5f279ba2-8e7b-4b1d-8c69-467d94dcb7fb';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220519190920_ToSJobCleanup') THEN
    DELETE FROM gateway."EmailTemplate"
    WHERE "EmailTemplateId" = 'eb695050-e2fb-4933-8815-3d4656e4541d';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220519190920_ToSJobCleanup') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220519190920_ToSJobCleanup', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220519201544_ToSUpdate051922') THEN
    INSERT INTO gateway."LegalAgreement" ("LegalAgreementsId", "CreatedBy", "CreatedDateTime", "EffectiveDate", "LegalAgreementCode", "LegalText", "UpdatedBy", "UpdatedDateTime")
    VALUES ('eafeee76-8a64-49ee-81ba-ddfe2c01deb8', 'System', TIMESTAMPTZ '2022-05-19T00:00:00Z', TIMESTAMPTZ '2022-05-19T00:00:00Z', 'ToS', '<p><strong>HealthGateway Terms of Service</strong></p>
    <p>
        Use of the Health Gateway service (the <strong>"Service"</strong>) is
        governed by the following terms and conditions. Please read these terms and
        conditions carefully, as by using the Service you will be deemed to have
        agreed to them. If you do not agree with these terms and conditions, please
        do not use the Service.
    </p>
    <p><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides residents of British Columbia with access to their
        health information (<strong>"Health Information"</strong>). It allows users
        to, in one place, view their Health Information from various Provincial
        health information systems, empowering patients and their families to manage
        their health care.
    </p>
    <p><strong>2. Your use of the Service</strong></p>
    <p>You may only access your own Health Information using the Service.</p>
    <p>
        If you choose to share the Health Information accessed through this Service
        with others (e.g. with a family member or caregiver), you are responsible
        for all the actions they take with respect to the use of your Health
        Information.
    </p>
    <p>
        You must follow any additional terms and conditions made available to you in
        relation to the Service.
    </p>
    <p>
        Do not misuse the Service, for example by trying to access or use it using a
        method other than the interface and instructions we provide. You may use the
        Service only as permitted by law. We may suspend or stop providing the
        Service to you if you do not comply with these terms and conditions, or if
        we are investigating a suspected misuse of the Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property
        rights in the Service or the content you access. Don’t remove, obscure, or
        alter any legal notices displayed in connection with the Service.
    </p>
    <p>
        We may stop providing the Service to you, or may add or create new limits on
        the Service, for any reason and at any time.
    </p>
    <p>
        <strong
            >3. Service is not a comprehensive health record or medical
            advice</strong
        >
    </p>
    <p>
        The Health Information accessed through this Service is not a comprehensive
        record of your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice or replace
        the care provided by qualified health care professionals. If you have
        questions or concerns about your health, please contact your care provider.
    </p>
    <p><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by the Health Gateway (Ministry
        of Health) and Service BC under the authority of section 26(c) of the
        Freedom of Information and Protection of Privacy Act for the purpose of
        providing access to your health records. Your personal information such as
        name, email and cell phone number will be shared with other public health
        service agencies to query your health information and notify you of updates.
        Your personal information will not be used or disclosed for any other
        purposes.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own notes
        to provide more information related to your health care. Use of these
        features is entirely voluntary. Any notes will be stored in the Health
        Gateway until you choose to delete your account or remove specific notes.
        Any notes that you create can only be accessed by you securely using your BC
        Services Card.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please direct your inquiries to the Health Gateway team:
    </p>
    <p>
        <em>
            Ministry of Health Privacy Officer
            <br />
            Telephone: <a href="tel:778-698-5849">778-698-5849</a>
            <br />
            Email: <a href="mailto:MOH.Privacy.Officer@gov.bc.ca">MOH.Privacy.Officer@gov.bc.ca</a>
            <br />
        </em>
    </p>
    <p><strong>5. Privacy Protection Guidance for Minors</strong></p>
    <p>
        Health Gateway takes the protection of your privacy very seriously and will
        make sure that your information is secure in the Health Gateway portal,
        however you will also need to take steps to protect your information. There
        may be consequences to sharing your device with others, for example people
        may be able to open your Health Gateway account and see whether you are
        taking any prescriptions, such as birth control medication or whether you
        have a positive COVID-19 test result.
    </p>
    <p>Protect your privacy by doing the following:</p>
    <p>
        It is not recommended that you download the BC Services Card app and set up
        a Health Gateway account if you cannot fully control who has access to your
        device. If you are unable to control who has access to your device, for
        example you do not own your phone or you share your phone or password with
        others, then the security of your information could be at risk. If this is a
        shared device, or one you do not fully control, there are other options to
        access your information. For example, to access your COVID-19 test results,
        please visit the BC Centre for Disease Control (BCCDC) website.
    </p>
    <p>
        Before downloading the BC Services Card app and setting up a Health Gateway
        account, make sure you have a secure password on your device that no one but
        you knows. For tips on how to create a secure password and password
        protection, please go to
        <a
            href="https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins"
            >https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins</a
        >.
    </p>
    <p>
        Make sure to log out of your account and close out of the screen once you
        are finished using the Health Gateway portal and lock and store your device
        securely when it is not in use.
    </p>
    <p>
        If you suspect your personal information has been viewed without your
        permission, please reach out to a trusted adult for help on what to do next.
        For more information, adults may go to
        <a
            href="https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/"
            >https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/</a
        >.
    </p>
    <p>
        Be suspicious of any unusual requests for your information. For example,
        Health Gateway will never ask you for any of your personal health
        information such as medical diagnosis.
    </p>
    <p>
        For more tips on how to protect your privacy go to
        <a href="https://www.getcybersafe.gc.ca/en/home"
            >https://www.getcybersafe.gc.ca/en/home</a
        >.
    </p>
    <p>
        If you notice anything unusual in your account or have any privacy
        questions, please contact the Health Gateway team below for help.
    </p>
    <p>
        Email: <a href="mailto:HealthGateway@gov.bc.ca">HealthGateway@gov.bc.ca</a>
    </p>
    <p><strong>6. Warranty Disclaimer</strong></p>
    <p>
        The Service and all of the information it contains are provided "as is"
        without warranty of any kind, whether express or implied. All implied
        warranties, including, without limitation, implied warranties of
        merchantability, fitness for a particular purpose, and non-infringement, are
        hereby expressly disclaimed.
    </p>
    <p><strong>7. Attestation of Guardianship</strong></p>
    <p>
        By providing the child’s name, date of birth, personal health number and
        date of COVID-19 test, I declare that I am the child’s legal guardian as per
        the Family Law Act, the Adoption Act and/or the Child, Family and Community
        Services Act, and am attesting that I have the authority to request and
        receive health information respecting the child from third parties.
    </p>
    <p>
        If I either: (a) cease to be guardian of this child; (b) or lose the right
        to request or receive health information from third parties respecting this
        child, I will remove them as a dependent under my Health Gateway account
        immediately.
    </p>
    <p>
        I understand that I will no longer be able to access my child’s COVID-19
        test results once they are 12 years of age. I understand it is a legal
        offence to falsely claim guardianship or access another individual’s
        personal health information without legal authority or consent.
    </p>
    <p><strong>8. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of the Service or any
        website or system to which this Service may be linked, including, without
        limitation, any lost profits, business interruption, or loss of programs or
        information, even if the Government of British Columbia has been
        specifically advised of the possibility of such damages.
    </p>
    <p><strong>9. About these Terms and Conditions</strong></p>
    <p>
        We may modify these terms of service, or any additional terms that apply to
        the Service, at any time, for example to reflect changes to the law or
        changes to the Service. You should review these terms of service regularly.
        Changes to these terms of service will be effective immediately after they
        are posted. If you do not agree to any changes to these terms, you should
        discontinue your use of the Service immediately.
    </p>
    <p>
        If there is any conflict between these terms of service and any additional
        terms of service, the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be construed in accordance
        with the laws of British Columbia and the federal laws of Canada applicable
        therein.
    </p>
    ', 'System', TIMESTAMPTZ '2022-05-19T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220519201544_ToSUpdate051922') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220519201544_ToSUpdate051922', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220606211634_RevampTermsOfService') THEN
    INSERT INTO gateway."LegalAgreement" ("LegalAgreementsId", "CreatedBy", "CreatedDateTime", "EffectiveDate", "LegalAgreementCode", "LegalText", "UpdatedBy", "UpdatedDateTime")
    VALUES ('2fab66e7-37c9-4b03-ba25-e8fad604dc7f', 'System', TIMESTAMPTZ '2022-06-07T00:00:00Z', TIMESTAMPTZ '2022-06-07T00:00:00Z', 'ToS', '<p><strong>Health Gateway Terms of Service</strong></p>
    <p><strong>Last updated: June 7, 2022</strong></p>
    <p>
        Health Gateway service (the <strong>"Service"</strong>) is governed by the
        following terms and conditions. Please read them carefully.
    </p>
    <p>
        By using Health Gateway, you are agreeing to follow and be bound by the
        Terms of Service.
    </p>
    <p>
        We may modify these terms of service at any time. You should review these
        terms of service regularly. Changes to these terms of service are effective
        immediately upon publication. If you do not agree to any changes to these
        terms, you should discontinue your use of Health Gateway right away.
    </p>
    <p>
        If there is any conflict between these terms of service and any new or
        additional terms of service, the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be interpreted in accordance
        with the laws of British Columbia and the federal laws of Canada applicable
        therein.
    </p>
    <p><strong>What’s covered in these terms:</strong></p>
    <ol>
        <li><a href="#tos-1">The Health Gateway Service</a></li>
        <li><a href="#tos-2">Your use of the Service</a></li>
        <li><a href="#tos-3">Service Limitations</a></li>
        <li><a href="#tos-4">Privacy Notice</a></li>
        <li><a href="#tos-5">Privacy Protection Guidance for Minors</a></li>
        <li><a href="#tos-6">Attestation of Guardianship</a></li>
        <li><a href="#tos-7">Warranty Disclaimer</a></li>
        <li><a href="#tos-8">Limitation of Liabilities</a></li>
    </ol>

    <p id="tos-1"><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides you with online access to your B.C. health information
        (<strong>"Health Information"</strong>). It allows you to view your Health
        Information from various Provincial health information systems in one place.
    </p>

    <p id="tos-2"><strong>2. Your use of the Service</strong></p>
    <p>
        You may only access your own or your dependents’ Health Information using
        the Service.
    </p>
    <p>
        If you choose to share the Health Information accessed through the Service
        with others (e.g., with a family member, caregiver or healthcare provider),
        Health Gateway cannot be held responsible for any actions they take with
        your Health Information.
    </p>
    <p>
        Do not misuse the Service. Do not try to access or use it in any way other
        than through the interface and instructions we provide. You may use the
        Service only as permitted by law.
    </p>
    <p>We may suspend or stop providing the Service to you if:</p>
    <ul>
        <li>you do not comply with these terms and conditions, or</li>
        <li>we are investigating a suspected misuse of the Service.</li>
    </ul>
    <p>
        We have the right to stop providing the Service to you or to create new
        Terms of Service for any reason and at any time. You must follow any
        additional terms and conditions made available to you in relation to the
        Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property
        rights in the Service or the content you access. Don’t remove, obscure, or
        alter any legal notices displayed in connection with the Service.
    </p>

    <p id="tos-3"><strong>3. Service Limitations</strong></p>
    <p>
        The Health Information accessed through this Service is not a complete
        record of all your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice and does not
        replace care from qualified health professionals. If you have questions or
        concerns about your health, please contact your doctor or care provider.
    </p>

    <p id="tos-4"><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by Health Gateway (Ministry of
        Health) and Service BC under the authority of section 26(c) of the Freedom
        of Information and Protection of Privacy Act for the purpose of providing
        access to your health records. Your personal information such as name, email
        and mobile phone number will be shared with other public health service
        agencies to query your health information and notify you of updates.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own
        private notes to record more information related to your health care. Use of
        this feature is voluntary. Any notes will be stored in Health Gateway until
        you choose to remove them or delete your account. Any notes that you create
        can only be accessed by you.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please contact:
    </p>
    <p>
        <em>
            Ministry of Health Privacy Officer
            <br />
            Telephone: <a href="tel:778-698-5849">778-698-5849</a>
            <br />
            Email:
            <a href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                >MOH.Privacy.Officer@gov.bc.ca</a
            >
        </em>
    </p>

    <p id="tos-5"><strong>5. Privacy Protection Guidance for Minors</strong></p>
    <p>The below guidance is recommended for anyone under the age of 19.</p>
    <p>
        Health Gateway takes your privacy seriously. Although Health Gateway
        includes security features, you must also take steps to protect your
        privacy:
    </p>
    <ol type="a">
        <li>
            <p>
                Do not set up a Health Gateway account if you cannot fully control
                who has access to your device. If you share your phone or password
                with others, the security of your information is at risk. Anyone who
                has access to your device may be able to open your Health Gateway
                account and see your:
            </p>
            <ul>
                <li><p>prescriptions, such as birth control medication,</p></li>
                <li><p>health visit and procedure details,</p></li>
                <li><p>vaccination status, and</p></li>
                <li>
                    <p>
                        blood test results, including sexually transmitted infection
                        testing.
                    </p>
                </li>
            </ul>
        </li>
        <li>
            <p>
                If you can’t use Health Gateway because you don’t have your own
                secure device, we can tell you about other ways to access the health
                information you need. Email us at:
                <a href="mailto:HealthGateway@gov.bc.ca">HealthGateway@gov.bc.ca</a>
            </p>
        </li>
        <li>
            <p>
                Before setting up a Health Gateway account, make sure you have a
                password on your device that only you know. For tips on how to
                create a secure password, visit
                <a
                    href="https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins"
                >
                    https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins</a
                >. Or you can secure your device with facial recognition or
                fingerprint identification.
            </p>
        </li>
        <li>
            <p>
                Make sure to log out of your account and close the screen once you
                are finished using Health Gateway. Lock and store your device
                securely when not in use.
            </p>
        </li>
        <li>
            <p>
                If you suspect your personal information has been viewed without
                your permission, please reach out to a trusted adult for help on
                what to do next. For more information, adults may go to
                <a
                    href="https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/"
                    >https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/</a
                >.
            </p>
        </li>
    </ol>
    <p>
        For more tips on how to protect your privacy, visit:
        <a href="https://www.getcybersafe.gc.ca/">https://www.getcybersafe.gc.ca</a>
    </p>
    <p>
        If you notice anything unusual in your account or if you have any privacy
        questions, contact the Health Gateway team for help:
        <a href="mailto:HealthGateway@gov.bc.ca">HealthGateway@gov.bc.ca</a>
    </p>

    <p id="tos-6">
        <strong
            >6. Attestation of Guardianship (Child under 12 years of age)</strong
        >
    </p>
    <p>
        By providing the child’s name, date of birth, personal health number and
        date of COVID-19 test, I declare that I am the child’s guardian and that I
        have the authority to request and receive health information respecting the
        child from third parties.
    </p>
    <p>
        If I either: (a) cease to be guardian of this child; (b) or lose the right
        to request or receive health information from third parties respecting this
        child, I will remove them as a dependent under my Health Gateway account
        immediately.
    </p>
    <p>
        I understand that I will no longer be able to access my child’s COVID-19
        test results once they are 12 years of age.
    </p>

    <p id="tos-7"><strong>7. Warranty Disclaimer</strong></p>
    <p>
        The Service and the information contained therein are provided "as is"
        without warranty of any kind, whether express or implied. All implied
        warranties, including, without limitation, implied warranties of
        merchantability, fitness for a particular purpose, and non-infringement, are
        hereby expressly disclaimed.
    </p>

    <p id="tos-8"><strong>8. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of the Service or any
        website or system to which this Service may be linked, including, without
        limitation, any lost profits, business interruption, or loss of programs or
        information, even if the Government of British Columbia has been
        specifically advised of the possibility of such damages.
    </p>
    ', 'System', TIMESTAMPTZ '2022-06-07T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220606211634_RevampTermsOfService') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220606211634_RevampTermsOfService', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220607220711_RemoveCommunicationEmail') THEN
    DROP TABLE gateway."CommunicationEmail";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220607220711_RemoveCommunicationEmail') THEN
    DELETE FROM gateway."CommunicationTypeCode"
    WHERE "StatusCode" = 'Email';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220607220711_RemoveCommunicationEmail') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220607220711_RemoveCommunicationEmail', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220819210700_UpdateResourceDelegate') THEN
    ALTER TABLE gateway."ResourceDelegateHistory" ALTER COLUMN "ReasonObjectType" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220819210700_UpdateResourceDelegate') THEN
    ALTER TABLE gateway."ResourceDelegateHistory" ALTER COLUMN "ReasonObject" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220819210700_UpdateResourceDelegate') THEN
    ALTER TABLE gateway."ResourceDelegate" ALTER COLUMN "ReasonObjectType" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220819210700_UpdateResourceDelegate') THEN
    ALTER TABLE gateway."ResourceDelegate" ALTER COLUMN "ReasonObject" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220819210700_UpdateResourceDelegate') THEN
    INSERT INTO gateway."ResourceDelegateReasonCode" ("ReasonTypeCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Guardian', 'System', TIMESTAMPTZ '2022-08-01T00:00:00Z', 'Resource Delegation for attested guardian', 'System', TIMESTAMPTZ '2022-08-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220819210700_UpdateResourceDelegate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220819210700_UpdateResourceDelegate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220822182613_ToSUpdate') THEN
    UPDATE gateway."LegalAgreement" SET "LegalText" = '<p><strong>Health Gateway Terms of Service</strong></p>
    <p><strong>Last updated: June 7, 2022</strong></p>
    <p>
        Health Gateway service (the <strong>"Service"</strong>) is governed by the
        following terms and conditions. Please read them carefully.
    </p>
    <p>
        By using Health Gateway, you are agreeing to follow and be bound by the
        Terms of Service.
    </p>
    <p>
        We may modify these terms of service at any time. You should review these
        terms of service regularly. Changes to these terms of service are effective
        immediately upon publication. If you do not agree to any changes to these
        terms, you should discontinue your use of Health Gateway right away.
    </p>
    <p>
        If there is any conflict between these terms of service and any new or
        additional terms of service, the additional terms of service will prevail.
    </p>
    <p>
        These terms of service are governed by and to be interpreted in accordance
        with the laws of British Columbia and the federal laws of Canada applicable
        therein.
    </p>
    <p><strong>What’s covered in these terms:</strong></p>
    <ol>
        <li><a href="#tos-1">The Health Gateway Service</a></li>
        <li><a href="#tos-2">Your use of the Service</a></li>
        <li><a href="#tos-3">Service Limitations</a></li>
        <li><a href="#tos-4">Privacy Notice</a></li>
        <li><a href="#tos-5">Privacy Protection Guidance for Minors</a></li>
        <li><a href="#tos-6">Attestation of Guardianship</a></li>
        <li><a href="#tos-7">Warranty Disclaimer</a></li>
        <li><a href="#tos-8">Limitation of Liabilities</a></li>
    </ol>

    <p id="tos-1"><strong>1. The Health Gateway Service</strong></p>
    <p>
        The Service provides you with online access to your B.C. health information
        (<strong>"Health Information"</strong>). It allows you to view your Health
        Information from various Provincial health information systems in one place.
    </p>

    <p id="tos-2"><strong>2. Your use of the Service</strong></p>
    <p>
        You may only access your own or your dependents’ Health Information using
        the Service.
    </p>
    <p>
        If you choose to share the Health Information accessed through the Service
        with others (e.g., with a family member, caregiver or healthcare provider),
        Health Gateway cannot be held responsible for any actions they take with
        your Health Information.
    </p>
    <p>
        Do not misuse the Service. Do not try to access or use it in any way other
        than through the interface and instructions we provide. You may use the
        Service only as permitted by law.
    </p>
    <p>We may suspend or stop providing the Service to you if:</p>
    <ul>
        <li>you do not comply with these terms and conditions, or</li>
        <li>we are investigating a suspected misuse of the Service.</li>
    </ul>
    <p>
        We have the right to stop providing the Service to you or to create new
        Terms of Service for any reason and at any time. You must follow any
        additional terms and conditions made available to you in relation to the
        Service.
    </p>
    <p>
        Using the Service does not give you ownership of any intellectual property
        rights in the Service or the content you access. Don’t remove, obscure, or
        alter any legal notices displayed in connection with the Service.
    </p>

    <p id="tos-3"><strong>3. Service Limitations</strong></p>
    <p>
        The Health Information accessed through this Service is not a complete
        record of all your health care in BC.
    </p>
    <p>
        This Service is not intended to provide you with medical advice and does not
        replace care from qualified health professionals. If you have questions or
        concerns about your health, please contact your doctor or care provider.
    </p>

    <p id="tos-4"><strong>4. Privacy Notice</strong></p>
    <p>
        Your personal information will be collected by Health Gateway (Ministry of
        Health) and Service BC under the authority of section 26(c) of the Freedom
        of Information and Protection of Privacy Act for the purpose of providing
        access to your health records. Your personal information such as name, email
        and mobile phone number will be shared with other public health service
        agencies to query your health information and notify you of updates.
    </p>
    <p>
        The Service’s notes and comments features allow you to enter your own
        private notes to record more information related to your health care. Use of
        this feature is voluntary. Any notes will be stored in Health Gateway until
        you choose to remove them or delete your account. Any notes that you create
        can only be accessed by you.
    </p>
    <p>
        If you have any questions about our collection or use of personal
        information, please contact:
    </p>
    <p>
        <em>
            Ministry of Health Privacy Officer
            <br />
            Telephone: <a href="tel:778-698-5849">778-698-5849</a>
            <br />
            Email:
            <a href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                >MOH.Privacy.Officer@gov.bc.ca</a
            >
        </em>
    </p>

    <p id="tos-5"><strong>5. Privacy Protection Guidance for Minors</strong></p>
    <p>The below guidance is recommended for anyone under the age of 19.</p>
    <p>
        Health Gateway takes your privacy seriously. Although Health Gateway
        includes security features, you must also take steps to protect your
        privacy:
    </p>
    <ol type="a">
        <li>
            <p>
                Do not set up a Health Gateway account if you cannot fully control
                who has access to your device. If you share your phone or password
                with others, the security of your information is at risk. Anyone who
                has access to your device may be able to open your Health Gateway
                account and see your:
            </p>
            <ul>
                <li><p>prescriptions, such as birth control medication,</p></li>
                <li><p>health visit and procedure details,</p></li>
                <li><p>vaccination status, and</p></li>
                <li>
                    <p>
                        blood test results, including sexually transmitted infection
                        testing.
                    </p>
                </li>
            </ul>
        </li>
        <li>
            <p>
                If you can’t use Health Gateway because you don’t have your own
                secure device, we can tell you about other ways to access the health
                information you need. Email us at:
                <a href="mailto:HealthGateway@gov.bc.ca">HealthGateway@gov.bc.ca</a>
            </p>
        </li>
        <li>
            <p>
                Before setting up a Health Gateway account, make sure you have a
                password on your device that only you know. For tips on how to
                create a secure password, visit
                <a
                    href="https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins"
                >
                    https://www.getcybersafe.gc.ca/en/secure-your-accounts/passphrases-passwords-and-pins</a
                >. Or you can secure your device with facial recognition or
                fingerprint identification.
            </p>
        </li>
        <li>
            <p>
                Make sure to log out of your account and close the screen once you
                are finished using Health Gateway. Lock and store your device
                securely when not in use.
            </p>
        </li>
        <li>
            <p>
                If you suspect your personal information has been viewed without
                your permission, please reach out to a trusted adult for help on
                what to do next. For more information, adults may go to
                <a
                    href="https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/"
                    >https://www.priv.gc.ca/en/privacy-topics/information-and-advice-for-individuals/privacy-and-kids/</a
                >.
            </p>
        </li>
    </ol>
    <p>
        For more tips on how to protect your privacy, visit:
        <a href="https://www.getcybersafe.gc.ca/">https://www.getcybersafe.gc.ca</a>
    </p>
    <p>
        If you notice anything unusual in your account or if you have any privacy
        questions, contact the Health Gateway team for help:
        <a href="mailto:HealthGateway@gov.bc.ca">HealthGateway@gov.bc.ca</a>
    </p>

    <p id="tos-6">
        <strong
            >6. Attestation of Guardianship (Child under 12 years of age)</strong
        >
    </p>
    <p>
        By providing the child’s name, date of birth, and personal health number,
        I declare that I am the child’s guardian and that I have the authority to
        request and receive health information respecting the child from third parties.
    </p>
    <p>
        If I either: (a) cease to be guardian of this child; (b) or lose the right
        to request or receive health information from third parties respecting this
        child, I will remove them as a dependent under my Health Gateway account
        immediately.
    </p>
    <p>
        I understand that I will no longer be able to access my child’s health records
        once they are 12 years of age.
    </p>

    <p id="tos-7"><strong>7. Warranty Disclaimer</strong></p>
    <p>
        The Service and the information contained therein are provided "as is"
        without warranty of any kind, whether express or implied. All implied
        warranties, including, without limitation, implied warranties of
        merchantability, fitness for a particular purpose, and non-infringement, are
        hereby expressly disclaimed.
    </p>

    <p id="tos-8"><strong>8. Limitation of Liabilities</strong></p>
    <p>
        Under no circumstances will the Government of British Columbia be liable to
        any person or business entity for any direct, indirect, special, incidental,
        consequential, or other damages based on any use of the Service or any
        website or system to which this Service may be linked, including, without
        limitation, any lost profits, business interruption, or loss of programs or
        information, even if the Government of British Columbia has been
        specifically advised of the possibility of such damages.
    </p>
    '
    WHERE "LegalAgreementsId" = '2fab66e7-37c9-4b03-ba25-e8fad604dc7f';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220822182613_ToSUpdate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220822182613_ToSUpdate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220823003707_RemoveGenericCache') THEN
    DROP TABLE gateway."GenericCache";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220823003707_RemoveGenericCache') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220823003707_RemoveGenericCache', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220825002518_ClinicalDocCommentEntry') THEN
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('CDO', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Comment for Clinical Documents Entry', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220825002518_ClinicalDocCommentEntry') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220825002518_ClinicalDocCommentEntry', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220830055608_NewCDOAppType') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('CDO', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Clinical Document Service', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220830055608_NewCDOAppType') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220830055608_NewCDOAppType', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220903162916_AddMobileComms') THEN
    INSERT INTO gateway."CommunicationTypeCode" ("StatusCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Mobile', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Mobile communication type', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220903162916_AddMobileComms') THEN

    CREATE OR REPLACE FUNCTION gateway."PushBannerChange"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    DECLARE
        data JSON;
        notification JSON;
        send boolean;
        BEGIN
            send = false;
            IF((TG_OP = 'INSERT' OR TG_OP = 'UPDATE') and (
                NEW."CommunicationTypeCode" = 'Banner' or
                NEW."CommunicationTypeCode" = 'InApp' or
                NEW."CommunicationTypeCode" = 'Mobile'))
            THEN
                data = row_to_json(NEW);
                send = true;
            ELSEIF(TG_OP = 'DELETE' and (
                OLD."CommunicationTypeCode" = 'Banner' or
                OLD."CommunicationTypeCode" = 'InApp' or
                OLD."CommunicationTypeCode" = 'Mobile'))
            THEN
                data = row_to_json(OLD);
                send = true;
            END IF;
            IF(send) THEN
                notification = json_build_object(
                    'Table', TG_TABLE_NAME,
                    'Action', TG_OP,
                    'Data', data);
                RAISE LOG 'Sending Banner Change notification';
                PERFORM pg_notify('BannerChange', notification::TEXT);
            END IF;
            RETURN NEW;
        END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220903162916_AddMobileComms') THEN

    DROP TRIGGER IF EXISTS "PushBannerChange" ON gateway."Communication";
    CREATE TRIGGER "PushBannerChange"
        AFTER INSERT or UPDATE or DELETE
        ON gateway."Communication"
        FOR EACH ROW
        EXECUTE PROCEDURE gateway."PushBannerChange"();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20220903162916_AddMobileComms') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220903162916_AddMobileComms', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221005233413_AddYearOfBirthToUserProfile') THEN
    ALTER TABLE gateway."UserProfileHistory" ADD "YearOfBirth" character varying(4);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221005233413_AddYearOfBirthToUserProfile') THEN
    ALTER TABLE gateway."UserProfile" ADD "YearOfBirth" character varying(4);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221005233413_AddYearOfBirthToUserProfile') THEN

    CREATE or REPLACE FUNCTION gateway."UserProfileHistoryFunction"()
        RETURNS trigger
        LANGUAGE 'plpgsql'
        NOT LEAKPROOF
    AS $BODY$
    BEGIN
        IF(TG_OP = 'DELETE') THEN
            INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                        "UserProfileId", "AcceptedTermsOfService", "TermsOfServiceId", "Email", "ClosedDateTime", "IdentityManagementId", "EncryptionKey",
                        "YearOfBirth", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "SMSNumber")
    		VALUES(uuid_generate_v4(), TG_OP, now(),
                   old."UserProfileId", null, old."TermsOfServiceId", old."Email", old."ClosedDateTime", old."IdentityManagementId", old."EncryptionKey",
                   old."YearOfBirth", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime", old."SMSNumber");
            RETURN old;
        ELSEIF(TG_OP = 'UPDATE') THEN
            INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                        "UserProfileId", "AcceptedTermsOfService", "TermsOfServiceId", "Email", "ClosedDateTime", "IdentityManagementId", "EncryptionKey",
                        "YearOfBirth", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "SMSNumber")
    		VALUES(uuid_generate_v4(), TG_OP || '_LOGIN', now(),
                   old."UserProfileId", null, old."TermsOfServiceId", old."Email", old."ClosedDateTime", old."IdentityManagementId", old."EncryptionKey",
                   old."YearOfBirth", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime", old."SMSNumber");
            RETURN old;
        END IF;
    END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221005233413_AddYearOfBirthToUserProfile') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20221005233413_AddYearOfBirthToUserProfile', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221019215540_HospitalVisitCommentEntry') THEN
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Hos', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Comment for Hospital Visits Entry', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221019215540_HospitalVisitCommentEntry') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20221019215540_HospitalVisitCommentEntry', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221208194329_AddUserProfileToUserFeedback') THEN
    CREATE INDEX "IX_UserFeedback_UserProfileId" ON gateway."UserFeedback" ("UserProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221208194329_AddUserProfileToUserFeedback') THEN
    ALTER TABLE gateway."UserFeedback" ADD CONSTRAINT "FK_UserFeedback_UserProfile_UserProfileId" FOREIGN KEY ("UserProfileId") REFERENCES gateway."UserProfile" ("UserProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221208194329_AddUserProfileToUserFeedback') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20221208194329_AddUserProfileToUserFeedback', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN
    ALTER TABLE gateway."UserProfileHistory" ADD "LastLoginClientCode" character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN
    ALTER TABLE gateway."UserProfile" ADD "LastLoginClientCode" character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN
    CREATE TABLE gateway."UserLoginClientTypeCode" (
        "UserLoginClientCode" character varying(10) NOT NULL,
        "Description" character varying(50) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_UserLoginClientTypeCode" PRIMARY KEY ("UserLoginClientCode")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN
    INSERT INTO gateway."UserLoginClientTypeCode" ("UserLoginClientCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Mobile', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Code for a login from the hg mobile app', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    INSERT INTO gateway."UserLoginClientTypeCode" ("UserLoginClientCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Web', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Code for a login from the hg web app', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN
    CREATE INDEX "IX_UserProfileHistory_LastLoginClientCode" ON gateway."UserProfileHistory" ("LastLoginClientCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN
    CREATE INDEX "IX_UserProfile_LastLoginClientCode" ON gateway."UserProfile" ("LastLoginClientCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN
    ALTER TABLE gateway."UserProfile" ADD CONSTRAINT "FK_UserProfile_UserLoginClientTypeCode_LastLoginClientCode" FOREIGN KEY ("LastLoginClientCode") REFERENCES gateway."UserLoginClientTypeCode" ("UserLoginClientCode") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN
    ALTER TABLE gateway."UserProfileHistory" ADD CONSTRAINT "FK_UserProfileHistory_UserLoginClientTypeCode_LastLoginClientC~" FOREIGN KEY ("LastLoginClientCode") REFERENCES gateway."UserLoginClientTypeCode" ("UserLoginClientCode") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN

                CREATE or REPLACE FUNCTION gateway."UserProfileHistoryFunction"()
                    RETURNS trigger
                    LANGUAGE 'plpgsql'
                    NOT LEAKPROOF
                AS $BODY$
                BEGIN
                    IF(TG_OP = 'DELETE') THEN
                        INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                                    "UserProfileId", "AcceptedTermsOfService", "TermsOfServiceId", "Email", "ClosedDateTime", "IdentityManagementId", "EncryptionKey",
                                    "YearOfBirth", "LastLoginClientCode", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "SMSNumber")
    		            VALUES(uuid_generate_v4(), TG_OP, now(),
                               old."UserProfileId", null, old."TermsOfServiceId", old."Email", old."ClosedDateTime", old."IdentityManagementId", old."EncryptionKey",
                               old."YearOfBirth", old."LastLoginClientCode", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime", old."SMSNumber");
                        RETURN old;
                    ELSEIF(TG_OP = 'UPDATE') THEN
                        INSERT INTO gateway."UserProfileHistory"("UserProfileHistoryId", "Operation", "OperationDateTime",
                                    "UserProfileId", "AcceptedTermsOfService", "TermsOfServiceId", "Email", "ClosedDateTime", "IdentityManagementId", "EncryptionKey",
                                    "YearOfBirth", "LastLoginClientCode", "LastLoginDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "SMSNumber")
    		            VALUES(uuid_generate_v4(), TG_OP || '_LOGIN', now(),
                               old."UserProfileId", null, old."TermsOfServiceId", old."Email", old."ClosedDateTime", old."IdentityManagementId", old."EncryptionKey",
                               old."YearOfBirth", old."LastLoginClientCode", old."LastLoginDateTime", old."CreatedBy", old."CreatedDateTime", old."UpdatedBy", old."UpdatedDateTime", old."SMSNumber");
                        RETURN old;
                    END IF;
                END;$BODY$;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20221209222859_AddLastLoginClientCodeToUserProfile') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20221209222859_AddLastLoginClientCodeToUserProfile', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230104233040_OnDeleteSetNullUserFeedbackUserProfile') THEN
    ALTER TABLE gateway."UserFeedback" DROP CONSTRAINT "FK_UserFeedback_UserProfile_UserProfileId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230104233040_OnDeleteSetNullUserFeedbackUserProfile') THEN
    ALTER TABLE gateway."UserFeedback" ADD CONSTRAINT "FK_UserFeedback_UserProfile_UserProfileId" FOREIGN KEY ("UserProfileId") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230104233040_OnDeleteSetNullUserFeedbackUserProfile') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230104233040_OnDeleteSetNullUserFeedbackUserProfile', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230313212610_ResolveMigrationIssueForCommentEntryType') THEN
    UPDATE gateway."CommentEntryTypeCode" SET "Description" = 'Comment for a Clinical Document Entry'
    WHERE "CommentEntryCode" = 'CDO';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230313212610_ResolveMigrationIssueForCommentEntryType') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230313212610_ResolveMigrationIssueForCommentEntryType', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230314220815_CreateDependentDelegationTables') THEN
    CREATE TABLE gateway."Dependent" (
        "HdId" character varying(52) NOT NULL,
        "Protected" boolean NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Dependent" PRIMARY KEY ("HdId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230314220815_CreateDependentDelegationTables') THEN
    CREATE TABLE gateway."AllowedDelegation" (
        "DependentHdId" character varying(52) NOT NULL,
        "DelegateHdId" character varying(52) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AllowedDelegation" PRIMARY KEY ("DependentHdId", "DelegateHdId"),
        CONSTRAINT "FK_AllowedDelegation_Dependent_DependentHdId" FOREIGN KEY ("DependentHdId") REFERENCES gateway."Dependent" ("HdId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230314220815_CreateDependentDelegationTables') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230314220815_CreateDependentDelegationTables', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230323175128_AddExpiryDateToResourceDelegate') THEN
    ALTER TABLE gateway."ResourceDelegate" ADD "ExpiryDate" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230323175128_AddExpiryDateToResourceDelegate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230323175128_AddExpiryDateToResourceDelegate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230412022509_CreateDependentAuditTables') THEN
    CREATE TABLE gateway."DependentAuditOperationCode" (
        "Code" character varying(10) NOT NULL,
        "Description" character varying(50) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_DependentAuditOperationCode" PRIMARY KEY ("Code")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230412022509_CreateDependentAuditTables') THEN
    CREATE TABLE gateway."DependentAudit" (
        "DependentAuditId" uuid NOT NULL,
        "HdId" character varying(52) NOT NULL,
        "AgentUsername" character varying(255) NOT NULL,
        "ProtectedReason" character varying(500) NOT NULL,
        "OperationCode" character varying(10) NOT NULL,
        "TransactionDateTime" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_DependentAudit" PRIMARY KEY ("DependentAuditId"),
        CONSTRAINT "FK_DependentAudit_DependentAuditOperationCode_OperationCode" FOREIGN KEY ("OperationCode") REFERENCES gateway."DependentAuditOperationCode" ("Code") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230412022509_CreateDependentAuditTables') THEN
    INSERT INTO gateway."DependentAuditOperationCode" ("Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Protect', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Protect Dependent Operation Code', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    INSERT INTO gateway."DependentAuditOperationCode" ("Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Unprotect', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Unprotect Dependent Operation Code', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230412022509_CreateDependentAuditTables') THEN
    CREATE INDEX "IX_DependentAudit_OperationCode" ON gateway."DependentAudit" ("OperationCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230412022509_CreateDependentAuditTables') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230412022509_CreateDependentAuditTables', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230421214103_TourChangeInitial') THEN
    INSERT INTO gateway."ApplicationSetting" ("ApplicationSettingsId", "Application", "Component", "CreatedBy", "CreatedDateTime", "Key", "UpdatedBy", "UpdatedDateTime", "Value")
    VALUES ('bfcb45f6-27f9-4c0c-b494-80b147bcba8e', 'WEB', 'Tour', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'latestChangeDateTime', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', '2023-05-03T22:00:00.0000000Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230421214103_TourChangeInitial') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230421214103_TourChangeInitial', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230428053159_ReworkMessageVerfications') THEN
    ALTER TABLE gateway."MessagingVerification" DROP CONSTRAINT "FK_MessagingVerification_Email_EmailId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230428053159_ReworkMessageVerfications') THEN
    ALTER TABLE gateway."MessagingVerification" ADD "EmailAddress" character varying(254);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230428053159_ReworkMessageVerfications') THEN
    ALTER TABLE gateway."Email" ALTER COLUMN "Subject" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230428053159_ReworkMessageVerfications') THEN
    ALTER TABLE gateway."Email" ALTER COLUMN "Body" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230428053159_ReworkMessageVerfications') THEN
    ALTER TABLE gateway."Email" ADD "NotificationId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230428053159_ReworkMessageVerfications') THEN
    ALTER TABLE gateway."Email" ADD "Personalization" jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230428053159_ReworkMessageVerfications') THEN
    ALTER TABLE gateway."Email" ADD "Template" character varying(60);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230428053159_ReworkMessageVerfications') THEN
    ALTER TABLE gateway."MessagingVerification" ADD CONSTRAINT "FK_MessagingVerification_Email_EmailId" FOREIGN KEY ("EmailId") REFERENCES gateway."Email" ("EmailId") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230428053159_ReworkMessageVerfications') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230428053159_ReworkMessageVerfications', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230501202904_RemoveRequiredInviteKey') THEN
    ALTER TABLE gateway."MessagingVerification" ALTER COLUMN "InviteKey" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230501202904_RemoveRequiredInviteKey') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230501202904_RemoveRequiredInviteKey', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230504184240_CreateMessagingOutbox') THEN
    CREATE TABLE gateway."Outbox" (
        "Id" uuid NOT NULL,
        "CreatedOn" timestamp with time zone NOT NULL,
        "Content" jsonb NOT NULL,
        "Metadata" jsonb NOT NULL,
        "Status" integer NOT NULL,
        CONSTRAINT "PK_Outbox" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230504184240_CreateMessagingOutbox') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230504184240_CreateMessagingOutbox', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN

                    CREATE TEMP TABLE TempDependentAudit AS
                    SELECT "DependentAuditId", "HdId", "AgentUsername", "ProtectedReason", "OperationCode"||'Dependent' AS "OperationCode",
                        'Dependent' AS "GroupCode", "TransactionDateTime", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime"
                    FROM gateway."DependentAudit";
                    
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    DROP TABLE gateway."DependentAudit";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    DROP TABLE gateway."DependentAuditOperationCode";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    CREATE TABLE gateway."AuditGroupCode" (
        "Code" character varying(50) NOT NULL,
        "Description" character varying(50) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AuditGroupCode" PRIMARY KEY ("Code")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    CREATE TABLE gateway."AuditOperationCode" (
        "Code" character varying(50) NOT NULL,
        "Description" character varying(50) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AuditOperationCode" PRIMARY KEY ("Code")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    CREATE TABLE gateway."BlockedAccess" (
        "Hdid" character varying(52) NOT NULL,
        "DataSources" jsonb NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_BlockedAccess" PRIMARY KEY ("Hdid")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    CREATE TABLE gateway."AgentAudit" (
        "AgentAuditId" uuid NOT NULL,
        "Hdid" character varying(52) NOT NULL,
        "AgentUsername" character varying(255) NOT NULL,
        "Reason" character varying(500) NOT NULL,
        "OperationCode" character varying(50) NOT NULL,
        "GroupCode" character varying(50) NOT NULL,
        "TransactionDateTime" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AgentAudit" PRIMARY KEY ("AgentAuditId"),
        CONSTRAINT "FK_AgentAudit_AuditGroupCode_GroupCode" FOREIGN KEY ("GroupCode") REFERENCES gateway."AuditGroupCode" ("Code") ON DELETE RESTRICT,
        CONSTRAINT "FK_AgentAudit_AuditOperationCode_OperationCode" FOREIGN KEY ("OperationCode") REFERENCES gateway."AuditOperationCode" ("Code") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    INSERT INTO gateway."AuditGroupCode" ("Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('BlockedAccess', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Audit Blocked Access Group Code', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    INSERT INTO gateway."AuditGroupCode" ("Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Dependent', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Audit Dependent Group Code', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    INSERT INTO gateway."AuditOperationCode" ("Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('ChangeDataSourceAccess', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Change Data Source Access Operation Code', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    INSERT INTO gateway."AuditOperationCode" ("Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('ProtectDependent', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Protect Dependent Operation Code', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    INSERT INTO gateway."AuditOperationCode" ("Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('UnprotectDependent', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Unprotect Dependent Operation Code', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    CREATE INDEX "IX_AgentAudit_GroupCode" ON gateway."AgentAudit" ("GroupCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    CREATE INDEX "IX_AgentAudit_OperationCode" ON gateway."AgentAudit" ("OperationCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN

                    INSERT INTO gateway."AgentAudit" ("AgentAuditId", "Hdid", "AgentUsername", "Reason",
                                                        "OperationCode", "GroupCode", "TransactionDateTime",
                                                        "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime")
                    SELECT * FROM TempDependentAudit;
                    
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    DROP TABLE TempDependentAudit;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230508204917_CreateBlockedAccessTable') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230508204917_CreateBlockedAccessTable', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230524201808_AddColumnsPharmaCareDrug') THEN
    ALTER TABLE gateway."PharmaCareDrug" ADD "PharmacyAssessmentTitle" character varying(250);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230524201808_AddColumnsPharmaCareDrug') THEN
    ALTER TABLE gateway."PharmaCareDrug" ADD "PrescriptionProvided" boolean;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230524201808_AddColumnsPharmaCareDrug') THEN
    ALTER TABLE gateway."PharmaCareDrug" ADD "RedirectedToHealthCareProvider" boolean;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230524201808_AddColumnsPharmaCareDrug') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230524201808_AddColumnsPharmaCareDrug', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230526034844_AddNewProgramTypeCode') THEN
    INSERT INTO gateway."ProgramTypeCode" ("ProgramCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('PHAR-ASSMT', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Pharmacy Assessment', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230526034844_AddNewProgramTypeCode') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230526034844_AddNewProgramTypeCode', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230530175319_UpdateExpiryDateToRequired') THEN
    UPDATE gateway."ResourceDelegate" SET "ExpiryDate" = DATE '-infinity' WHERE "ExpiryDate" IS NULL;
    ALTER TABLE gateway."ResourceDelegate" ALTER COLUMN "ExpiryDate" SET NOT NULL;
    ALTER TABLE gateway."ResourceDelegate" ALTER COLUMN "ExpiryDate" SET DEFAULT DATE '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230530175319_UpdateExpiryDateToRequired') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230530175319_UpdateExpiryDateToRequired', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230811223424_AddDiagnosticImagingCommentEntryTypeCode') THEN
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('DIA', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Comment for a Diagnostic Imaging Entry', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230811223424_AddDiagnosticImagingCommentEntryTypeCode') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230811223424_AddDiagnosticImagingCommentEntryTypeCode', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230824180327_AddCancerScreeningCommentEntryTypeCode') THEN
    INSERT INTO gateway."CommentEntryTypeCode" ("CommentEntryCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('CNS', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Comment for a Cancer Screening Entry', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20230824180327_AddCancerScreeningCommentEntryTypeCode') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230824180327_AddCancerScreeningCommentEntryTypeCode', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20231031220124_ProfileIndexes') THEN
    CREATE INDEX "IX_UserProfileHistory_LastLoginDateTime" ON gateway."UserProfileHistory" ("LastLoginDateTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20231031220124_ProfileIndexes') THEN
    CREATE INDEX "IX_UserProfile_LastLoginDateTime" ON gateway."UserProfile" ("LastLoginDateTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20231031220124_ProfileIndexes') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20231031220124_ProfileIndexes', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20231103223810_RemoveDateTruncate') THEN
    DROP FUNCTION IF EXISTS gateway.date_trunc(text, timestamp with time zone);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20231103223810_RemoveDateTruncate') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20231103223810_RemoveDateTruncate', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240115221144_AddIndicesToDrugTables') THEN
    CREATE INDEX "IX_PharmaCareDrug_DINPIN" ON gateway."PharmaCareDrug" ("DINPIN");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240115221144_AddIndicesToDrugTables') THEN
    CREATE INDEX "IX_DrugProduct_DrugIdentificationNumber" ON gateway."DrugProduct" ("DrugIdentificationNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240115221144_AddIndicesToDrugTables') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240115221144_AddIndicesToDrugTables', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240119003540_UseDateColumnTypeForDrugTableDates') THEN
    ALTER TABLE gateway."DrugProduct" ALTER COLUMN "LastUpdate" TYPE Date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240119003540_UseDateColumnTypeForDrugTableDates') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240119003540_UseDateColumnTypeForDrugTableDates', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240311193221_AddSalesforceUserLoginClientTypeCode') THEN
    UPDATE gateway."UserLoginClientTypeCode" SET "Description" = 'Code for a login from the HG mobile app'
    WHERE "UserLoginClientCode" = 'Mobile';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240311193221_AddSalesforceUserLoginClientTypeCode') THEN
    UPDATE gateway."UserLoginClientTypeCode" SET "Description" = 'Code for a login from the HG web app'
    WHERE "UserLoginClientCode" = 'Web';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240311193221_AddSalesforceUserLoginClientTypeCode') THEN
    INSERT INTO gateway."UserLoginClientTypeCode" ("UserLoginClientCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Salesforce', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Code for a login from the HG Salesforce app', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240311193221_AddSalesforceUserLoginClientTypeCode') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240311193221_AddSalesforceUserLoginClientTypeCode', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240311220606_AddClientCodeToUserFeedback') THEN
    ALTER TABLE gateway."UserFeedback" ADD "ClientCode" character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240311220606_AddClientCodeToUserFeedback') THEN
    CREATE INDEX "IX_UserFeedback_ClientCode" ON gateway."UserFeedback" ("ClientCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240311220606_AddClientCodeToUserFeedback') THEN
    ALTER TABLE gateway."UserFeedback" ADD CONSTRAINT "FK_UserFeedback_UserLoginClientTypeCode_ClientCode" FOREIGN KEY ("ClientCode") REFERENCES gateway."UserLoginClientTypeCode" ("UserLoginClientCode") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240311220606_AddClientCodeToUserFeedback') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240311220606_AddClientCodeToUserFeedback', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240313200110_CreateBetaFeatureAccessTable') THEN
    CREATE TABLE gateway."BetaFeatureCode" (
        "Code" character varying(50) NOT NULL,
        "Description" character varying(50) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_BetaFeatureCode" PRIMARY KEY ("Code")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240313200110_CreateBetaFeatureAccessTable') THEN
    CREATE TABLE gateway."BetaFeatureAccess" (
        "UserProfileId" character varying(52) NOT NULL,
        "BetaFeatureCode" character varying(50) NOT NULL,
        "CreatedBy" character varying(60) NOT NULL,
        "CreatedDateTime" timestamp with time zone NOT NULL,
        "UpdatedBy" character varying(60) NOT NULL,
        "UpdatedDateTime" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_BetaFeatureAccess" PRIMARY KEY ("UserProfileId", "BetaFeatureCode"),
        CONSTRAINT "FK_BetaFeatureAccess_BetaFeatureCode_BetaFeatureCode" FOREIGN KEY ("BetaFeatureCode") REFERENCES gateway."BetaFeatureCode" ("Code") ON DELETE RESTRICT,
        CONSTRAINT "FK_BetaFeatureAccess_UserProfile_UserProfileId" FOREIGN KEY ("UserProfileId") REFERENCES gateway."UserProfile" ("UserProfileId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240313200110_CreateBetaFeatureAccessTable') THEN
    INSERT INTO gateway."BetaFeatureCode" ("Code", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Salesforce', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z', 'Salesforce Beta Feature Code', 'System', TIMESTAMPTZ '2019-05-01T07:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240313200110_CreateBetaFeatureAccessTable') THEN
    CREATE INDEX "IX_BetaFeatureAccess_BetaFeatureCode" ON gateway."BetaFeatureAccess" ("BetaFeatureCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240313200110_CreateBetaFeatureAccessTable') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240313200110_CreateBetaFeatureAccessTable', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240503192135_UpdateDatabaseAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Email Validation</title>
        </head>
        <body style="margin: 0">
            <table
                style="
                    width: 100%;
                    border-spacing: 0px;
                    margin: 0;
                    color: #707070;
                    font-family: Helvetica, Arial, Verdana, Tahoma, sans-serif;
                    font-size: 12px;
                "
                aria-describedby="Layout Table"
            >
                <tr style="background: #003366">
                    <th scope="col" style="width:45px;"></th>
                    <th
                        scope="col"
                        style="text-align: left; width:350px;"
                    >
                        <div aria-label="Health Gateway Logo">
                            <img
                                src="${ActivationHost}/Logo.png"
                                alt="Health Gateway Logo"
                            />
                        </div>
                    </th>
                    <th scope="col"></th>
                </tr>
                <tr>
                    <td colspan="3" style="height:20px;"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <h1 style="font-size: 18px">Almost there!</h1>
                        <p>
                            We''ve received a request to register your email address
                            for a Health Gateway account.
                        </p>
                        <p>
                            To activate your account, please verify your email by
                            clicking the link:
                        </p>
                        <a
                            style="color: #1292c5; font-weight: 600"
                            href="${ActivationHost}/ValidateEmail/${InviteKey}"
                        >
                            Health Gateway Account Verification
                        </a>
                        <p>
                            This email verification link will expire in
                            ${ExpiryHours} hours.
                        </p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = '040c2ec3-d6c0-4199-9e4b-ebe6da48d52a';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240503192135_UpdateDatabaseAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Account Recovered</title>
        </head>
        <body style="margin: 0">
            <table
                style="
                    width: 100%;
                    border-spacing: 0px;
                    margin: 0;
                    color: #707070;
                    font-family: Helvetica, Arial, Verdana, Tahoma, sans-serif;
                    font-size: 12px;
                "
                aria-describedby="Layout Table"
            >
                <tr style="background: #036">
                    <th scope="col" style="width:45px;"></th>
                    <th
                        scope="col"
                        style="text-align: left; width:350px;"
                    >
                        <div aria-label="Health Gateway Logo">
                            <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                        </div>
                    </th>
                    <th scope="col"></th>
                </tr>
                <tr>
                    <td colspan="3" style="height:20px;"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <h1 style="font-size: 18px">Hi,</h1>
                        <p>
                            You have successfully recovered your Health Gateway
                            account. You may continue to use the service as you did
                            before.
                        </p>
                        <p>Thanks,</p>
                        <p>Health Gateway Team</p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = '2fe8c825-d4de-4884-be6a-01a97b466425';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240503192135_UpdateDatabaseAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Account Closed</title>
        </head>
        <body style="margin: 0">
            <table
                style="
                    width: 100%;
                    border-spacing: 0px;
                    margin: 0;
                    color: #707070;
                    font-family: Helvetica, Arial, Verdana, Tahoma, sans-serif;
                    font-size: 12px;
                "
                aria-describedby="Layout Table"
            >
                <tr style="background: #036">
                    <th scope="col" style="width:45px;"></th>
                    <th
                        scope="col"
                        style="text-align: left; width:350px;"
                    >
                        <div aria-label="Health Gateway Logo">
                            <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                        </div>
                    </th>
                    <th scope="col"></th>
                </tr>
                <tr>
                    <td colspan="3" style="height:20px;"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <h1 style="font-size: 18px">Hi,</h1>
                        <p>
                            You have closed your Health Gateway account. If you
                            would like to recover your account, please login to
                            Health Gateway within the next 30 days and click
                            “Recover Account”. No further action is required if you
                            want your account and personally entered information to
                            be removed from the Health Gateway after this time
                            period.
                        </p>
                        <p>Thanks,</p>
                        <p>Health Gateway Team</p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = '79503a38-c14a-4992-b2fe-5586629f552e';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240503192135_UpdateDatabaseAssets') THEN
    UPDATE gateway."EmailTemplate" SET "Body" = '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Account Removed</title>
        </head>
        <body style="margin: 0">
            <table
                style="
                    width: 100%;
                    border-spacing: 0px;
                    margin: 0;
                    color: #707070;
                    font-family: Helvetica, Arial, Verdana, Tahoma, sans-serif;
                    font-size: 12px;
                "
                aria-describedby="Layout Table"
            >
                <tr style="background: #036">
                    <th scope="col" style="width:45px;"></th>
                    <th
                        scope="col"
                        style="text-align: left; width:350px;"
                    >
                        <div aria-label="Health Gateway Logo">
                            <img src="${host}/Logo.png" alt="Health Gateway Logo" />
                        </div>
                    </th>
                    <th scope="col"></th>
                </tr>
                <tr>
                    <td colspan="3" style="height:20px;"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <h1 style="font-size: 18px">Hi,</h1>
                        <p>
                            Your Health Gateway account closure has been completed.
                            Your account and personally entered information have
                            been removed from the application. You are welcome to
                            register again for the Health Gateway in the future.
                        </p>
                        <p>Thanks,</p>
                        <p>Health Gateway Team</p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>
    '
    WHERE "EmailTemplateId" = 'd9898318-4e53-4074-9979-5d24bd370055';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240503192135_UpdateDatabaseAssets') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240503192135_UpdateDatabaseAssets', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240521174320_AddMobileUserLoginClientCodes') THEN
    INSERT INTO gateway."UserLoginClientTypeCode" ("UserLoginClientCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('Android', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Code for a login from the HG Android mobile app', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    INSERT INTO gateway."UserLoginClientTypeCode" ("UserLoginClientCode", "CreatedBy", "CreatedDateTime", "Description", "UpdatedBy", "UpdatedDateTime")
    VALUES ('iOS', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', 'Code for a login from the HG iOS mobile app', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240521174320_AddMobileUserLoginClientCodes') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240521174320_AddMobileUserLoginClientCodes', '8.0.6');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240529224631_AddEmailTemplateForAddDependentMismatch') THEN
    INSERT INTO gateway."EmailTemplate" ("EmailTemplateId", "Body", "CreatedBy", "CreatedDateTime", "EffectiveDate", "ExpiryDate", "FormatCode", "From", "Name", "Priority", "Subject", "UpdatedBy", "UpdatedDateTime")
    VALUES ('491dabc6-f799-427c-ace4-b49ece2d612c', '<!DOCTYPE html>
    <html lang="en">
        <head>
            <title>Health Gateway Debug Info: Add Dependent Mismatch</title>
            <style>
                td:not(:first-child),
                th:not(:first-child) {
                    padding: 0.5em;
                }
                pre {
                    margin: 0;
                }
            </style>
        </head>
        <body style="margin: 0">
            <strong>Hi Health Gateway Team,</strong>
            <p>
                Find debug information below relating to a failed request to add a
                dependent due to mismatched data.
            </p>
            <table>
                <tbody>
                    <tr>
                        <td><strong>Delegate HDID</strong></td>
                        <td>
                            <pre>${delegateHdid}</pre>
                        </td>
                    </tr>
                </tbody>
            </table>
            <table>
                <thead>
                    <tr>
                        <th></th>
                        <th>Request</th>
                        <th>Response</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><strong>PHN</strong></td>
                        <td><pre>${requestPhn}</pre></td>
                        <td><pre>${responsePhn}</pre></td>
                    </tr>
                    <tr>
                        <td><strong>Given Name(s)</strong></td>
                        <td><pre>${requestFirstName}</pre></td>
                        <td><pre>${responseFirstName}</pre></td>
                    </tr>
                    <tr>
                        <td><strong>Last Name</strong></td>
                        <td><pre>${requestLastName}</pre></td>
                        <td><pre>${responseLastName}</pre></td>
                    </tr>
                    <tr>
                        <td><strong>Birthdate</strong></td>
                        <td><pre>${requestBirthdate}</pre></td>
                        <td><pre>${responseBirthdate}</pre></td>
                    </tr>
                </tbody>
            </table>
        </body>
    </html>
    ', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z', TIMESTAMPTZ '2019-05-01T00:00:00Z', NULL, 'HTML', 'HG_Donotreply@gov.bc.ca', 'AdminAddDependentMismatch', 1, 'Health Gateway Debug Info: Add Dependent Mismatch', 'System', TIMESTAMPTZ '2019-05-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM gateway."__EFMigrationsHistory" WHERE "MigrationId" = '20240529224631_AddEmailTemplateForAddDependentMismatch') THEN
    INSERT INTO gateway."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240529224631_AddEmailTemplateForAddDependentMismatch', '8.0.6');
    END IF;
END $EF$;
COMMIT;

