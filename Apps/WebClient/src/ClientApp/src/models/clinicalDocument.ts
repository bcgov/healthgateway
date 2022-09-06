import { StringISODate } from "@/models/dateWrapper";

export default interface ClinicalDocument {
    id: string;
    fileId: string;
    name: string;
    type: string;
    facilityName: string;
    discipline: string;
    serviceDate: StringISODate;
}
