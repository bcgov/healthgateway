import { Module } from "vuex";

import { ImmunizationState, RootState, StateType } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: ImmunizationState = {
    statusMessage: "",
    immunizations: [],
    recommendations: [],
    error: false,
    stateType: StateType.NONE,
};

const namespaced = true;

export const immunization: Module<ImmunizationState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
