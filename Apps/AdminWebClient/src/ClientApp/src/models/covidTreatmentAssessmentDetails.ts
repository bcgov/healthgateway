import PreviousAssessmentDetailsList from "./previousAssessmentDetailsList";

export default interface CovidTreatmentAssessmentDetails {
    hasKnownPositiveC19Past7Days: boolean;
    citizenIsConsideredImmunoCompromised: boolean;
    has3DoseMoreThan14Days: boolean;
    hasDocumentedChronicCondition: boolean;
    previousAssessmentDetailsList?: PreviousAssessmentDetailsList[];
}
