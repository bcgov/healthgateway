import { ClinicalDocument } from "@/models/clinicalDocument";
import { Encounter, HospitalVisit } from "@/models/encounter";
import { ResultError } from "@/models/errors";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { Covid19LaboratoryOrder, LaboratoryOrder } from "@/models/laboratory";
import MedicationRequest from "@/models/medicationRequest";
import MedicationStatement from "@/models/medicationStatement";
import {
    PatientData,
    PatientDataFile,
    PatientDataType,
} from "@/models/patientDataResponse";
import { LoadStatus } from "@/models/storeOperations";

export interface DatasetState<T> {
    data: T;
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
}

export type PatientDataMap = Map<PatientDataType, PatientData[]>;

export type ClinicalDocumentDatasetState = DatasetState<ClinicalDocument[]>;
export type Covid19TestResultState = DatasetState<Covid19LaboratoryOrder[]>;
export type HealthVisitState = DatasetState<Encounter[]>;
export type PatientDataState = DatasetState<PatientDataMap>;
export type PatientDataFileState = DatasetState<PatientDataFile | undefined>;
export interface HospitalVisitState extends DatasetState<HospitalVisit[]> {
    queued: boolean;
}
export interface ImmunizationDatasetState extends DatasetState<
    ImmunizationEvent[]
> {
    recommendations: Recommendation[];
}
export interface LabResultState extends DatasetState<LaboratoryOrder[]> {
    queued: boolean;
}
export interface MedicationState extends DatasetState<MedicationStatement[]> {
    protectiveWordAttempts: number;
}
export type SpecialAuthorityRequestState = DatasetState<MedicationRequest[]>;
