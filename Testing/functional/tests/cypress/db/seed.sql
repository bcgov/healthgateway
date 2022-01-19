/* Data Cleanup */
TRUNCATE gateway."UserProfile" CASCADE;
TRUNCATE gateway."UserProfileHistory" CASCADE;
TRUNCATE gateway."Communication" CASCADE;
TRUNCATE gateway."ResourceDelegate" CASCADE;
TRUNCATE gateway."ResourceDelegateHistory" CASCADE;
TRUNCATE gateway."UserPreference" CASCADE;
TRUNCATE gateway."Comment" CASCADE;
TRUNCATE gateway."MessagingVerification" CASCADE;
TRUNCATE gateway."Note" CASCADE;

/* Registered HealthGateway User */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"AcceptedTermsOfService", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber")
VALUES (
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',	
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	true, 
	null,
	null,
	null,
	current_timestamp, 
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null
);

/* Protected User */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"AcceptedTermsOfService", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber")
VALUES (
	'RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWQ',	
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	true, 
	null,
	null,
	null,
	current_timestamp, 
	'iHa5atSWqppGzWsR1Z8nbL9OHJamPHLMwYqdKmsf4jU=',
	null
);

/* Invaliddoses - Keycloak User */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"AcceptedTermsOfService", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber")
VALUES (
	'DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA',	
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	true, 
	null,
	null,
	null,
	current_timestamp, 
	'SnSf90IS+9I75+wXNuInLdod7s9bpiSjWW4vs94g7BY=',
	null
);

/* Notfound - Keycloak User */
INSERT INTO gateway."UserProfile"(
	"UserProfileId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"AcceptedTermsOfService", 
	"Email", 
	"ClosedDateTime", 
	"IdentityManagementId", 
	"LastLoginDateTime", 
	"EncryptionKey", 
	"SMSNumber")
VALUES (
	'R43YCT4ZY37EIJLW2O5LV2I77BZA3K3M25EUJGWAVGVJ7JKBDKCQ',
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	true, 
	null,
	null,
	null,
	current_timestamp, 
	'KtBm7JYegayKpx5fjwM2RUGZf79JOnNC21NhUrIAzmg=',
	null
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
	'actionedCovidModalAt', 
	'2150-01-01T12:00:00.000-08:00'
);

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
	'tutorialMenuNote', 
	'false'
);

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
	'tutorialMenuExport', 
	'false'
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
	'COVIDLab', 
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
	'COVIDLab', 
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	'System.DateTime, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e', 
	'"2021-01-20T00:00:00"'
);

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
	false,
	null,
	'00000000-0000-0000-0000-000000000000',
	now()+INTERVAL '1 day',
	'2501234567',
	'123456',
	'SMS',
	false,
	0
);

INSERT INTO gateway."Note"(
	"NoteId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"Title", 
	"Text", 
	"JournalDateTime")
VALUES (
	uuid_generate_v4(),
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	now(),
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	now(),
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'Z5/Z7ZkAF8t5HpJq07QGBw==',
	'W/rhf01ikEQrN9v1wX5sfA==',
	now()
);


INSERT INTO gateway."Note"(
	"NoteId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"Title", 
	"Text", 
	"JournalDateTime")
VALUES (
	uuid_generate_v4(),
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2022-01-17 00:48:01.11617',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2022-01-17 00:48:01.11617',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'Z5/Z7ZkAF8t5HpJq07QGBw==',
	'W/rhf01ikEQrN9v1wX5sfA==',
	'2022-01-17 00:48:01.116171'
);

INSERT INTO gateway."Note"(
	"NoteId", 
	"CreatedBy", 
	"CreatedDateTime", 
	"UpdatedBy", 
	"UpdatedDateTime", 
	"UserProfileId", 
	"Title", 
	"Text", 
	"JournalDateTime")
VALUES (
	uuid_generate_v4(),
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2022-01-16 00:48:01.11617',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'2022-01-16 00:48:01.11617',
	'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A',
	'Z5/Z7ZkAF8t5HpJq07QGBw==',
	'W/rhf01ikEQrN9v1wX5sfA==',
	'2022-01-16 00:48:01.11617'
);

