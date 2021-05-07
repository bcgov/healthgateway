export interface AdminTag {
    id: string; // PK
    tagName: string; // Unique
}

export interface FeedbackTags {
    id: string; // PK

    feedbackId: string; // feedback.id (FK)
    adminTagId: string; // adminTag.id (FK)

    tagName: string; // Unique
}
