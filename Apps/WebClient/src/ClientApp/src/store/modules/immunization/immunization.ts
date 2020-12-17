import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { ImmunizationState, RootState, StateType } from "@/models/storeState";

export const state: ImmunizationState = {
    statusMessage: "",
    immunizations: [],
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
