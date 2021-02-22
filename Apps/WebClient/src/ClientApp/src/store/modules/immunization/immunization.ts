import { Module } from "vuex";

import { ImmunizationState, LoadStatus, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: ImmunizationState = {
    statusMessage: "",
    immunizations: [],
    recommendations: [],
    error: undefined,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const immunization: Module<ImmunizationState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
