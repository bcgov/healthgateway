import { MutationTree } from "vuex";

import BannerError from "@/models/bannerError";
import { ErrorBannerState } from "@/models/storeState";

export const mutations: MutationTree<ErrorBannerState> = {
    dissmiss(state: ErrorBannerState) {
        state.errors = [];
        state.isShowing = !state.isShowing;
    },
    show(state: ErrorBannerState) {
        state.isShowing = true;
    },
    setError(state: ErrorBannerState, bannerError: BannerError) {
        state.isShowing = true;
        state.errors = [];
        state.errors.push(bannerError);
    },
    addError(state: ErrorBannerState, bannerError: BannerError) {
        state.isShowing = true;
        state.errors.push(bannerError);
    },
};
