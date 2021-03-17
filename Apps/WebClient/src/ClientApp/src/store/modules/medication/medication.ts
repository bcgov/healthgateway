import { Module } from "vuex";

import { LoadStatus, MedicationState, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: MedicationState = {
    medicationStatements: [],
    medications: [],
    protectiveWordAttempts: 0,
    medicationRequests: [],
    status: LoadStatus.NONE,
    error: undefined,
    statusMessage: "",
};

const namespaced = true;

export const medication: Module<MedicationState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
