import { StringISODateTime } from "@/models/dateWrapper";

export const enum CommunicationType {
    Email = "Email",
    Banner = "Banner",
    InApp = "InApp",
}

export const enum CommunicationStatus {
    New = "New",
    Processed = "Processed",
    Error = "Error",
    Pending = "Pending",
    Processing = "Processing",
    Draft = "Draft",
}

// Model that provides a user representation of admin communications.
export default interface Communication {
    // Gets or sets the id.
    id?: string;

    // Gets or sets the subject.
    subject: string;

    // Gets or sets the text.
    text: string;

    // Gets or sets the effective date.
    effectiveDateTime: StringISODateTime;

    // Gets or sets the expiry date.
    expiryDateTime: StringISODateTime;

    // Gets or sets the scheduled date of the email
    scheduledDateTime: StringISODateTime;

    // Gets or sets the email communication priority
    priority: number;

    // Gets or sets the communication type: email or banner
    communicationTypeCode: CommunicationType;

    // Gets or sets the communication status code: new or processed
    communicationStatusCode: string;

    // The communication version number.
    version: number;
}
