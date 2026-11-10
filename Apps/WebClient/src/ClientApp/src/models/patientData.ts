/** Port of models/patientDataResponse.ts */
export enum PatientDataType {
    OrganDonorRegistrationStatus = "OrganDonorRegistrationStatus",
    DiagnosticImaging = "DiagnosticImaging",
    BcCancerScreening = "BcCancerScreening",
}

export enum HealthDataType {
    OrganDonorRegistration = "OrganDonorRegistration",
    DiagnosticImagingExam = "DiagnosticImagingExam",
    BcCancerScreening = "BcCancerScreening",
}

export const PatientDataToHealthDataTypeMap = new Map<
    PatientDataType,
    HealthDataType
>([
    [
        PatientDataType.OrganDonorRegistrationStatus,
        HealthDataType.OrganDonorRegistration,
    ],
    [PatientDataType.DiagnosticImaging, HealthDataType.DiagnosticImagingExam],
    [PatientDataType.BcCancerScreening, HealthDataType.BcCancerScreening],
]);

export interface PatientData {
    id: string | undefined;
    type: HealthDataType;
}

export interface OrganDonorRegistration extends PatientData {
    status: string;
    statusMessage: string;
    registrationFileId: string | undefined;
    organDonorRegistrationLinkText: string;
}

export interface DiagnosticImagingExam extends PatientData {
    procedureDescription: string;
    bodyPart: string;
    modality: string;
    organization: string;
    healthAuthority: string;
    examStatus: string;
    fileId: string | undefined;
    examDate: string;
    isUpdated?: boolean;
}

export enum BcCancerScreeningType {
    Unknown = "Unknown",
    Recall = "Recall",
    Result = "Result",
}

export interface BcCancerScreening extends PatientData {
    eventType: BcCancerScreeningType;
    programName: string;
    fileId: string;
    eventDateTime: string;
    resultDateTime: string;
}

export interface PatientDataResponse {
    items: PatientData[];
}

export interface PatientDataFile {
    content: number[];
    contentType: string;
}
