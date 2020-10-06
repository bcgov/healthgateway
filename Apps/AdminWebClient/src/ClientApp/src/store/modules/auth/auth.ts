import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { RootState, AuthState, StateType } from "@/models/storeState";

export const state: AuthState = {
    statusMessage: "",
    authentication: undefined,
    error: false,
    stateType: StateType.NONE
};

const namespaced = true;

export const auth: Module<AuthState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations
};
