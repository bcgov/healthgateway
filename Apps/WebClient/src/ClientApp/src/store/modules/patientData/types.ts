import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import { HttpError } from "@/models/errors";
import PatientData, { PatientDataFile } from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

/**
 * A wrapping interface which contains the metadata for the requested PatientData
 */
export interface RecordState<T> {
    data: T | undefined;
    status: LoadStatus;
    statusMessage: string;
    error: HttpError | undefined;
}

export type PatientDataRecordState = RecordState<PatientData>;
export type PatientDataFileState = RecordState<PatientDataFile>;

export interface PatientDataState {
    patientDataRecords: Dictionary<PatientDataRecordState>;
    patientDataFiles: Dictionary<PatientDataFileState>;
}

export interface PatientDataGetters
    extends GetterTree<PatientDataState, RootState> {
    patientData(
        state: PatientDataState
    ): (hdid: string) => PatientData | undefined;
    isPatientDataLoading(state: PatientDataState): (hdid: string) => boolean;
    patientDataFiles(state: PatientDataState): Dictionary<PatientDataFileState>;
}

type StoreContext = ActionContext<PatientDataState, RootState>;

export interface PatientDataActions
    extends ActionTree<PatientDataState, RootState> {
    retrievePatientData(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<PatientData>;

    retrievePatientDataFile(
        context: StoreContext,
        params: { hdid: string; fileId: string }
    ): Promise<PatientDataFile>;

    handleError(
        context: StoreContext,
        params: {
            error: HttpError;
            errorType: ErrorType;
            hdid?: string;
            fileId?: string;
        }
    ): void;
}

export interface PatientDataMutations extends MutationTree<PatientDataState> {
    setPatientDataRequested(state: PatientDataState, hdid: string): void;
    setPatientDataFileRequested(state: PatientDataState, fileId: string): void;

    setPatientData(
        state: PatientDataState,
        payload: { hdid: string; patientData: PatientData }
    ): void;

    setPatientDataFile(
        state: PatientDataState,
        payload: { fileId: string; file: PatientDataFile }
    ): void;
    setPatientDataError(
        state: PatientDataState,
        payload: { hdid: string; error: HttpError }
    ): void;

    setPatientDataFileError(
        state: PatientDataState,
        payload: { fileId: string; error: HttpError }
    ): void;
}

export interface PatientDataModule extends Module<PatientDataState, RootState> {
    namespaced: boolean;
    state: PatientDataState;
    getters: PatientDataGetters;
    actions: PatientDataActions;
    mutations: PatientDataMutations;
}
