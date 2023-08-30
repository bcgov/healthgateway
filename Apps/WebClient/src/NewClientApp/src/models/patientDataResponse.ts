export default class PatientDataResponse {
    items!: PatientData[];
}

export enum PatientDataType {
    OrganDonorRegistrationStatus = "OrganDonorRegistrationStatus",
    DiagnosticImaging = "DiagnosticImaging",
    CancerScreening = "CancerScreening",
}

export enum HealthDataType {
    OrganDonorRegistration = "OrganDonorRegistration",
    DiagnosticImagingExam = "DiagnosticImagingExam",
    CancerScreeningExam = "CancerScreeningExam",
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

PatientDataToHealthDataTypeMap.set(
    PatientDataType.CancerScreening,
    HealthDataType.CancerScreeningExam
);

export abstract class PatientData {
    public id: string | undefined;
    public type!: HealthDataType;
}

export class OrganDonorRegistration extends PatientData {
    public status!: string;
    public statusMessage!: string;
    public registrationFileId: string | undefined;
    public organDonorRegistrationLinkText!: string;
}

export class DiagnosticImagingExam extends PatientData {
    public procedureDescription!: string;
    public bodyPart!: string;
    public modality!: string;
    public organization!: string;
    public healthAuthority!: string;
    public examStatus!: string;
    public fileId: string | undefined;
    public examDate!: string;
    public isUpdated?: boolean;
}

export class CancerScreeningExam extends PatientData {
    public programName!: string;
    public fileId!: string;
    public eventTimestampUtc!: string;
    public resultTimestamp!: string;
}

export class PatientDataFile {
    public content!: number[];
    public contentType!: string;
}
