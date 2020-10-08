import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { AuthState, RootState, StateType } from "@/models/storeState";

export const state: AuthState = {
    statusMessage: "",
    authentication: { isChecked: false, identityProvider: "" },
    error: undefined,
    isAuthenticated: false,
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
