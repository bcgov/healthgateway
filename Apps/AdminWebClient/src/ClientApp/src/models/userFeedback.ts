// Model that provides a user representation of user feedback.
export default interface UserFeedback {
    // Gets or sets the beta request id.
    id: string;

    // Gets or sets the is satisfied flag.
    isSatisfied: boolean;

    // Gets or sets the is reviewed flag.
    isReviewed: boolean;

    // Gets or sets the comments on the feedback.
    comment: string;

    // Gets or sets the date when the feedback was created.
    createdDateTime: Date;

    // Gets or sets the row concurrency version.
    version: string;

    // Gets or sets the email if known for this feedback.
    email: string;
}
