export default class PatientData {
    items!: PatientHealthOption[];
}

export enum PatientDataType {
    OrganDonorRegistrationStatus = "OrganDonorRegistrationStatus",
}

export enum HealthOptionType {
    OrganDonorRegistrationData = "OrganDonorRegistrationData",
}

export abstract class PatientHealthOption {
    public type!: HealthOptionType;
}

export class OrganDonorRegistrationData extends PatientHealthOption {
    public status!: string;
    public statusMessage!: string;
    public registrationFileId: string | undefined;
}

export class PatientDataFile {
    public content!: BlobPart[];
    public contentType!: string;
}
