import { Dictionary } from "@/models/baseTypes";
import {
    ClinicalDocument,
    ClinicalDocumentFile,
} from "@/models/clinicalDocument";
import { LoadStatus } from "@/models/storeOperations";

import { ClinicalDocumentGetters, ClinicalDocumentState } from "./types";
import { getClinicalDocumentDatasetState } from "./util";

export const getters: ClinicalDocumentGetters = {
    clinicalDocuments(
        state: ClinicalDocumentState
    ): (hdid: string) => ClinicalDocument[] {
        return (hdid: string) =>
            getClinicalDocumentDatasetState(state, hdid).data;
    },
    clinicalDocumentsCount(
        state: ClinicalDocumentState
    ): (hdid: string) => number {
        return (hdid: string) =>
            getClinicalDocumentDatasetState(state, hdid).data.length;
    },
    clinicalDocumentsAreLoading(
        state: ClinicalDocumentState
    ): (hdid: string) => boolean {
        return (hdid: string) =>
            getClinicalDocumentDatasetState(state, hdid).status ===
            LoadStatus.REQUESTED;
    },
    files(state: ClinicalDocumentState): Dictionary<ClinicalDocumentFile> {
        return state.files;
    },
};
