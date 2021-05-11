// Model that provides a user representation of user feedback.
export default interface UserFeedback {
    // Feedback unique identifier.
    id: string;

    // Satisfied flag.
    isSatisfied: boolean;

    // Reviewed flag.
    isReviewed: boolean;

    // Comments on the feedback.
    comment: string;

    // Date when the feedback was created.
    createdDateTime: Date;

    //The row concurrency version.
    version: string;

    // Email if known for this feedback.
    email: string;

    // The feedback tags
    tags: AdminTag[];
}

export interface AdminTag {
    id: string;
    name: string;
    version: number;
}
