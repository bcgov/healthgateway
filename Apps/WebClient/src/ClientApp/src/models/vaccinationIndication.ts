import { VaccinationStatus } from "@/constants/vaccinationStatus";
import { StringISODate } from "@/models/dateWrapper";

export default interface VaccinationIndication {
    // The patient's name
    name: string;

    // The patient's date of birth
    dateOfBirth: StringISODate;

    // The patient's vaccination status
    status: VaccinationStatus;

    // The number of vaccination doses the patient has received
    doses: number;
}
