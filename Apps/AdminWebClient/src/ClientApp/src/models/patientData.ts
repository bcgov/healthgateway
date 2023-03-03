import Address from "@/models/address";
import { StringISODate } from "@/models/dateWrapper";
import Name from "@/models/Name";

export default interface PatientData {
    hdid: string;
    personalhealthnumber: string;
    preferredName: Name;
    commonName: Name;
    legalName: Name;
    birthdate: StringISODate;
    physicalAddress: Address | null;
    postalAddress: Address | null;
    responseCode: string;
}
