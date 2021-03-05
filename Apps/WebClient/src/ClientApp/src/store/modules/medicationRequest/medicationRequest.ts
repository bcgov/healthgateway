import { Module } from "vuex";

import {
    LoadStatus,
    MedicationRequestState,
    RootState,
} from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: MedicationRequestState = {
    medicationRequests: [],
    status: LoadStatus.NONE,
    error: undefined,
    statusMessage: "",
};

const namespaced = true;

export const medicationRequest: Module<MedicationRequestState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
