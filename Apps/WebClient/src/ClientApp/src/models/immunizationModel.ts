import { StringISODate } from "@/models/dateWrapper";

export interface Immunization {
    name: string;
    immunizationAgents: ImmunizationAgent[];
}

export interface ImmunizationAgent {
    code: string;
    name: string;
    lotNumber: string;
    productName: string;
}

export interface ImmunizationEvent {
    id: string;
    isSelfReported: boolean;
    location: string;
    immunization: Immunization;
    status: string;
    valid: boolean;
    dateOfImmunization: StringISODate;
    providerOrClinic: string;
    targetedDisease: string;
    forecast?: Forecast;
}

export interface Recommendation {
    recommendedVaccinations: string;
    recommendationSetId: string;
    diseaseEligibleDate?: StringISODate;
    diseaseDueDate?: StringISODate;
    agentEligibleDate?: StringISODate;
    agentDueDate?: StringISODate;

    status: string;
    targetDiseases: TargetDisease[];

    immunization: Immunization;
}

export interface TargetDisease {
    code: string;
    name: string;
}

export interface Forecast {
    recommendationId: string;
    createDate: StringISODate;
    status: string;
    displayName: string;
    eligibleDate: StringISODate;
    dueDate: StringISODate;
}
