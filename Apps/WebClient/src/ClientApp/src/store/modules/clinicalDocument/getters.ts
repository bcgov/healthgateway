import { Dictionary } from "@/models/baseTypes";
import ClinicalDocument from "@/models/clinicalDocument";
import { LoadStatus } from "@/models/storeOperations";

import {
    ClinicalDocumentFileState,
    ClinicalDocumentGetters,
    ClinicalDocumentState,
} from "./types";

export const getters: ClinicalDocumentGetters = {
    records(state: ClinicalDocumentState): ClinicalDocument[] {
        return state.records;
    },
    recordCount(state: ClinicalDocumentState): number {
        return state.records.length;
    },
    isLoading(state: ClinicalDocumentState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
    files(state: ClinicalDocumentState): Dictionary<ClinicalDocumentFileState> {
        return state.files;
    },
};
