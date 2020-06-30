import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { DrawerState, RootState } from "@/models/storeState";

export const state: DrawerState = {
    isOpen: true
};

const namespaced: boolean = true;

export const drawer: Module<DrawerState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations
};
