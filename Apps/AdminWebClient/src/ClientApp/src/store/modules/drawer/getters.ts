import { GetterTree } from "vuex";

import { DrawerState, RootState } from "@/models/storeState";

export const getters: GetterTree<DrawerState, RootState> = {
    isOpen(state: DrawerState): boolean {
        const { isOpen } = state;
        return isOpen;
    },
};
