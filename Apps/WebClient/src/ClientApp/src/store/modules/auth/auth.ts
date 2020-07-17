import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { AuthState, RootState, StateType } from "@/models/storeState";
import AuthenticationData from "@/models/authenticationData";

export const state: AuthState = {
    statusMessage: "",
    authentication: new AuthenticationData(),
    error: undefined,
    isAuthenticated: false,
    stateType: StateType.NONE,
};

const namespaced: boolean = true;

export const auth: Module<AuthState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
