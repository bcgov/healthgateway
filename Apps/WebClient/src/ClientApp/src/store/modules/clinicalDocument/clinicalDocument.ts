import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { ClinicalDocumentModule, ClinicalDocumentState } from "./types";

const state: ClinicalDocumentState = {
    records: [],
    error: undefined,
    statusMessage: "",
    status: LoadStatus.NONE,
    files: {},
};

const namespaced = true;

export const clinicalDocument: ClinicalDocumentModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
