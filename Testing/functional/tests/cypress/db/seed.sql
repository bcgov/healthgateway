/* Data Cleanup */
TRUNCATE gateway."AuditEvent";
TRUNCATE gateway."UserProfile" CASCADE;
TRUNCATE gateway."UserProfileHistory" CASCADE;
TRUNCATE gateway."Communication" CASCADE;
TRUNCATE gateway."ResourceDelegate" CASCADE;
TRUNCATE gateway."ResourceDelegateHistory" CASCADE;
TRUNCATE gateway."UserPreference" CASCADE;
TRUNCATE gateway."Comment" CASCADE;
TRUNCATE gateway."MessagingVerification" CASCADE;
TRUNCATE gateway."Note" CASCADE;
TRUNCATE gateway."Rating" CASCADE;
TRUNCATE gateway."Email" CASCADE;
TRUNCATE gateway."AdminTag" CASCADE;
TRUNCATE gateway."UserFeedback" CASCADE;
TRUNCATE gateway."Dependent" CASCADE;
TRUNCATE gateway."AgentAudit" CASCADE;
TRUNCATE gateway."BlockedAccess" CASCADE;
TRUNCATE gateway."BetaFeatureAccess" CASCADE;
TRUNCATE gateway."Outbox" CASCADE;


/* Registered HealthGateway User - Keycloak User (healthgateway) */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"TermsOfServiceId", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',	
	'System', 
	current_timestamp - INTERVAL '2 day',  
	'System', 
	current_timestamp,  
	'2fab66e7-37c9-4b03-ba25-e8fad604dc7f', 
	'nobody@healthgateway.gov.bc.ca',
	null,
	null,
	current_timestamp, 
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
	'1967',
	'Salesforce'
);

/* Protected User - Keycloak User (protected) */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"TermsOfServiceId", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	'RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWQ',	
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'2fab66e7-37c9-4b03-ba25-e8fad604dc7f', 
	'somebody@healthgateway.gov.bc.ca',
	null,
	null,
	current_timestamp, 
	'iHa5atSWqppGzWsR1Z8nbL9OHJamPHLMwYqdKmsf4jU=',
	null,
	'1995',
	'iOS'
);

/* Invaliddoses - Keycloak User (hthgtwy20) */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"TermsOfServiceId", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',	
	'System', 
	current_timestamp - INTERVAL '120 day', 
	'System', 
	current_timestamp,
	'2fab66e7-37c9-4b03-ba25-e8fad604dc7f', 
	'nobody@healthgateway.gov.bc.ca',
	null,
	null,
	current_timestamp, 
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
	null,
	'Web'
);

/* Labratory Queued - Keycloak User (hthgtwy09) */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"TermsOfServiceId", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	'3ZQCSNNC6KVP2GYLA4O3EFZXGUAPWBQHU6ZEB7FXNZJ2WYCLPH3A',	
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'2fab66e7-37c9-4b03-ba25-e8fad604dc7f', 
	null,
	null,
	null,
	current_timestamp, 
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
	'1967',
	null
);

/* Notfound - Keycloak User - Keycloak User (hthgtwy03) */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"TermsOfServiceId", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	'R43YCT4ZY37EIJLW2O5LV2I77BZA3K3M25EUJGWAVGVJ7JKBDKCQ',
	'System', 
	current_timestamp - INTERVAL '2 day',  
	'System', 
	current_timestamp, 
	'2fab66e7-37c9-4b03-ba25-e8fad604dc7f', 
	null,
	null,
	null,
	current_timestamp, 
	'KtBm7JYegayKpx5fjwM2RUGZf79JOnNC21NhUrIAzmg=',
	null,
	'1988',
	'Mobile'
);

/* User without the latest accepted terms of service - Keycloak User - Keycloak User (hthgtwy04) */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"TermsOfServiceId", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	'K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A',	
	'System', 
	current_timestamp - INTERVAL '3 day',  
	'System', 
	current_timestamp,  
	'c99fd839-b4a2-40f9-b103-529efccd0dcd', 
	null,
	null,
	null,
	current_timestamp,  
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
	'2001',
	'Android'
);

/* PHN: 9735352535 used for admin covid assessment and blocked dataset tests */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"TermsOfServiceId", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	'GO4DOSMRJ7MFKPPADDZ3FK2MOJ45SFKONJWR67XNLMZQFNEHDKDA',	
	'System', 
	current_timestamp - INTERVAL '1000 day',  
	'System', 
	current_timestamp - INTERVAL '1000 day',   
	'c99fd839-b4a2-40f9-b103-529efccd0dcd', 
	null,
	null,
	null,
	current_timestamp - INTERVAL '1000 day',  
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
	null,
	'Web'
);

/* PHN:  9874359198 used for Warning: The person returned is the subject of an potential duplicate */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"TermsOfServiceId", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	'NW55426',	
	'System', 
	current_timestamp - INTERVAL '1000 day',  
	'System', 
	current_timestamp - INTERVAL '1000 day',   
	'c99fd839-b4a2-40f9-b103-529efccd0dcd', 
	null,
	null,
	null,
	current_timestamp - INTERVAL '1000 day',  
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
	null,
	'Web'
);


/* User (hthgtwy04) - last logged in 1 day ago */
INSERT INTO gateway."UserProfileHistory"(
	"UserProfileHistoryId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"AcceptedTermsOfService",
    "Email",
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime",
    "Operation",
    "OperationDateTime",
	"EncryptionKey", 
	"SMSNumber",
    "TermsOfServiceId",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	uuid_generate_v4(),	
	'System', 
	current_timestamp - INTERVAL '3 day',  
	'System', 
	current_timestamp - INTERVAL '1 day', 
	'K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A', 
	null,
    null,
	null,
	null,
	current_timestamp - INTERVAL '1 day',
    'UPDATE_LOGIN',
    current_timestamp,
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
    'eafeee76-8a64-49ee-81ba-ddfe2c01deb8',
	'2001',
	'Web'
);

/* User (hthgtwy04) - last logged in 2 days ago */
INSERT INTO gateway."UserProfileHistory"(
	"UserProfileHistoryId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"AcceptedTermsOfService",
    "Email",
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
    "Operation",
    "OperationDateTime",
	"EncryptionKey", 
	"SMSNumber",
    "TermsOfServiceId",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	uuid_generate_v4(),		
	'System', 
	current_timestamp - INTERVAL '3 day',  
	'System', 
	current_timestamp - INTERVAL '2 day',  
	'K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A', 
	null,
    null,
	null,
	null,
	current_timestamp - INTERVAL '2 day', 
    'UPDATE_LOGIN',
    current_timestamp,
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
    'eafeee76-8a64-49ee-81ba-ddfe2c01deb8',
	'2001',
	'Web'
);

/* User (hthgtwy04) - last logged in 3 days ago */
INSERT INTO gateway."UserProfileHistory"(
	"UserProfileHistoryId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"AcceptedTermsOfService",
    "Email",
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
    "Operation",
    "OperationDateTime",
	"EncryptionKey", 
	"SMSNumber",
    "TermsOfServiceId",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	uuid_generate_v4(),	
	'System', 
	current_timestamp - INTERVAL '3 day',  
	'System', 
	current_timestamp - INTERVAL '3 day',  
	'K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A', 
	null,
    null,
	null,
	null,
	current_timestamp - INTERVAL '3 day',  
    'UPDATE_LOGIN',
    current_timestamp,
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
    'eafeee76-8a64-49ee-81ba-ddfe2c01deb8',
	'2001',
	'Mobile'
);

/* User keycloak (hthgtwy11) - last logged in 1 day ago  */
INSERT INTO gateway."UserProfileHistory"(
	"UserProfileHistoryId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"AcceptedTermsOfService",
    "Email",
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime",
    "Operation",
    "OperationDateTime",
	"EncryptionKey", 
	"SMSNumber",
    "TermsOfServiceId",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	uuid_generate_v4(),	
	'System', 
	current_timestamp - INTERVAL '2 day', 
	'System', 
	current_timestamp - INTERVAL '1 day',  
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A', 
	null,
    null,
	null,
	null,
	current_timestamp - INTERVAL '1 day',
    'UPDATE_LOGIN',
    current_timestamp,
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
    'eafeee76-8a64-49ee-81ba-ddfe2c01deb8',
	'1967',
	'Web'
);

/* User keycloak (hthgtwy11) - last logged in 2 days ago  */
INSERT INTO gateway."UserProfileHistory"(
	"UserProfileHistoryId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"AcceptedTermsOfService",
    "Email",
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
    "Operation",
    "OperationDateTime",
	"EncryptionKey", 
	"SMSNumber",
    "TermsOfServiceId",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	uuid_generate_v4(),		
	'System', 
	current_timestamp - INTERVAL '2 day',  
	'System', 
	current_timestamp - INTERVAL '2 day',  
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A', 
	null,
    null,
	null,
	null,
	current_timestamp - INTERVAL '2 day', 
    'UPDATE_LOGIN',
    current_timestamp,
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
    'eafeee76-8a64-49ee-81ba-ddfe2c01deb8',
	'1967',
	'Web'
);

/* Notfound - Keycloak User - Last logged in 2 days ago */
INSERT INTO gateway."UserProfileHistory"(
	"UserProfileHistoryId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"AcceptedTermsOfService",
    "Email",
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
    "Operation",
    "OperationDateTime",
	"EncryptionKey", 
	"SMSNumber",
    "TermsOfServiceId",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	uuid_generate_v4(),	
	'System', 
	current_timestamp - INTERVAL '2 day',  
	'System', 
	current_timestamp - INTERVAL '2 day',  
	'R43YCT4ZY37EIJLW2O5LV2I77BZA3K3M25EUJGWAVGVJ7JKBDKCQ', 
	null,
    null,
	null,
	null,
	current_timestamp - INTERVAL '2 day',  
    'UPDATE_LOGIN',
    current_timestamp,
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
    'eafeee76-8a64-49ee-81ba-ddfe2c01deb8',
	'1988',
	'Mobile'
);

/* Invaliddoses - Keycloak User - Last logged in 120 days ago */
INSERT INTO gateway."UserProfileHistory"(
	"UserProfileHistoryId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"AcceptedTermsOfService",
    "Email",
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
    "Operation",
    "OperationDateTime",
	"EncryptionKey", 
	"SMSNumber",
    "TermsOfServiceId",
	"YearOfBirth",
	"LastLoginClientCode")
VALUES (
	uuid_generate_v4(),	
	'System', 
	current_timestamp - INTERVAL '120 day',  
	'System', 
	current_timestamp - INTERVAL '120 day',  
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA', 
	null,
    null,
	null,
	null,
	current_timestamp - INTERVAL '120 day',  
    'UPDATE_LOGIN',
    current_timestamp,
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null,
    'eafeee76-8a64-49ee-81ba-ddfe2c01deb8',
	null,
	'Web'
);

INSERT INTO gateway."UserProfileHistory"(
    "UserProfileHistoryId",
    "CreatedBy",
    "CreatedDateTime",
    "UpdatedBy",
    "UpdatedDateTime",
    "UserProfileId",
    "AcceptedTermsOfService",
    "Email",
    "ClosedDateTime",
    "IdentityManagementId",
    "LastLoginDateTime",
    "Operation",
    "OperationDateTime",
    "EncryptionKey",
    "SMSNumber",
    "TermsOfServiceId",
    "YearOfBirth",
    "LastLoginClientCode")
VALUES (
    'd139c62d-c899-43fb-84f2-45c654ca3270',
    'System',
    '2023-09-29 19:11:01.637456 +00:00',
    'System',
    '2023-10-04 19:40:02.282833 +00:00',
    'UF3PZJUD45R7GJFWEVZ6USVANSIRGXOZK7JZI2XBYTG5P4CBAUAA',
    null,
    null,
    '2023-10-31 19:40:02.282237 +00:00',
    'b5a687c7-2915-4d3f-bb14-a3cf0504a2b2',
    '2023-10-31 19:39:28.000000 +00:00',
    'DELETE',
    '2023-12-01 19:41:29.067450 +00:00',
    'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
    null,
    '2fab66e7-37c9-4b03-ba25-e8fad604dc7f',
    '1994',
    'Web'
);

INSERT INTO gateway."BetaFeatureAccess" (
    "UserProfileId",
    "BetaFeatureCode",
    "CreatedBy",
    "CreatedDateTime",
    "UpdatedBy",
    "UpdatedDateTime")
VALUES (
    'RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWQ',
    'Salesforce',
    'System',
    current_timestamp - INTERVAL '1 day',
    'System',
    current_timestamp - INTERVAL '1 day');

INSERT INTO gateway."Rating"(
	"RatingId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"RatingValue", 
	"Skip")
VALUES (
	uuid_generate_v4(),		
	'System', 
	current_timestamp - INTERVAL '3 day',  
	'System', 
	current_timestamp - INTERVAL '2 day',  
	5,
    FALSE
);

INSERT INTO gateway."Rating"(
	"RatingId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"RatingValue", 
	"Skip")
VALUES (
	uuid_generate_v4(),		
	'System', 
	current_timestamp - INTERVAL '3 day',  
	'System', 
	current_timestamp - INTERVAL '2 day',  
	3,
    FALSE
);

/* Communication Banner */
INSERT INTO gateway."Communication"(
	"CommunicationId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"Text", 
	"Subject", 
	"EffectiveDateTime", 
	"ExpiryDateTime", 
	"CommunicationStatusCode", 
	"CommunicationTypeCode", 
	"Priority", 
	"ScheduledDateTime"
) VALUES (
	'f3ce0b06-9372-495f-baff-784c38b09480',	
	'System',
	current_timestamp,	
	'System',
	current_timestamp,	
	'<p>Test Banner - <a href="healthgateway@gov.bc.ca" rel="noopener noreferrer nofollow">healthgateway@gov.bc.ca</a></p>',
	'Test Banner',
	current_timestamp,
	current_timestamp + INTERVAL '1 day',
	'New',
	'Banner',
	10,
	null
);

/* In-App Communication Banner */
INSERT INTO gateway."Communication"(
	"CommunicationId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"Text", 
	"Subject", 
	"EffectiveDateTime", 
	"ExpiryDateTime", 
	"CommunicationStatusCode", 
	"CommunicationTypeCode", 
	"Priority", 
	"ScheduledDateTime"
) VALUES (
	'f3ce0b06-9372-495f-baff-784c38b09481',	
	'System',
	current_timestamp,	
	'System',
	current_timestamp,	
	'<p>In-App Banner - <a href="healthgateway@gov.bc.ca" rel="noopener noreferrer nofollow">healthgateway@gov.bc.ca</a></p>',
	'In-App Banner',
	current_timestamp,
	current_timestamp + INTERVAL '1 day',
	'New',
	'InApp',
	10,
	null
);

/* Mobile Communication */
INSERT INTO gateway."Communication"(
	"CommunicationId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"Text", 
	"Subject", 
	"EffectiveDateTime", 
	"ExpiryDateTime", 
	"CommunicationStatusCode", 
	"CommunicationTypeCode", 
	"Priority", 
	"ScheduledDateTime"
) VALUES (
	'70e79659-70c7-4021-85eb-dc6b46a874de',	
	'System',
	current_timestamp,	
	'System',
	current_timestamp,	
	'<p>Mobile Communication - <a href="healthgateway@gov.bc.ca" rel="noopener noreferrer nofollow">healthgateway@gov.bc.ca</a></p>',
	'Seeded Mobile Comm',
	current_timestamp,
	current_timestamp + INTERVAL '1 day',
	'New',
	'Mobile',
	10,
	null
);

/* User Preferences */
INSERT INTO gateway."UserPreference"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"Preference", 
	"Value")
VALUES (
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',	
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'quickLinks', 
	'[{"name":"Medications","filter":{"modules":["Medication"]}},{"name":"My Notes","filter":{"modules":["Note"]}}]'
);

/* Add Resource Delegates */
INSERT INTO gateway."ResourceDelegate"(
	"ResourceOwnerHdid", 
	"ProfileHdid", 
	"ReasonCode", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"ReasonObjectType", 
	"ReasonObject")
VALUES (
	'232434345442257', 
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A', 
	'Guardian', 
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'System.DateTime, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e', 
	'"2021-01-20T00:00:00"'
);
INSERT INTO gateway."ResourceDelegate"(
	"ResourceOwnerHdid", 
	"ProfileHdid", 
	"ReasonCode", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"ReasonObjectType", 
	"ReasonObject")
VALUES (
	'162346565465464564565463257', 
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A', 
	'Guardian', 
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'System.DateTime, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e', 
	'"2021-01-20T00:00:00"'
);
/* Dependent with special authority - phn: 9746208843 */
INSERT INTO gateway."ResourceDelegate"(
	"ResourceOwnerHdid", 
	"ProfileHdid", 
	"ReasonCode", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"ReasonObjectType", 
	"ReasonObject")
VALUES (
	'IASGH65211V6WHXKGQDSEJAHYMYR', 
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A', 
	'Guardian', 
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'System.DateTime, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e', 
	'"2021-01-20T00:00:00"'
);
/* Dependent PHN:9874307208 (BNV554213556), Guardian PHN: 9735353315 (P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A) */
INSERT INTO gateway."ResourceDelegate"(
	"ResourceOwnerHdid", 
	"ProfileHdid", 
	"ReasonCode", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"ReasonObjectType", 
	"ReasonObject")
VALUES (
	'BNV554213556', 
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A', 
	'Guardian', 
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'System.DateTime, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e', 
	'"2023-03-27T00:00:00"'
);
/* Dependent PHN:9872868095 (727302800477298080), Guardian PHN: 9735353315 (P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A) */
INSERT INTO gateway."ResourceDelegate"(
	"ResourceOwnerHdid", 
	"ProfileHdid", 
	"ReasonCode", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"ReasonObjectType", 
	"ReasonObject")
VALUES (
	'727302800477298080', 
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A', 
	'Guardian', 
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'System.DateTime, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e', 
	'"2021-01-20T00:00:00"'
);
/* Dependent PHN:9872868095 (727302800477298080), Guardian PHN: 9735352495 (3ZQCSNNC6KVP2GYLA4O3EFZXGUAPWBQHU6ZEB7FXNZJ2WYCLPH3A) */
INSERT INTO gateway."ResourceDelegate"(
	"ResourceOwnerHdid", 
	"ProfileHdid", 
	"ReasonCode", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"ReasonObjectType", 
	"ReasonObject")
VALUES (
	'727302800477298080', 
	'3ZQCSNNC6KVP2GYLA4O3EFZXGUAPWBQHU6ZEB7FXNZJ2WYCLPH3A', 
	'Guardian', 
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'System.DateTime, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e', 
	'"2021-01-20T00:00:00"'
);

INSERT INTO gateway."Email"(
    "EmailId", 
    "CreatedBy", 
    "CreatedDateTime", 
    "UpdatedBy", 
    "UpdatedDateTime", 
    "From", 
    "To", 
    "Subject", 
    "Body", 
    "FormatCode", 
    "Priority", 
    "SentDateTime", 
    "LastRetryDateTime", 
    "Attempts", 
    "SmtpStatusCode", 
    "EmailStatusCode")
VALUES (
    'a86b1a95-42c1-49e4-9d48-6080cf2a223d', 
    'System',
    current_timestamp - INTERVAL '31 day',
    'System',
    now(),
    'HG_Donotreply@gov.bc.ca', 
    'fakeemail@healthgateway.gov.bc.ca', 
    'Health Gateway Email Verification hgdev', 
    '<!DOCTYPE html>
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
                                src="https://dev-legacy.healthgateway.gov.bc.ca/Logo.png"
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
                            href="https://dev-legacy.healthgateway.gov.bc.ca/ValidateEmail/5f9ceabb-a6e2-4eb3-839c-b364f65c502d"
                        >
                            Health Gateway Account Verification
                        </a>
                        <p>
                            This email verification link will expire in
                            12 hours.
                        </p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>', 
    'HTML', 
    10, 
    '2022-07-05 00:47:05.828812+00', 
    null, 
    1,
    250, 
    'Processed'
);

INSERT INTO gateway."Email"(
    "EmailId", 
    "CreatedBy", 
    "CreatedDateTime", 
    "UpdatedBy", 
    "UpdatedDateTime", 
    "From", 
    "To", 
    "Subject", 
    "Body", 
    "FormatCode", 
    "Priority", 
    "SentDateTime", 
    "LastRetryDateTime", 
    "Attempts", 
    "SmtpStatusCode", 
    "EmailStatusCode")
VALUES (
    '8986a8b9-02e1-4756-a660-3e1ed4fa81ce', 
    'System',
    current_timestamp - INTERVAL '31 day',
    'System',
    now(),
    'HG_Donotreply@gov.bc.ca', 
    'nobody@healthgateway.gov.bc.ca', 
    'Health Gateway Email Verification hgdev', 
    '<!DOCTYPE html>
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
                                src="https://dev-legacy.healthgateway.gov.bc.ca/Logo.png"
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
                            href="https://dev-legacy.healthgateway.gov.bc.ca/ValidateEmail/5f9ceabb-a6e2-4eb3-839c-b364f65c502d"
                        >
                            Health Gateway Account Verification
                        </a>
                        <p>
                            This email verification link will expire in
                            12 hours.
                        </p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>', 
    'HTML', 
    10, 
    '2022-07-05 00:47:05.828812+00', 
    null, 
    1,
    250, 
    'Processed'
);

INSERT INTO gateway."Email"(
    "EmailId", 
    "CreatedBy", 
    "CreatedDateTime", 
    "UpdatedBy", 
    "UpdatedDateTime", 
    "From", 
    "To", 
    "Subject", 
    "Body", 
    "FormatCode", 
    "Priority", 
    "SentDateTime", 
    "LastRetryDateTime", 
    "Attempts", 
    "SmtpStatusCode", 
    "EmailStatusCode")
VALUES (
    '0e2b5d66-dfac-4396-82c1-aec3be4bb6f6', 
    'System',
    current_timestamp - INTERVAL '31 day',
    'System',
    now(),
    'HG_Donotreply@gov.bc.ca', 
    'nobody@healthgateway.gov.bc.ca', 
    'Health Gateway Email Verification hgdev', 
    '<!DOCTYPE html>
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
                                src="https://dev-legacy.healthgateway.gov.bc.ca/Logo.png"
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
                            href="https://dev-legacy.healthgateway.gov.bc.ca/ValidateEmail/5f9ceabb-a6e2-4eb3-839c-b364f65c502d"
                        >
                            Health Gateway Account Verification
                        </a>
                        <p>
                            This email verification link will expire in
                            12 hours.
                        </p>
                    </td>
                    <td></td>
                </tr>
            </table>
        </body>
    </html>', 
    'HTML', 
    10, 
    '2022-07-05 00:47:05.828812+00', 
    null, 
    1,
    250, 
    'Processed'
);

/* Registered HealthGateway User - Keycloak User (healthgateway) */
INSERT INTO gateway."MessagingVerification"(
	"MessagingVerificationId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"HdId", 
	"Validated", 
	"EmailId", 
	"InviteKey", 
	"ExpireDate", 
	"SMSNumber", 
	"SMSValidationCode", 
	"VerificationType", 
	"Deleted", 
	"VerificationAttempts")
VALUES (
	uuid_generate_v4(),
	'System',
	now(),
	'System',
	now(),
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	true,
	null,
	'00000000-0000-0000-0000-000000000000',
	now()+INTERVAL '1 day',
	'2506715000',
	'654321',
	'SMS',
	false,
	0
);

/* Registered HealthGateway User - Keycloak User (healthgateway) */
INSERT INTO gateway."MessagingVerification"(
	"MessagingVerificationId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"HdId", 
	"Validated", 
	"EmailId", 
	"InviteKey", 
	"ExpireDate", 
	"SMSNumber", 
	"SMSValidationCode", 
	"VerificationType", 
	"Deleted", 
	"VerificationAttempts")
VALUES (
	uuid_generate_v4(),
	'System',
	now()- INTERVAL '1 hour',
	'System',
	now()- INTERVAL '1 hour',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	false,
	null,
	'00000000-0000-0000-0000-000000000000',
	now()+INTERVAL '1 day',
	'2506715000',
	'123456',
	'SMS',
	false,
	0
);

/* Registered HealthGateway User - Keycloak User (healthgateway) */
INSERT INTO gateway."MessagingVerification"(
	"MessagingVerificationId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"HdId", 
	"Validated", 
	"EmailId", 
	"InviteKey", 
	"ExpireDate", 
	"SMSNumber", 
	"SMSValidationCode", 
	"VerificationType", 
	"Deleted", 
	"VerificationAttempts",
	"EmailAddress")
VALUES (
	uuid_generate_v4(),
	'System',
	now()- INTERVAL '1 hour',
	'System',
	now()- INTERVAL '1 hour',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	true,
	'0e2b5d66-dfac-4396-82c1-aec3be4bb6f6',
	'00000000-0000-0000-0000-000000000000',
	now()+INTERVAL '1 day',
	null,
	null,
	'Email',
	false,
	0,
	'nobody@healthgateway.gov.bc.ca'
);

/* Registered HealthGateway User - Keycloak User (healthgateway) */
INSERT INTO gateway."MessagingVerification"(
	"MessagingVerificationId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"HdId", 
	"Validated", 
	"EmailId", 
	"InviteKey", 
	"ExpireDate", 
	"SMSNumber", 
	"SMSValidationCode", 
	"VerificationType", 
	"Deleted", 
	"VerificationAttempts")
VALUES (
	uuid_generate_v4(),
	'System',
	now(),
	'System',
	now(),
	'RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWQ',
	false,
	null,
	'00000000-0000-0000-0000-000000000000',
	now()+INTERVAL '1 day',
	'2506715000',
	'567890',
	'SMS',
	false,
	0
);

/* Keycloak User (hthgtwy03) */
INSERT INTO gateway."MessagingVerification"(
	"MessagingVerificationId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"HdId", 
	"Validated", 
	"EmailId", 
	"InviteKey", 
	"ExpireDate", 
	"SMSNumber", 
	"SMSValidationCode", 
	"VerificationType", 
	"Deleted", 
	"VerificationAttempts",
    "EmailAddress")
VALUES (
	uuid_generate_v4(),
	'System',
	now(),
	'System',
	now(),
	'R43YCT4ZY37EIJLW2O5LV2I77BZA3K3M25EUJGWAVGVJ7JKBDKCQ',
	false,
	'a86b1a95-42c1-49e4-9d48-6080cf2a223d',
	'00000000-0000-0000-0000-000000000000',
	now()+INTERVAL '1 day',
	null,
	'123456',
	'Email',
	false,
	0,
	'fakeemail@healthgateway.gov.bc.ca'
);

/* Keycloak User (hthgtwy20) */
INSERT INTO gateway."MessagingVerification"(
	"MessagingVerificationId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"HdId", 
	"Validated", 
	"EmailId", 
	"InviteKey", 
	"ExpireDate", 
	"SMSNumber", 
	"SMSValidationCode", 
	"VerificationType", 
	"Deleted", 
	"VerificationAttempts",
    "EmailAddress")
VALUES (
	uuid_generate_v4(),
	'System',
	now()- INTERVAL '1 hour',
	'System',
	now()- INTERVAL '1 hour',
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	true,
	'8986a8b9-02e1-4756-a660-3e1ed4fa81ce',
	'00000000-0000-0000-0000-000000000000',
	now()+INTERVAL '1 day',
	null,
	'123456',
	'Email',
	false,
	0,
	'nobody@healthgateway.gov.bc.ca'
);

/* User hthgtwy20 */
INSERT INTO gateway."Note"(
	"NoteId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"Title", 
	"Text", 
	"JournalDate")
VALUES (
	uuid_generate_v4(),
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	now(),
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	now(),
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	'Z5/Z7ZkAF8t5HpJq07QGBw==',
	'W/rhf01ikEQrN9v1wX5sfA==',
	now()
);

/* User hthgtwy20 */
INSERT INTO gateway."Note"(
	"NoteId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"Title", 
	"Text", 
	"JournalDate")
VALUES (
	uuid_generate_v4(),
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	'2022-01-17 00:48:01.11617',
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	'2022-01-17 00:48:01.11617',
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	'Z5/Z7ZkAF8t5HpJq07QGBw==',
	'W/rhf01ikEQrN9v1wX5sfA==',
	'2022-01-17'
);

/* User hthgtwy20 */
INSERT INTO gateway."Note"(
	"NoteId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"Title", 
	"Text", 
	"JournalDate")
VALUES (
	uuid_generate_v4(),
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	'2022-01-16 00:48:01.11617',
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	'2022-01-16 00:48:01.11617',
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',
	'Z5/Z7ZkAF8t5HpJq07QGBw==',
	'W/rhf01ikEQrN9v1wX5sfA==',
	'2022-01-16'
);

INSERT INTO gateway."UserFeedback"(
	"UserFeedbackId",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime",
	"IsSatisfied",
	"Comment",
	"IsReviewed",
	"UserProfileId")
VALUES (
	'6913821b-8c8c-4273-8cd8-afe6fdd05194',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2022-07-06 15:30:43.095652+00',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2022-07-06 15:30:43.095652+00',
	false,
	'The database seed script should populate some rows in the feedback table.',
	false,
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A'
);

INSERT INTO gateway."UserFeedback"(
	"UserFeedbackId",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime",
	"IsSatisfied",
	"Comment",
	"IsReviewed",
	"UserProfileId",
    "ClientCode")
VALUES (
	'57abe4d7-2f83-43fd-9037-cc11ab9c9a12',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2022-07-06 15:31:18.492904+00',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2022-07-06 15:31:18.492904+00',
	false,
	'I''d like to receive an email when I have a new health record.',
	false,
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
    'Web'
);

INSERT INTO gateway."UserFeedback"(
	"UserFeedbackId",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime",
	"IsSatisfied",
	"Comment",
	"IsReviewed",
	"UserProfileId",
    "ClientCode")
VALUES (
	'487ce155-1cd7-43b7-abf3-4cffb6b94b48',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2024-03-11 15:32:18.492904+00',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2024-03-11 15:32:18.492904+00',
	false,
	'I''d like to be able to view and create notes and comments on my health records in the mobile app.',
	false,
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
    'Mobile'
);

INSERT INTO gateway."UserFeedback"(
	"UserFeedbackId",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime",
	"IsSatisfied",
	"Comment",
	"IsReviewed",
	"UserProfileId",
    "ClientCode")
VALUES (
	'4bb2e090-8657-4057-b10d-8df4b75b0268',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2024-03-12 15:33:18.492904+00',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2024-03-12 15:33:18.492904+00',
	false,
	'Did Health Gateway get an update?',
	false,
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
    'Salesforce'
);

/* PHN: 9872868103 (508820774378599978) will have no Allowed Delegation record(s) */
INSERT INTO gateway."Dependent"(
	"HdId",
	"Protected",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime")
VALUES (
	'508820774378599978',
	true,
	'System',
	current_timestamp,
	'System',
	current_timestamp
);

/* PHN: 9872868128 (35224807075386271) */
INSERT INTO gateway."Dependent"(
	"HdId",
	"Protected",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime")
VALUES (
	'35224807075386271',
	true,
	'System',
	current_timestamp,
	'System',
	current_timestamp
);

/* Dependent PHN: 9872868128 (35224807075386271), Delegate PHN: 9735353315 (P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A) */
INSERT INTO gateway."AllowedDelegation"(
	"DependentHdId",
	"DelegateHdId",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime")
VALUES (
	'35224807075386271',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'System',
	current_timestamp,
	'System',
	current_timestamp
);

/* Dependent PHN: 9872868128 (35224807075386271) */
INSERT INTO gateway."AgentAudit"(
	"AgentAuditId",
	"Hdid",
	"AgentUsername",
	"Reason",
	"OperationCode",
	"GroupCode",
	"TransactionDateTime",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime")
VALUES (
	uuid_generate_v4(), 
	'35224807075386271', 
	'admin_agent@idir', 
	'Protecting dependent', 
	'ProtectDependent',
	'Dependent',
	current_timestamp,
	'System',
	current_timestamp,
	'System',
	current_timestamp
);

/* Dependent PHN: 9872868128 (35224807075386271) */
INSERT INTO gateway."AgentAudit"(
	"AgentAuditId",
	"Hdid",
	"AgentUsername",
	"Reason",
	"OperationCode",
	"GroupCode",
	"TransactionDateTime",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime")
VALUES (
	uuid_generate_v4(), 
	'35224807075386271', 
	'support_agent@idir', 
	'Unprotecting dependent', 
	'UnprotectDependent',
	'Dependent',
	current_timestamp - INTERVAL '1 day', 
	'System',
	current_timestamp - INTERVAL '1 day', 
	'System',
	current_timestamp - INTERVAL '1 day'
);

/* Dependent PHN: 9872868128 (35224807075386271) */
INSERT INTO gateway."AgentAudit"(
	"AgentAuditId",
	"Hdid",
	"AgentUsername",
	"Reason",
	"OperationCode",
	"GroupCode",
	"TransactionDateTime",
	"CreatedBy",
	"CreatedDateTime",
	"UpdatedBy",
	"UpdatedDateTime")
VALUES (
	uuid_generate_v4(), 
	'35224807075386271', 
	'reviewer_agent@idir', 
	'Protecting dependent', 
	'ProtectDependent',
	'Dependent',
	current_timestamp - INTERVAL '2 day', 
	'System',
	current_timestamp - INTERVAL '2 day', 
	'System',
	current_timestamp - INTERVAL '2 day'
);


INSERT INTO gateway."BlockedAccess" (
    "Hdid", 
    "DataSources", 
    "CreatedBy", 
    "CreatedDateTime", 
    "UpdatedBy", 
    "UpdatedDateTime"
)
VALUES (
    'GO4DOSMRJ7MFKPPADDZ3FK2MOJ45SFKONJWR67XNLMZQFNEHDKDA', 
    '[
        "ClinicalDocument",
        "Covid19TestResult",
        "HealthVisit",
        "Medication",
        "SpecialAuthorityRequest",
        "Immunization",
        "DiagnosticImaging",
        "HospitalVisit",
        "Note",
        "OrganDonorRegistration",
        "LabResult",
        "BcCancerScreening"
    ]', 
    'System', 
    current_timestamp - INTERVAL '2 day', 
    'System', 
    current_timestamp - INTERVAL '2 day'
);

INSERT INTO gateway."BlockedAccess"(
	"Hdid", 
	"DataSources", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime")
VALUES (
		'S22BPV6WHS5TRLBL4XKGQDBVDUKLPIRSBGYSEJAHYMYRP22SP2TA', 
		'[]', 
		'System', 
		current_timestamp - INTERVAL '2 day', 
		'System', 
		current_timestamp - INTERVAL '2 day'
);