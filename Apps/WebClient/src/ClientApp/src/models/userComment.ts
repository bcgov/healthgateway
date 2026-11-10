import type { CommentEntryType } from "@/constants/commentEntryType";

/** Port of models/userComment.ts */
export interface UserComment {
    id: string;
    userProfileId: string;
    parentEntryId: string;
    text: string;
    /** ISO date-time */
    createdDateTime: string;
    version: number;
    entryTypeCode: CommentEntryType;
    updatedBy?: string;
    updatedDateTime?: string;
}
