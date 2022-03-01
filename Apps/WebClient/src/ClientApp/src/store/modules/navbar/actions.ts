import { NavbarActions } from "./types";

export const actions: NavbarActions = {
    toggleSidebar(context) {
        const isOpen = context.getters["isSidebarOpen"];
        context.commit("setSidebarState", !isOpen);
    },

    setSidebarState(context, isOpen: boolean) {
        context.commit("setSidebarState", isOpen);
    },

    setHeaderState(context, isOpen: boolean) {
        context.commit("setHeaderState", isOpen);
    },
};
