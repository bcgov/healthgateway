import { StringISODate } from "@/models/dateWrapper";

export type ForecastStatus = string;

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
    recommendationSetId: string;
    diseaseEligibleDate?: StringISODate;
    diseaseDueDate?: StringISODate;
    agentEligibleDate?: StringISODate;
    agentDueDate?: StringISODate;

    status: ForecastStatus;
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
    status: ForecastStatus;
    displayName: string;
    eligibleDate: StringISODate;
    dueDate: StringISODate;
}
