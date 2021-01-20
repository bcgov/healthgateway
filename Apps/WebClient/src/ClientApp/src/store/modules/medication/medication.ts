import { Module } from "vuex";

import { MedicationState, RootState, StateType } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: MedicationState = {
    statusMessage: "",
    medicationStatements: [],
    medications: [],
    error: false,
    stateType: StateType.NONE,
};

const namespaced = true;

export const medication: Module<MedicationState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
