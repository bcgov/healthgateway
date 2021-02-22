import { ActionTree } from "vuex";

import { IdleState, RootState } from "@/models/storeState";

export const actions: ActionTree<IdleState, RootState> = {
    setVisibleState(context, isVisible: boolean) {
        context.commit("setVisibleState", isVisible);
    },
};
