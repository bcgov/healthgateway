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

export const request: Module<MedicationRequestState, RootState> = {
    state,
    getters,
    actions,
    mutations,
};
