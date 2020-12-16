import { StringISODate } from "@/models/dateWrapper";

export default interface ImmunizationAgent {
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
    dateOfImmunization: StringISODate;
    providerOrClinic: string;
    immunizationAgents: ImmunizationAgent[];
}
