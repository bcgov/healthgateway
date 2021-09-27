import { StringISODate } from "@/models/dateWrapper";

export interface VaccineDetails {
    doses: VaccineDose[];
    vaccineStatus: string;
}

export interface VaccineDose {
    product: string;
    lot: string;
    location: string;
    date: StringISODate;
}
