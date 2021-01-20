import { GetterTree } from "vuex";

import BannerError from "@/models/bannerError";
import { ErrorBannerState, RootState } from "@/models/storeState";

export const getters: GetterTree<ErrorBannerState, RootState> = {
    isShowing(state: ErrorBannerState): boolean {
        return state.isShowing;
    },
    errors(state: ErrorBannerState): BannerError[] {
        return state.errors;
    },
};
