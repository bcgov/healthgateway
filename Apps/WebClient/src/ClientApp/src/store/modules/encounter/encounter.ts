import { Module } from "vuex";

import { EncounterState, LoadStatus, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: EncounterState = {
    statusMessage: "",
    patientEncounters: [],
    error: undefined,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const encounter: Module<EncounterState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
