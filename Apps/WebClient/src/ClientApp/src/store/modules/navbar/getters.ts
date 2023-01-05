import { RootState } from "@/store/types";

import { NavbarGetters, NavbarState } from "./types";

export const getters: NavbarGetters = {
    isHeaderShown(state: NavbarState): boolean {
        return state.isHeaderShown;
    },
    isSidebarOpen(
        state: NavbarState,
        // eslint-disable-next-line
        _getters: any,
        rootState: RootState
    ): boolean {
        // initial sidebar state depends on viewport width
        if (state.isSidebarOpen === null) {
            return rootState.isMobile ? false : true;
        }
        return state.isSidebarOpen;
    },
    isSidebarAnimating(state: NavbarState): boolean {
        return state.isSidebarAnimating;
    },
};
