import { CovidTherapyAssessmentOption } from "@/constants/covidTherapyAssessmentOption";

import { StringISODateTime } from "./dateWrapper";

export default interface CovidTherapyAssessmentRequest {
    phn?: string;
    firstName?: string;
    lastName?: string;
    phoneNumber?: string;
    identifiesIndigenous: CovidTherapyAssessmentOption;
    hasAFamilyDoctorOrNp: CovidTherapyAssessmentOption;
    confirmsOver12: boolean;
    testedPositiveInPast7Days: CovidTherapyAssessmentOption;
    hasSevereCovid19Symptoms: CovidTherapyAssessmentOption;
    hasMildOrModerateCovid19Symptoms: boolean;
    symptomOnSetDate: StringISODateTime;
    hasImmunityCompromisingMedicalConditionAntiViralTri: CovidTherapyAssessmentOption;
    reports3DosesC19Vaccine: CovidTherapyAssessmentOption;
    hasChronicConditionDiagnoses: CovidTherapyAssessmentOption;
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
