export enum CommunicationType {
    Email = "Email",
    Banner = "Banner"
}

export enum CommunicationStatus {
    New = "New",
    Processed = "Processed"
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
    effectiveDateTime: Date;

    // Gets or sets the expiry date.
    expiryDateTime: Date;

    // Gets or sets the scheduled date of the email
    scheduledDateTime: Date;

    // Gets or sets the email communication priority
    priority: number;

    // Gets or sets the communication type: email or banner
    communicationTypeCode: string;

    // Gets or sets the communication status code: new or processed
    communicationStatusCode: string;

    // The communication version number.
    version: number;
}
