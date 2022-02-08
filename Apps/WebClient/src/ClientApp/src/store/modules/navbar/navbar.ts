import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { NavbarModule, NavbarState } from "./types";

const state: NavbarState = {
    isSidebarOpen: false,
    isHeaderShown: true,
    isHeaderButtonShown: true,
    isSidebarButtonShown: true,
};

const namespaced = true;

export const navbar: NavbarModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
