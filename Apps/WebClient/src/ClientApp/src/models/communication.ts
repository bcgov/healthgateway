export default interface Communication {
    // Gets or sets the communication text.
    text: string;

    // Gets or sets the type of the Communication.
    communicationTypeCode: CommunicationType;
}

export enum CommunicationType {
    // Banner communication.
    Banner = 0,

    // Communication inside the app.
    InApp = 2,
}
