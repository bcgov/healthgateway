import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import ClinicalDocument from "@/models/clinicalDocument";
import EncodedMedia from "@/models/encodedMedia";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface ClinicalDocumentState {
    records: ClinicalDocument[];
    error?: ResultError;
    statusMessage: string;
    status: LoadStatus;
    files: Dictionary<ClinicalDocumentFileState>;
}

export interface ClinicalDocumentFileState {
    fileId: string;
    file?: EncodedMedia;
    error?: ResultError;
    status: LoadStatus;
}

export interface ClinicalDocumentGetters
    extends GetterTree<ClinicalDocumentState, RootState> {
    records(state: ClinicalDocumentState): ClinicalDocument[];
    recordCount(state: ClinicalDocumentState): number;
    isLoading(state: ClinicalDocumentState): boolean;
    files(state: ClinicalDocumentState): Dictionary<ClinicalDocumentFileState>;
}

type StoreContext = ActionContext<ClinicalDocumentState, RootState>;
export interface ClinicalDocumentActions
    extends ActionTree<ClinicalDocumentState, RootState> {
    retrieve(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<ClinicalDocument[]>>;
    handleError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface ClinicalDocumentMutations
    extends MutationTree<ClinicalDocumentState> {
    setRequested(state: ClinicalDocumentState): void;
    setRecords(state: ClinicalDocumentState, records: ClinicalDocument[]): void;
    setError(state: ClinicalDocumentState, error: ResultError): void;
    setFileRequested(state: ClinicalDocumentState, fileId: string): void;
    setFile(
        state: ClinicalDocumentState,
        payload: { fileId: string; file: EncodedMedia }
    ): void;
    setFileError(
        state: ClinicalDocumentState,
        payload: { fileId: string; error: ResultError }
    ): void;
}

export interface ClinicalDocumentModule
    extends Module<ClinicalDocumentState, RootState> {
    namespaced: boolean;
    state: ClinicalDocumentState;
    getters: ClinicalDocumentGetters;
    actions: ClinicalDocumentActions;
    mutations: ClinicalDocumentMutations;
}
