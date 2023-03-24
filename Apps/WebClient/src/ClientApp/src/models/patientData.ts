export default class PatientData {
    items!: PatientHealthOption[];
}

export enum HealthOptionsType {
    OrganDonorRegistrationStatus = "OrganDonorRegistrationStatus",
}

export enum HealthOptionResponseType {
    OrganDonorRegistrationData = "OrganDonorRegistrationData",
}

export abstract class PatientHealthOption {
    public type!: HealthOptionResponseType;
}

export class OrganDonorRegistrationData extends PatientHealthOption {
    public status!: string;
    public statusMessage!: string;
    public registrationFileId: string | undefined;
}

export class PatientDataFile {
    public content!: number[];
    public contentType!: string;
}
