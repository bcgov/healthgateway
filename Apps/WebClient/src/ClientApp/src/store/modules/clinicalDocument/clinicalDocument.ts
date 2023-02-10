import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { ClinicalDocumentModule, ClinicalDocumentState } from "./types";

const state: ClinicalDocumentState = {
    clinicalDocuments: {},
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
