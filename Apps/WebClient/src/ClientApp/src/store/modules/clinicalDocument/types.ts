import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import {
    ClinicalDocument,
    ClinicalDocumentFile,
} from "@/models/clinicalDocument";
import { ClinicalDocumentDatasetState } from "@/models/datasetState";
import EncodedMedia from "@/models/encodedMedia";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { RootState } from "@/store/types";

export interface ClinicalDocumentState {
    clinicalDocuments: Dictionary<ClinicalDocumentDatasetState>;
    files: Dictionary<ClinicalDocumentFile>;
}

export interface ClinicalDocumentGetters
    extends GetterTree<ClinicalDocumentState, RootState> {
    records(state: ClinicalDocumentState): (hdid: string) => ClinicalDocument[];
    recordCount(state: ClinicalDocumentState): (hdid: string) => number;
    isLoading(state: ClinicalDocumentState): (hdid: string) => boolean;
    files(state: ClinicalDocumentState): Dictionary<ClinicalDocumentFile>;
}

type StoreContext = ActionContext<ClinicalDocumentState, RootState>;
export interface ClinicalDocumentActions
    extends ActionTree<ClinicalDocumentState, RootState> {
    retrieve(
        context: StoreContext,
        params: { hdid: string }
    ): Promise<RequestResult<ClinicalDocument[]>>;
    getFile(
        context: StoreContext,
        params: { fileId: string; hdid: string }
    ): Promise<EncodedMedia>;
    handleError(
        context: StoreContext,
        params: {
            error: ResultError;
            errorType: ErrorType;
            hdid?: string;
            fileId?: string;
        }
    ): void;
}

export interface ClinicalDocumentMutations
    extends MutationTree<ClinicalDocumentState> {
    setRequested(state: ClinicalDocumentState, hdid: string): void;
    setRecords(
        state: ClinicalDocumentState,
        payload: { hdid: string; records: ClinicalDocument[] }
    ): void;
    setError(
        state: ClinicalDocumentState,
        payload: { hdid: string; error: ResultError }
    ): void;
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
