export const enum VerificationType {
    Email = "Email",
    SMS = "SMS",
}

export default interface MessageVerification {
    id: string;
    userProfileId: string;
    validated: boolean;
    emailId?: string;
    email: Email | null;
    inviteKey: string;
    verificationType: VerificationType;
    smsNumber: string | null;
    smsValidationCode: string;
    expireDate: string;
    verificationAttempts: number;
    deleted: boolean;
    updatedDateTime: string;
}

export interface Email {
    id: string;
    from: string;
    to: string;
    subject: string;
    body: string;
    sentDateTime: string;
    lastRetryDateTime?: string;
    attempts: number;
    smtpStatusCode: number;
}
