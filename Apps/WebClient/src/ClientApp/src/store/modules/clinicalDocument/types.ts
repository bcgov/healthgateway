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
    clinicalDocuments(
        state: ClinicalDocumentState
    ): (hdid: string) => ClinicalDocument[];
    clinicalDocumentsCount(
        state: ClinicalDocumentState
    ): (hdid: string) => number;
    clinicalDocumentsAreLoading(
        state: ClinicalDocumentState
    ): (hdid: string) => boolean;
    files(state: ClinicalDocumentState): Dictionary<ClinicalDocumentFile>;
}

type StoreContext = ActionContext<ClinicalDocumentState, RootState>;
export interface ClinicalDocumentActions
    extends ActionTree<ClinicalDocumentState, RootState> {
    retrieveClinicalDocuments(
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
    setClinicalDocumentsRequested(
        state: ClinicalDocumentState,
        hdid: string
    ): void;
    setClinicalDocuments(
        state: ClinicalDocumentState,
        payload: { hdid: string; clinicalDocuments: ClinicalDocument[] }
    ): void;
    setClinicalDocumentsError(
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
