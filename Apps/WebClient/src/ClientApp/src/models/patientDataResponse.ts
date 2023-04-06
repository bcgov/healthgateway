export default class PatientDataResponse {
    items!: PatientData[];
}

export enum PatientDataType {
    OrganDonorRegistrationStatus = "OrganDonorRegistrationStatus",
    DiagnosticImaging = "DiagnosticImaging",
}

export enum HealthDataType {
    OrganDonorRegistration = "OrganDonorRegistration",
    DiagnosticImagingExam = "DiagnosticImagingExam",
}

export const PatientDataToHealthDataTypeMap: Map<
    PatientDataType,
    HealthDataType
> = new Map<PatientDataType, HealthDataType>();

PatientDataToHealthDataTypeMap.set(
    PatientDataType.OrganDonorRegistrationStatus,
    HealthDataType.OrganDonorRegistration
);

PatientDataToHealthDataTypeMap.set(
    PatientDataType.DiagnosticImaging,
    HealthDataType.DiagnosticImagingExam
);

export abstract class PatientData {
    public type!: HealthDataType;
}

export class OrganDonorRegistration extends PatientData {
    public status!: string;
    public statusMessage!: string;
    public registrationFileId: string | undefined;
}

export class PatientDataFile {
    public content!: number[];
    public contentType!: string;
}
