import { StringISODate } from "@/models/dateWrapper";

export default class PatientData {
    public hdid!: string;
    public personalhealthnumber!: string;
    public firstname!: string;
    public lastname!: string;
    public birthdate?: StringISODate;
    public physicalAddress?: Address;
    public postalAddress?: Address;
}

export class Address {
    public city!: string;
    public state!: string;
    public postalCode!: string;
    public country!: string;
    public streetLines!: string[];
}
