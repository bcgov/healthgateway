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
import PatientData, { PatientHealthOptions } from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

/**
 * A wrapping interface which contains the metadata for the requested PatientData
 */
export interface PatientDataRecordState {
    data: PatientData | null;
    status: LoadStatus;
    statusMessage: string;
    error: HttpError | unknown; // TODO: Review this discussion
}

export interface PatientDataState {
    patientDataRecords: Dictionary<PatientDataRecordState>;
}

export interface PatientDataGetters
    extends GetterTree<PatientDataState, RootState> {
    patientData(state: PatientDataState): (hdid: string) => PatientData | null;
    isPatientDataLoading(state: PatientDataState): (hdid: string) => boolean;
}

type StoreContext = ActionContext<PatientDataState, RootState>;

export interface PatientDataActions
    extends ActionTree<PatientDataState, RootState> {
    retrievePatientData(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<PatientData>;

    handleError(
        context: StoreContext,
        params: {
            // TODO: Consider this after testing
            error: HttpError;
            errorType: ErrorType;
            hdid?: string;
            fileId: string;
        }
    ): void;
}

export interface PatientDataMutations extends MutationTree<PatientDataState> {
    setPatientDataRequested(state: PatientDataState, hdid: string): void;

    setPatientData(
        state: PatientDataState,
        payload: { hdid: string; patientData: PatientData }
    ): void;

    setPatientDataError(
        state: PatientDataState,
        // TODO: consider this after testing a failure.
        payload: { hdid: string; error: HttpError | unknown }
    ): void;
}

export interface PatientDataModule extends Module<PatientDataState, RootState> {
    namespaced: boolean;
    state: PatientDataState;
    getters: PatientDataGetters;
    actions: PatientDataActions;
    mutations: PatientDataMutations;
}
