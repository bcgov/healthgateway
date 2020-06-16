import { ActionTree } from "vuex";
import { RootState, SidebarState } from "@/models/storeState";

export const actions: ActionTree<SidebarState, RootState> = {
  toggleSidebar(context) {
    return new Promise((resolve) => {
      context.commit("toggle");
      resolve();
    });
  },
};
