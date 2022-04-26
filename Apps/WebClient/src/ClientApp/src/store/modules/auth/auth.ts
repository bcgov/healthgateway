import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { AuthModule, AuthState } from "./types";

export const state: AuthState = {
    tokenDetails: undefined,
    error: undefined,
};

const namespaced = true;

export const auth: AuthModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
