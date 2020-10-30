import { ActionTree } from "vuex";
import { RootState, SidebarState } from "@/models/storeState";

export const actions: ActionTree<SidebarState, RootState> = {
    toggleSidebar(context) {
        context.commit("toggle");
    },

    setSidebarState(context, isOpen: boolean) {
        context.commit("setState", isOpen);
    },
};
