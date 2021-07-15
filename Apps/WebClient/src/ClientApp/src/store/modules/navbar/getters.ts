import { NavbarGetters, NavbarState } from "./types";

export const getters: NavbarGetters = {
    isSidebarOpen(state: NavbarState): boolean {
        return state.isSidebarOpen;
    },
    isHeaderShown(state: NavbarState): boolean {
        return state.isHeaderShown;
    },
};
