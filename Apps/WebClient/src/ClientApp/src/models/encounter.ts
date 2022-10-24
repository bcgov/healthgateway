import Clinic from "@/models/clinic";
import { StringISODate } from "@/models/dateWrapper";

export interface Encounter {
    // Gets or sets the id.
    id: string;

    // Gets or sets the practitioner name.
    practitionerName: string;

    // Gets or sets the specialty description.
    specialtyDescription: string;

    // Gets or sets the encounter timeline datetime.
    encounterDate: StringISODate;

    // Gets or sets the clinic.
    clinic: Clinic;
}

export interface HospitalVisit {
    // Gets or sets the encounter id.
    encounterId: string;

    // Gets or sets the facility.
    facility?: string;

    // Gets or sets the health service.
    healthService?: string;

    // Gets or sets the visit type.
    visitType?: string;

    // Gets or sets the health authority.
    healthAuthority?: string;

    // Gets or sets the admit date time.
    admitDateTime?: StringISODate;

    // Gets or sets the end date time.
    endDateTime?: StringISODate;

    // Gets or sets the provider.
    provider: string[];
}
