import { ActionTree, Commit } from "vuex";

import { RootState, DrawerState } from "@/models/storeState";

export const actions: ActionTree<DrawerState, RootState> = {
  initialize(): boolean {
    console.log("Initializing the config store...");
    return true;
  },

  setState({ commit }, { isDrawerOpen }): void {
    commit("setDrawerState", isDrawerOpen);
  }
};
