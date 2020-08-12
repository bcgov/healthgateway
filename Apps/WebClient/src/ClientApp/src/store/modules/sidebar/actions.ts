import { ActionTree } from "vuex";
import { RootState, SidebarState } from "@/models/storeState";

export const actions: ActionTree<SidebarState, RootState> = {
    toggleSidebar(context) {
        context.commit("toggle");
    },
};
