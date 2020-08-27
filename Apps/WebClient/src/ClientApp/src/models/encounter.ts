import Clinic from "@models/clinic";

export default interface Encounter {
    // Gets or sets the id.
    id?: string;

    // Gets or sets the practitioner name.
    practitionerName: string;

    // Gets or sets the specialty description.
    specialtyDescription: string;

    // Gets or sets the encounter timeline datetime.
    serviceDateTime: Date;

    // Gets or sets the clinic.
    clinic: Clinic;
}
