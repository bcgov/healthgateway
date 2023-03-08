import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { EncounterModule, EncounterState } from "./types";

const state: EncounterState = {
    healthVisits: {},
    hospitalVisits: {},
};

const namespaced = true;

export const encounter: EncounterModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
