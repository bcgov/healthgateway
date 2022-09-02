import Address from "@/models/address";
import { StringISODate } from "@/models/dateWrapper";

export default interface PatientData {
    hdid: string;
    personalhealthnumber: string;
    firstname: string;
    lastname: string;
    birthdate: StringISODate;
    physicalAddress: Address | null;
    postalAddress: Address | null;
    responseCode: string;
}
