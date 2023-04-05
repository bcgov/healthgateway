export default class PatientData {
    items!: PatientHealthOption[];
}

export enum PatientDataType {
    OrganDonorRegistrationStatus = "OrganDonorRegistrationStatus",
    DiagnosticImaging = "DiagnosticImaging",
}

export enum HealthOptionType {
    OrganDonorRegistration = "OrganDonorRegistration",
    DiagnosticImagingExam = "DiagnosticImagingExam",
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
    public content!: number[];
    public contentType!: string;
}
