import { StringISODate } from "@/models/dateWrapper";

export default class PatientData {
    public hdid!: string;
    public personalHealthNumber!: string;
    public preferredName!: Name;
    public commonName: Name | undefined;
    public legalName: Name | undefined;
    public birthdate?: StringISODate;
    public physicalAddress?: Address;
    public postalAddress?: Address;
}

export class Name {
    public givenName: string | undefined;
    public surname: string | undefined;
}

export class Address {
    public city!: string;
    public state!: string;
    public postalCode!: string;
    public country!: string;
    public streetLines!: string[];
}
