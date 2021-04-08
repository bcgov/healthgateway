import { Module } from "vuex";

import { AuthState, RootState, StateType } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: AuthState = {
    statusMessage: "",
    authentication: undefined,
    error: false,
    stateType: StateType.NONE,
};

const namespaced = true;

export const auth: Module<AuthState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
