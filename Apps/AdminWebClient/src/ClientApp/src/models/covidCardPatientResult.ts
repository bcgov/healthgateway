import { ImmunizationEvent } from "@/models/immunizationModel";
import PatientData from "@/models/patientData";

export default interface CovidCardPatientResult {
    // Gets or sets the patient information.
    patient: PatientData;

    // Gets or sets the patient covid immunization records.
    immunizations: ImmunizationEvent[];
}
