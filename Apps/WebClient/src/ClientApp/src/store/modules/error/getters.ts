import { GetterTree } from "vuex";
import { RootState, ErrorBannerState } from "@/models/storeState";
import BannerError from "@/models/bannerError";

export const getters: GetterTree<ErrorBannerState, RootState> = {
    isShowing(state: ErrorBannerState): boolean {
        return state.isShowing;
    },
    errors(state: ErrorBannerState): BannerError[] {
        return state.errors;
    },
};
