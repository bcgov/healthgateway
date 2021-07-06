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
    expireDate: Date;
    verificationAttempts: number;
    deleted: boolean;
    updatedDateTime: Date;
}

export interface Email {
    id: string;
    from: string;
    to: string;
    subject: string;
    body: string;
    sentDateTime: Date;
    lastRetryDateTime?: string;
    attempts: number;
    smtpStatusCode: number;
}
