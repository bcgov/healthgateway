CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DO $$
DECLARE 
	/*** NEEDS TO BE POPULATED ***/
    host VARCHAR(100) := '';
    emailToArr VARCHAR[] := array[
		'email1', 
		'email2'];
	/*** END  ***/
    emailId UUID;
    inviteKey UUID;
    createdAt TIMESTAMP := now() at time zone 'utc';
    createdBy VARCHAR(50) := 'Manual Script';
    emailTemplate TEXT := (SELECT "Body" FROM gateway."EmailTemplate" WHERE "Name" = 'Invite');
    emailTo VARCHAR;
    emailBody TEXT;
BEGIN
    RAISE NOTICE 'Processing batch';

    FOREACH emailTo IN ARRAY emailToArr
    LOOP
	    RAISE NOTICE 'Inserting %', emailTo;

        emailId := uuid_generate_v4();
        inviteKey := uuid_generate_v4();
        emailBody := emailTemplate;
        emailBody := REPLACE(emailBody, '${host}', host);
        emailBody := REPLACE(emailBody, '${inviteKey}', CAST (inviteKey AS VARCHAR(36) ));
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
    END LOOP;

    RAISE NOTICE 'Finished processing batch';
END $$;