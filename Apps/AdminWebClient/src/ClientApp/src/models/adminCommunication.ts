export enum EntryType {
    Email = "Email",
    Banner = "Banner"
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

    // Gets or sets the email communication priority
    priority: number;

    // Gets or sets the communication type: email or banner
    communicationTypeCode: string;

    // The communication version number.
    version: number;
}
