export default class PatientData {
    items!: PatientHealthOptions[];
}

export enum PatientDataTypes {
    OrganDonorRegistrationStatus = "OrganDonorRegistrationStatus",
}

export enum HealthOptionTypes {
    OrganDonorRegistrationData = "OrganDonorRegistrationData",
}

export abstract class PatientHealthOptions {
    public type!: HealthOptionTypes;
}

export class OrganDonorRegistrationData extends PatientHealthOptions {
    public status!: string;
    public statusMessage!: string;
    public registrationFileId: string | undefined;
}

export class PatientDataFile {
    public content!: BlobPart[];
    public contentType!: string;
}
