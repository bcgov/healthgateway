import PatientData from "@/models/patientData";
import { VaccineDetails } from "@/models/vaccineDetails";

export default interface CovidCardPatientResult {
    // Gets or sets the patient information.
    patient: PatientData;

    // Gets or sets the patient's vaccine details.
    vaccineDetails: VaccineDetails;
}
