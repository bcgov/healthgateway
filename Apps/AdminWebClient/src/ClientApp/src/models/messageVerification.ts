import { StringISODateTime } from "@/models/dateWrapper";

export const enum VerificationType {
    Email = "Email",
    SMS = "SMS",
}

export default interface MessageVerification {
    id: string;
    personalHealthNumber: string;
    userProfileId: string;
    validated: boolean;
    emailId?: string;
    email: Email | null;
    inviteKey: string;
    verificationType: VerificationType;
    smsNumber: string | null;
    smsValidationCode: string;
    expireDate: StringISODateTime;
    verificationAttempts: number;
    deleted: boolean;
    updatedDateTime: StringISODateTime;
}

export interface Email {
    id: string;
    from: string;
    to: string;
    subject: string;
    body: string;
    sentDateTime: StringISODateTime;
    lastRetryDateTime?: StringISODateTime;
    attempts: number;
    smtpStatusCode: number;
}
