import { MutationTree } from "vuex";
import { DrawerState } from "@/models/storeState";

export const mutations: MutationTree<DrawerState> = {
    setDrawerState(state: DrawerState, openState: boolean) {
        state.isOpen = openState;
    }
};
