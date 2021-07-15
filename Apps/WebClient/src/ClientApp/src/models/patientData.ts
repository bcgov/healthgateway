import { StringISODate } from "@/models/dateWrapper";

export default class PatientData {
    public hdid!: string;
    public personalhealthnumber!: string;
    public firstname!: string;
    public lastname!: string;
    public birthdate?: StringISODate;
}
