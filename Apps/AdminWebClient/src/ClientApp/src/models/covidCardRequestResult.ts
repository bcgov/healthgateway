import { ImmunizationEvent } from "@/models/immunizationModel";
import PatientData from "@/models/patientData";

export default class CovidCardRequestResult {
    
    // Gets or sets the patient information.
    public patientInfo?: PatientData;

    // Gets or sets the patient covid immunization records.
    public immunizations?: ImmunizationEvent[];
}
