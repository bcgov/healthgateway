import { HospitalVisit } from "./encounter";
import { ResultError } from "./errors";
import { LoadStatus } from "./storeOperations";

export interface DatasetState<T> {
    data: T;
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
}

export interface HospitalVisitState extends DatasetState<HospitalVisit[]> {
    queued: boolean;
}
