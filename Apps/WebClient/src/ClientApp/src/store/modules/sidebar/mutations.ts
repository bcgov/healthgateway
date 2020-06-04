import { MutationTree } from "vuex";
import { SidebarState } from "@/models/storeState";

export const mutations: MutationTree<SidebarState> = {
  toggle(state: SidebarState) {
    console.log("SidebarState:toggle");
    state.isOpen = !state.isOpen;
  },
  setState(state: SidebarState, isOpen: boolean) {
    console.log("SidebarState:setState");
    state.isOpen = isOpen;
  },
};
