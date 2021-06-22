export const enum MessagingVerificationType {
    Email = "Email",
    SMS = "SMS",
}

// Model that provides a user representation of admin communications.
export default interface MessageVerification {
    id: string;
    hdId: string;
    validated: boolean;
    emailId?: string;
    //email : Email? ;
    inviteKey: string;
    verificationType: string;
    SMSNumber?: string;
    SMSValidationCode?: string;
    expireDate: string;
    verificationAttempts: number;
    deleted: boolean;
}
