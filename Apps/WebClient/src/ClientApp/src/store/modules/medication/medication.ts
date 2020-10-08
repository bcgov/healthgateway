import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { MedicationState, RootState, StateType } from "@/models/storeState";

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
