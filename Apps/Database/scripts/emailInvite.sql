CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DO $$
DECLARE 
    emailId UUID := uuid_generate_v4();
    createdAt TIMESTAMP := now() at time zone 'utc';
    createdBy VARCHAR(50) := 'Manual Script';
    emailTo VARCHAR(50) := null;
    emailFrom VARCHAR(50) := 'HG_Donotreply@gov.bc.ca';
    emailSubject VARCHAR(50) := 'Health Gateway Private Invitation';
    host VARCHAR(100) := null;
    inviteKey UUID := uuid_generate_v4();
    emailBody TEXT := concat('
    <!doctype html>
    <html lang="en">
    <head></head>
    <body style="margin:0">
    <table cellspacing="0" align="left" width="100%" style="margin:0;color:#707070;font-family:Helvetica;font-size:12px;">
        <tr style="background:#036;">
            <th width="45"></th>
            <th width="350" align="left" style="text-align:left;">
                <div role="img" aria-label="Health Gateway Logo">
                    <img src="', host, '/Logo" alt="Health Gateway Logo"/>
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
                <a style = "color:#1292c5;font-weight:600;" href = "',
                    host, '/registrationInfo?inviteKey=', inviteKey, '&email=', emailTo,
                '">Register Now</a>
                <p>If you have any questions about the registration process, including signing up to use your BC Services Card for authentication, please contact Nino Samson at nino.samson@gov.bc.ca.</p>
            </td>
            <td></td>
        </tr>
    </table>
</body>
</html>
');
BEGIN
	RAISE NOTICE 'Inserting %', emailTo;
	INSERT INTO gateway."Email"(
		"EmailId", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "From", "To", "Subject", "Body", "FormatCode", "Priority", "SentDateTime", "LastRetryDateTime", "Attempts", "SmtpStatusCode", "EmailStatusCode")
		VALUES (emailid, createdBy, createdAt, createdBy, createdAt, emailFrom, emailTo, emailSubject, emailBody, 'HTML', 5, null, null, 0, 0, 'New');

	INSERT INTO gateway."EmailInvite"(
		"EmailInviteId", "CreatedBy", "CreatedDateTime", "UpdatedBy", "UpdatedDateTime", "HdId", "Validated", "EmailId", "InviteKey")
		VALUES (uuid_generate_v4(), createdBy, createdAt, createdBy, createdAt, null, false, emailId, inviteKey);
	RAISE NOTICE 'Finished Inserting %', emailTo;
END $$;

