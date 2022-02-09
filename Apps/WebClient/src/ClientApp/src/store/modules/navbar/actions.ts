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

    setHeaderButtonState(context, visible: boolean) {
        context.commit("setHeaderButtonState", visible);
    },

    setSidebarButtonState(context, visible: boolean) {
        context.commit("setSidebarButtonState", visible);
    },
};
