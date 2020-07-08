// Model that provides a user representation of a BetaRequest.
export default interface Email {
    // Gets or sets the primary key of this Email entity.
    id: string;

    // Gets or sets the From address for sending the email.
    from: string;

    // Gets or sets the To address for sending the email.
    to: string;

    // Gets or sets the Subject line of the email.
    subject: string;

    // Gets or sets the Date/Time we last tried to send the email.
    sentDateTime: Date;

    // Gets or sets the state of the Email (New, Pending ...).
    emailStatusCode: string;
}
