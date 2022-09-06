import { NavbarActions } from "./types";

export const actions: NavbarActions = {
    toggleSidebar(context) {
        const isOpen = context.getters["isSidebarOpen"];
        context.commit("setSidebarState", !isOpen);
    },
    setSidebarStoppedAnimating(context) {
        context.commit("setSidebarStoppedAnimating");
    },
    setHeaderState(context, isOpen: boolean) {
        context.commit("setHeaderState", isOpen);
    },
};
