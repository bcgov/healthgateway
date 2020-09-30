/* Data Cleanup */
TRUNCATE gateway."UserProfile" CASCADE;
TRUNCATE gateway."Communication" CASCADE;

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
	null,
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
	null,
	'iHa5atSWqppGzWsR1Z8nbL9OHJamPHLMwYqdKmsf4jU=',
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