import { ActionTree } from "vuex";

import { NavbarState, RootState } from "@/models/storeState";

export const actions: ActionTree<NavbarState, RootState> = {
    toggleSidebar(context) {
        context.commit("toggleSidebar");
    },

    setSidebarState(context, isOpen: boolean) {
        context.commit("setSidebarState", isOpen);
    },

    toggleHeader(context) {
        context.commit("toggleHeader");
    },

    setHeaderState(context, isOpen: boolean) {
        context.commit("setHeaderState", isOpen);
    },
};
