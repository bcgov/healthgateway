export default interface Communication {
    // Gets or sets the id.
    id: string;

    // Gets or sets the communication subject.
    subject: string;

    // Gets or sets the communication text.
    text: string;

    // Gets or sets the type of the Communication.
    communicationTypeCode: CommunicationType;

    // Gets or sets the state of the Communication.
    communicationStatusCode: CommunicationStatus;

    // Gets or sets the effective date time
    effectiveDateTime: Date;

    // Gets or sets the expiry date time
    expiryDateTime: Date;
}

export enum CommunicationStatus {
    // New communication.
    New,

    // Errored out.
    Error,

    // Communication that has been sent.
    Processed,

    // Pending communication.
    Pending,

    // Processing communication.
    Processing,

    // Draft communication.
    Draft,
}

export enum CommunicationType {
    // Banner communication.
    Banner,

    // Email communication.
    Email,

    // Communication inside the app.
    InApp,
}
