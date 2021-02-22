import { Module } from "vuex";

import { IdleState, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: IdleState = {
    isVisible: false,
};

const namespaced = true;

export const idle: Module<IdleState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
