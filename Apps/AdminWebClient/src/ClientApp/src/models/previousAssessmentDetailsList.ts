import { StringISODate } from "@/models/dateWrapper";

export default interface PreviousAssessmentDetailsList {
    dateOfAssessment: StringISODate;
    timeOfAssessment: string;
    formId: string;
}
