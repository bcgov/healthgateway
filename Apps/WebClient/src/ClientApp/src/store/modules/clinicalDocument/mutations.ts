import Vue from "vue";

import ClinicalDocument from "@/models/clinicalDocument";
import EncodedMedia from "@/models/encodedMedia";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";

import {
    ClinicalDocumentFileState,
    ClinicalDocumentMutations,
    ClinicalDocumentState,
} from "./types";

export const mutations: ClinicalDocumentMutations = {
    setRequested(state: ClinicalDocumentState) {
        state.status = LoadStatus.REQUESTED;
    },
    setRecords(state: ClinicalDocumentState, records: ClinicalDocument[]) {
        state.records = records;
        state.error = undefined;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    setError(state: ClinicalDocumentState, error: ResultError) {
        state.error = error;
        state.statusMessage = error.resultMessage;
        state.status = LoadStatus.ERROR;
    },
    setFileRequested(state: ClinicalDocumentState, fileId: string) {
        const fileState: ClinicalDocumentFileState = {
            fileId,
            status: LoadStatus.REQUESTED,
        };
        Vue.set(state.files, fileId, fileState);
    },
    setFile(
        state: ClinicalDocumentState,
        payload: { fileId: string; file: EncodedMedia }
    ) {
        const fileState: ClinicalDocumentFileState = {
            fileId: payload.fileId,
            file: payload.file,
            error: undefined,
            status: LoadStatus.LOADED,
        };
        Vue.set(state.files, payload.fileId, fileState);
    },
    setFileError(
        state: ClinicalDocumentState,
        payload: { fileId: string; error: ResultError }
    ) {
        const fileState: ClinicalDocumentFileState = {
            fileId: payload.fileId,
            file: undefined,
            error: payload.error,
            status: LoadStatus.ERROR,
        };
        Vue.set(state.files, payload.fileId, fileState);
    },
};
