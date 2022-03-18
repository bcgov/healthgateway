import { CovidTreatmentAssessmentOption } from "@/constants/covidTreatmentAssessmentOption";

import { StringISODateTime } from "./dateWrapper";

export default interface CovidTreatmentAssessmentRequest {
    phn?: string;
    firstName?: string;
    lastName?: string;
    dob?: string;
    phoneNumber?: string;
    identifiesIndigenous: CovidTreatmentAssessmentOption;
    hasAFamilyDoctorOrNp: CovidTreatmentAssessmentOption;
    confirmsOver12: boolean;
    testedPositiveInPast7Days: CovidTreatmentAssessmentOption;
    hasSevereCovid19Symptoms: CovidTreatmentAssessmentOption;
    hasMildOrModerateCovid19Symptoms: CovidTreatmentAssessmentOption;
    symptomOnSetDate?: StringISODateTime | null;
    hasImmunityCompromisingMedicalCondition: CovidTreatmentAssessmentOption;
    reports3DosesC19Vaccine: CovidTreatmentAssessmentOption;
    hasChronicConditionDiagnoses: CovidTreatmentAssessmentOption;
    consentToSendCC: boolean;
    agentComments?: string;
    streetAddresses?: string[];
    provOrState?: string;
    postalCode?: string;
    city?: string;
    country?: string;
    changeAddressFlag: boolean;
    positiveCovidLabData?: string;
    covidVaccinationHistory?: string;
    cevGroupDetails?: string;
    submitted?: StringISODateTime;
}
