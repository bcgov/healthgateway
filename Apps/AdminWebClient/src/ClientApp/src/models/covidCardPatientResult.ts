import PatientData from "@/models/patientData";
import { VaccineDetails } from "@/models/vaccineDetails";

export default interface CovidCardPatientResult {
    // Gets or sets the patient information.
    patient: PatientData;

    // Gets or sets the patient's vaccine details.
    vaccineDetails: VaccineDetails;

    // Gets or sets a value indicating whether the requested record has been protected from being accessed.
    blocked: boolean;
}
