import { BannerError } from "@/models/errors";

import { ErrorBannerMutations, ErrorBannerState } from "./types";

export const mutations: ErrorBannerMutations = {
    dismiss(state: ErrorBannerState) {
        state.genericErrorBanner.errors = [];
        state.genericErrorBanner.isShowing =
            !state.genericErrorBanner.isShowing;
    },
    show(state: ErrorBannerState) {
        state.genericErrorBanner.isShowing = true;
    },
    addError(state: ErrorBannerState, bannerError: BannerError) {
        state.genericErrorBanner.isShowing = true;
        state.genericErrorBanner.errors.push(bannerError);
    },
    clearError(state: ErrorBannerState) {
        state.genericErrorBanner.errors = [];
    },
    setTooManyRequestsWarning(state: ErrorBannerState, key: string) {
        state.tooManyRequestsWarning = key;
    },
    setTooManyRequestsError(state: ErrorBannerState, key: string) {
        state.tooManyRequestsError = key;
    },
    clearTooManyRequests(state: ErrorBannerState) {
        state.tooManyRequestsWarning = undefined;
        state.tooManyRequestsError = undefined;
    },
};
