import { StringISODate } from "@/models/dateWrapper";
import { EntryType } from "@/models/timelineEntry";

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
}

export const enum CommentEntryType {
    // The code represeting no entry type set.
    None = "NA",
    // The code representing Medication.
    Medication = "Med",
    // The code representing Immunization.
    Immunization = "Imm",
    // The code representing Laboratory.
    Laboratory = "Lab",
    // The code representing Encounter.
    Encounter = "ENC",
}

export class EntryTypeMapper {
    public static toCommentEntryType(entryType: EntryType): CommentEntryType {
        console.log(entryType);
        switch (entryType) {
            case EntryType.Medication:
                return CommentEntryType.Medication;
            case EntryType.Immunization:
                return CommentEntryType.Immunization;
            case EntryType.Encounter:
                return CommentEntryType.Encounter;
            default:
                return CommentEntryType.None;
        }
    }
}
