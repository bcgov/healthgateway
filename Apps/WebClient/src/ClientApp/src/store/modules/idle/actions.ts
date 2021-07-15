import { IdleActions } from "./types";

export const actions: IdleActions = {
    setVisibleState(context, isVisible: boolean) {
        context.commit("setVisibleState", isVisible);
    },
};
