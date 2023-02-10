import { Encounter, HospitalVisit } from "./encounter";
import { ResultError } from "./errors";
import { ImmunizationEvent, Recommendation } from "./immunizationModel";
import { Covid19LaboratoryOrder, LaboratoryOrder } from "./laboratory";
import { LoadStatus } from "./storeOperations";

export interface DatasetState<T> {
    data: T;
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
}

export type Covid19TestResultState = DatasetState<Covid19LaboratoryOrder[]>;
export type HealthVisitState = DatasetState<Encounter[]>;
export interface HospitalVisitState extends DatasetState<HospitalVisit[]> {
    queued: boolean;
}
export interface ImmunizationDatasetState
    extends DatasetState<ImmunizationEvent[]> {
    recommendations: Recommendation[];
}
export interface LabResultState extends DatasetState<LaboratoryOrder[]> {
    queued: boolean;
}
