import { GetterTree } from "vuex";
import { RootState, SidebarState } from "@/models/storeState";

export const getters: GetterTree<SidebarState, RootState> = {
  isOpen(state: SidebarState): boolean {
    return state.isOpen;
  },
};
