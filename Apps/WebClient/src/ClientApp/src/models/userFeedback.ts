export interface UserFeedback {
    comment: string;
    clientType?: string;
    isSatisfied?: boolean;
}

export interface UserRating {
    ratingValue: number;
    skip: boolean;
}
