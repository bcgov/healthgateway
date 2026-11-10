/** Port of models/immunizationModel.ts and models/immunizationResult.ts */
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
    immunization: Immunization;
    status: string;
    /** ISO date */
    dateOfImmunization: string;
    providerOrClinic: string;
    forecast?: Forecast;
}

export interface Recommendation {
    recommendedVaccinations: string;
    recommendationSetId: string;
    diseaseEligibleDate?: string;
    diseaseDueDate?: string;
    agentEligibleDate?: string;
    agentDueDate?: string;
    status: string;
    targetDiseases: TargetDisease[];
    immunization: Immunization;
}

export interface TargetDisease {
    code: string;
    name: string;
}

export interface Forecast {
    createDate: string;
    status: string;
    displayName: string;
    eligibleDate: string;
    dueDate: string;
}

export interface ImmunizationResult {
    immunizations: ImmunizationEvent[];
    recommendations: Recommendation[];
}
