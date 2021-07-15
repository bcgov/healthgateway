import { NavbarActions } from "./types";

export const actions: NavbarActions = {
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
