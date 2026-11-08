import type { ResultError } from "./errors";

/** Port of models/storeOperations.ts LoadStatus */
export enum LoadStatus {
    NONE,
    REQUESTED,
    ASYNC_REQUESTED,
    PARTIALLY_LOADED,
    LOADED,
    DEFERRED,
    PROTECTED,
    ERROR,
}

/** Port of models/datasetState.ts DatasetState */
export interface DatasetState<T> {
    data: T;
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
}
