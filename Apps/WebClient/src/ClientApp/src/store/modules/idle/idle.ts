import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { IdleModule, IdleState } from "./types";

const state: IdleState = {
    isVisible: false,
};

const namespaced = true;

export const idle: IdleModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
