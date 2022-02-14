import { StringISODateTime } from "@/models/dateWrapper";

export default interface PreviousAssessmentDetailsList {
    dateTimeOfAssessment: StringISODateTime;
    formId: string;
}
