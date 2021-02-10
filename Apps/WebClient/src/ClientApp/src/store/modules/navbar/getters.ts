import { GetterTree } from "vuex";

import { NavbarState, RootState } from "@/models/storeState";

export const getters: GetterTree<NavbarState, RootState> = {
    isSidebarOpen(state: NavbarState): boolean {
        return state.isSidebarOpen;
    },
    isHeaderShown(state: NavbarState): boolean {
        return state.isHeaderShown;
    },
};
