import { CovidTreatmentAssessmentOption } from "@/constants/CovidTreatmentAssessmentOption";

import { StringISODateTime } from "./dateWrapper";

export default interface CovidTreatmentAssessmentRequest {
    phn?: string;
    firstName?: string;
    lastName?: string;
    phoneNumber?: string;
    identifiesIndigenous: CovidTreatmentAssessmentOption;
    hasAFamilyDoctorOrNp: CovidTreatmentAssessmentOption;
    confirmsOver12: boolean;
    testedPositiveInPast7Days: CovidTreatmentAssessmentOption;
    hasSevereCovid19Symptoms: CovidTreatmentAssessmentOption;
    hasMildOrModerateCovid19Symptoms: boolean;
    symptomOnSetDate: StringISODateTime;
    hasImmunityCompromisingMedicalConditionAntiViralTri: CovidTreatmentAssessmentOption;
    reports3DosesC19Vaccine: CovidTreatmentAssessmentOption;
    hasChronicConditionDiagnoses: CovidTreatmentAssessmentOption;
    agentComments?: string;
    streetAddress?: string;
    provOrState?: string;
    postalCode?: string;
    country?: string;
    changeAddressFlag: boolean;
    positiveCovidLabData?: string;
    covidVaccinationHistory?: string;
    cevGroupDetails?: string;
    submitted?: StringISODateTime;
}
