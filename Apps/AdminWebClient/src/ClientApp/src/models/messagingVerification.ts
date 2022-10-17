import { StringISODateTime } from "@/models/dateWrapper";

export const enum VerificationType {
    Email = "Email",
    SMS = "SMS",
}

export default interface MessagingVerification {
    id: string;
    userProfileId: string;
    validated: boolean;
    emailId?: string;
    email?: string;
    inviteKey: string;
    verificationType: VerificationType;
    smsNumber: string | null;
    smsValidationCode: string;
    expireDate: StringISODateTime;
    verificationAttempts: number;
    deleted: boolean;
    updatedDateTime: StringISODateTime;
}
