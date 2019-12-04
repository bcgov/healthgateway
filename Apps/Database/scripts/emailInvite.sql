CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DO $$
DECLARE 
    emailId UUID := uuid_generate_v4();
    createdAt TIMESTAMP := now() at time zone 'utc';
    createdBy VARCHAR(50) := 'Manual Script';
    emailTo VARCHAR(50) := null;
    host VARCHAR(100) := null;
    inviteKey UUID := uuid_generate_v4();
    emailBody TEXT := (SELECT "Body" FROM gateway."EmailTemplate" WHERE "Name" = 'Invite');
BEGIN
	RAISE NOTICE 'Inserting %', emailTo;

    emailBody := REPLACE(emailBody, '${host}', host);
    emailBody := REPLACE(emailBody, '${inviteKey}', CAST (inviteKey AS VARCHAR(32) ));
    emailBody := REPLACE(emailBody, '${emailTo}', emailTo);

	INSERT INTO gateway."Email"(
		"EmailId", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "From", "To", "Subject", "Body", "FormatCode", "Priority", "SentDateTime", "LastRetryDateTime", "Attempts", "SmtpStatusCode", "EmailStatusCode")
    SELECT 
        emailid, createdBy, createdAt, createdBy, createdAt, "From", emailTo, "Subject", emailBody, "FormatCode", "Priority", null, null, 0, 0, 'New'
    FROM gateway."EmailTemplate" WHERE "Name" = 'Invite';

	INSERT INTO gateway."EmailInvite"(
		"EmailInviteId", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "HdId", "Validated", "EmailId", "InviteKey")
		VALUES (uuid_generate_v4(), createdBy, createdAt, createdBy, createdAt, null, false, emailId, inviteKey);

	RAISE NOTICE 'Finished Inserting %', emailTo;
END $$;

