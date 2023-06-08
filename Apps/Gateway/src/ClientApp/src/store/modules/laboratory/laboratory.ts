import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { LaboratoryModule, LaboratoryState } from "./types";

const state: LaboratoryState = {
    covid19TestResults: {},
    labResults: {},
};

const namespaced = true;

export const laboratory: LaboratoryModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
