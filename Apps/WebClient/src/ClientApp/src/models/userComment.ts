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
    // The comment's updated by.
    updatedBy?: string;
    // The comment update date time.
    updatedDateTime?: StringISODate;
}

export const enum CommentEntryType {
    // The code represeting no entry type set.
    None = "NA",
    // The code representing Special Authority Requests.
    MedicationRequest = "SAR",
    // The code representing Medication.
    Medication = "Med",
    // The code representing Immunization.
    Immunization = "Imm",
    // The code representing COVID-19 Laboratory Orders.
    Covid19LaboratoryOrder = "Lab",
    // The code representing all Laboratory Orders.
    LaboratoryOrder = "ALO",
    // The code representing Encounter.
    Encounter = "Enc",
}

export class EntryTypeMapper {
    public static toCommentEntryType(entryType: EntryType): CommentEntryType {
        switch (entryType) {
            case EntryType.MedicationRequest:
                return CommentEntryType.MedicationRequest;
            case EntryType.Medication:
                return CommentEntryType.Medication;
            case EntryType.Immunization:
                return CommentEntryType.Immunization;
            case EntryType.Covid19LaboratoryOrder:
                return CommentEntryType.Covid19LaboratoryOrder;
            case EntryType.LaboratoryOrder:
                return CommentEntryType.LaboratoryOrder;
            case EntryType.Encounter:
                return CommentEntryType.Encounter;
            default:
                return CommentEntryType.None;
        }
    }
}
