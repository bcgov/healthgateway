import { Module } from "vuex";

import { RootState, SidebarState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: SidebarState = {
    isOpen: false,
};

const namespaced = true;

export const sidebar: Module<SidebarState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
