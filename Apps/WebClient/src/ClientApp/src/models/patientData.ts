export default class PatientData {
    items: PatientHealthOptions[] | undefined;
}

export abstract class PatientHealthOptions {
    public type!: string;
}

export class OrganDonorRegistrationData extends PatientHealthOptions {
    public status!: string;
    public statusMessage!: string;
    public registrationFileId: string | undefined;
}

export class PatientDataFile {
    public content!: Blob;
    public contentType!: string;
}
