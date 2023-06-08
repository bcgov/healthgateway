import Vue from "vue";

import {
    ClinicalDocument,
    ClinicalDocumentFile,
} from "@/models/clinicalDocument";
import { ClinicalDocumentDatasetState } from "@/models/datasetState";
import EncodedMedia from "@/models/encodedMedia";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";

import { ClinicalDocumentMutations, ClinicalDocumentState } from "./types";
import {
    getClinicalDocumentDatasetState,
    setClinicalDocumentDatasetState,
} from "./util";

export const mutations: ClinicalDocumentMutations = {
    setClinicalDocumentsRequested(state: ClinicalDocumentState, hdid: string) {
        const currentState = getClinicalDocumentDatasetState(state, hdid);
        const nextState: ClinicalDocumentDatasetState = {
            ...currentState,
            status: LoadStatus.REQUESTED,
        };
        setClinicalDocumentDatasetState(state, hdid, nextState);
    },
    setClinicalDocuments(
        state: ClinicalDocumentState,
        payload: { hdid: string; clinicalDocuments: ClinicalDocument[] }
    ) {
        const { hdid, clinicalDocuments } = payload;
        const currentState = getClinicalDocumentDatasetState(state, hdid);
        const nextState: ClinicalDocumentDatasetState = {
            ...currentState,
            data: clinicalDocuments,
            error: undefined,
            statusMessage: "success",
            status: LoadStatus.LOADED,
        };
        setClinicalDocumentDatasetState(state, hdid, nextState);
    },
    setClinicalDocumentsError(
        state: ClinicalDocumentState,
        payload: { hdid: string; error: ResultError }
    ) {
        const { hdid, error } = payload;
        const currentState = getClinicalDocumentDatasetState(state, hdid);
        const nextState: ClinicalDocumentDatasetState = {
            ...currentState,
            error: error,
            statusMessage: error.resultMessage,
            status: LoadStatus.ERROR,
        };
        setClinicalDocumentDatasetState(state, hdid, nextState);
    },
    setFileRequested(state: ClinicalDocumentState, fileId: string) {
        const fileState: ClinicalDocumentFile = {
            fileId,
            status: LoadStatus.REQUESTED,
        };
        Vue.set(state.files, fileId, fileState);
    },
    setFile(
        state: ClinicalDocumentState,
        payload: { fileId: string; file: EncodedMedia }
    ) {
        const fileState: ClinicalDocumentFile = {
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
        const fileState: ClinicalDocumentFile = {
            fileId: payload.fileId,
            file: undefined,
            error: payload.error,
            status: LoadStatus.ERROR,
        };
        Vue.set(state.files, payload.fileId, fileState);
    },
};
