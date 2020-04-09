import { ActionTree } from "vuex";
import { RootState, SidebarState } from "@/models/storeState";

export const actions: ActionTree<SidebarState, RootState> = {
  toggleSidebar({ commit }) {
    return new Promise(resolve => {
      commit("toggle");
      resolve();
    });
  }
};
