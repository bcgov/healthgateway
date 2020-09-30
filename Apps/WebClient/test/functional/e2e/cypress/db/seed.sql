
/* Unregistered HealthGateway User */
DELETE FROM gateway."UserProfile" WHERE "UserProfileId" = 'S22BPV6WHS5TRLBL4XKGQDBVDUKLPIRSBGYSEJAHYMYRP22SP2TA';
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
	'S22BPV6WHS5TRLBL4XKGQDBVDUKLPIRSBGYSEJAHYMYRP22SP2TA',	
	'System', 
	current_timestamp, 
	'System', 
	current_timestamp, 
	false, 
	null,
	null,
	null,
	null,
	'9FdkVaUS2F/q0yiJiPR1aiOAsIe5LkAewFPG0vawg1Y=',
	null
);

/* Registered HealthGateway User */
DELETE FROM gateway."UserProfile" WHERE "UserProfileId" = 'P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A';
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
	null,
	'CwqU7+gCkL3jMWWcUpq80Oh42QejXOwI+Ov0tmsVWBI=',
	null
);

/* Protected User */
DELETE FROM gateway."UserProfile" WHERE "UserProfileId" = 'RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWQ';
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
	null,
	'iHa5atSWqppGzWsR1Z8nbL9OHJamPHLMwYqdKmsf4jU=',
	null
);