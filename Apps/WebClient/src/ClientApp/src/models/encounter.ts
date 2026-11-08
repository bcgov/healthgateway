/** Port of models/encounter.ts */
export interface Encounter {
    id: string;
    practitionerName: string;
    specialtyDescription: string;
    /** ISO date */
    encounterDate: string;
    clinic: Clinic;
}

export interface Clinic {
    clinicId: string;
    name: string;
}

export interface HospitalVisit {
    encounterId: string;
    facility: string;
    healthService: string;
    visitType: string;
    healthAuthority: string;
    admitDateTime: string;
    endDateTime: string;
    provider: string;
}
