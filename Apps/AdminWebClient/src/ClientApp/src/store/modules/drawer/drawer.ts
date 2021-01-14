import { Module } from "vuex";

import { DrawerState, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: DrawerState = {
    isOpen: true
};

const namespaced = true;

export const drawer: Module<DrawerState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations
};
