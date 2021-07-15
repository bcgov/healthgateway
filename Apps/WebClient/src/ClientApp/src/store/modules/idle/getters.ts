import { IdleGetters, IdleState } from "./types";

export const getters: IdleGetters = {
    isVisible(state: IdleState): boolean {
        return state.isVisible;
    },
};
