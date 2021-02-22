import { GetterTree } from "vuex";

import { IdleState, RootState } from "@/models/storeState";

export const getters: GetterTree<IdleState, RootState> = {
    isVisible(state: IdleState): boolean {
        return state.isVisible;
    },
};
