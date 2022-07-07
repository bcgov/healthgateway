import { BannerError } from "@/models/errors";

import { ErrorBannerMutations, ErrorBannerState } from "./types";

export const mutations: ErrorBannerMutations = {
    dismiss(state: ErrorBannerState) {
        state.errors = [];
        state.isShowing = !state.isShowing;
    },
    show(state: ErrorBannerState) {
        state.isShowing = true;
    },
    addError(state: ErrorBannerState, bannerError: BannerError) {
        state.isShowing = true;
        state.errors.push(bannerError);
    },
    clearError(state: ErrorBannerState) {
        state.errors = [];
    },
};
