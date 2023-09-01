import { CommentEntryType } from "@/constants/commentEntryType";
import { StringISODate } from "@/models/dateWrapper";

export interface UserComment {
    // The comment id.
    id: string;
    // The user hdid.
    userProfileId: string;
    // The entry this comment belongs to.
    parentEntryId: string;
    // The text of the comment.
    text: string;
    // Comment datetime.
    createdDateTime: StringISODate;
    // Comment db version.
    version: number;
    // The comment's parent entry type.
    entryTypeCode: CommentEntryType;
    // The comment's updated by.
    updatedBy?: string;
    // The comment update date time.
    updatedDateTime?: StringISODate;
}
