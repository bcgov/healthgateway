import { Module } from "vuex";

import { NavbarState, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: NavbarState = {
    isSidebarOpen: false,
    isHeaderShown: true,
};

const namespaced = true;

export const navbar: Module<NavbarState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
