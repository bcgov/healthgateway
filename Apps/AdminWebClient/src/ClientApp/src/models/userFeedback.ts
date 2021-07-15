import { StringISODateTime } from "@/models/dateWrapper";

// Model that provides a user representation of user feedback.
export default interface UserFeedback {
    // Feedback unique identifier.
    id: string;

    // Associated user unique identifier.
    userProfileId: string;

    // Satisfied flag.
    isSatisfied: boolean;

    // Reviewed flag.
    isReviewed: boolean;

    // Comments on the feedback.
    comment: string;

    // Date when the feedback was created.
    createdDateTime: StringISODateTime;

    //The row concurrency version.
    version: string;

    // Email if known for this feedback.
    email: string;

    // The feedback tags
    tags: UserFeedbackTag[];
}

export interface AdminTag {
    id: string;
    name: string;
    version: number;
}

export interface UserFeedbackTag {
    id: string;
    tag: AdminTag;
    version: number;
}
