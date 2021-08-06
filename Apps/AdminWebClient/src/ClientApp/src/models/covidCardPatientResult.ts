import { ImmunizationEvent } from "@/models/immunizationModel";
import PatientData from "@/models/patientData";

export default class CovidCardPatientResult {
    // Gets or sets the patient information.
    public patient?: PatientData;

    // Gets or sets the patient covid immunization records.
    public immunizations?: ImmunizationEvent[];
}
