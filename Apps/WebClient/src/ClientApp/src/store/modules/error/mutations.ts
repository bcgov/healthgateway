import BannerError from "@/models/bannerError";

import { ErrorBannerMutations, ErrorBannerState } from "./types";

export const mutations: ErrorBannerMutations = {
    dismiss(state: ErrorBannerState) {
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
