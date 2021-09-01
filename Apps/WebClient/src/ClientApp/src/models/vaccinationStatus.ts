import { VaccinationState } from "@/constants/vaccinationState";
import { StringISODate } from "@/models/dateWrapper";
import EncodedMedia from "@/models/encodedMedia";

export default interface VaccinationStatus {
    id: string;
    loaded: boolean;
    retryin: number;
    personalhealthnumber: string | null;
    firstname: string | null;
    lastname: string | null;
    birthdate: StringISODate | null;
    vaccinedate: StringISODate | null;
    doses: number;
    state: VaccinationState;
    qrCode: EncodedMedia;
}
