import { StringISODate } from "@/models/dateWrapper";

export default class PatientData {
    public hdid!: string;
    public personalhealthnumber!: string;
    public preferredname!: Name;
    public commonname: Name | undefined;
    public legalname: Name | undefined;
    public birthdate?: StringISODate;
    public physicalAddress?: Address;
    public postalAddress?: Address;
}
export class Name {
    public givenName!: string;
    public surname!: string;
}
export class Address {
    public city!: string;
    public state!: string;
    public postalCode!: string;
    public country!: string;
    public streetLines!: string[];
}
