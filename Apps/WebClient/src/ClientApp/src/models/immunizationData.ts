export default interface ImmunizationData {
    id: string;
    name: string;
    status: string;
    immunizationAgents: ImmunizationAgent[];
    occurrenceDateTime: Date;
}

export interface ImmunizationAgent {
    code: string;
    name: string;
}
