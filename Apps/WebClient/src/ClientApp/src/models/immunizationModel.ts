import { StringISODate } from "@/models/dateWrapper";

export interface ImmunizationAgent {
    code: string;
    name: string;
    lotNumber: string;
    productName: string;
}

export default interface ImmunizationModel {
    id: string;
    isSelfReported: boolean;
    location: string;
    name: string;
    status: string;
    dateOfImmunization: StringISODate;
    providerOrClinic: string;
    immunizationAgents: ImmunizationAgent[];
}
