import { BannerError } from "@/models/errors";

import { ErrorBannerMutations, ErrorBannerState } from "./types";

export const mutations: ErrorBannerMutations = {
    show(state: ErrorBannerState) {
        state.genericErrorBanner.isShowing = true;
    },
    addError(state: ErrorBannerState, bannerError: BannerError) {
        state.genericErrorBanner.isShowing = true;
        state.genericErrorBanner.errors.push(bannerError);
    },
    clearErrors(state: ErrorBannerState) {
        state.genericErrorBanner.errors = [];
        state.genericErrorBanner.isShowing = false;
    },
    setTooManyRequestsWarning(state: ErrorBannerState, key: string) {
        state.tooManyRequestsWarning = key;
    },
    setTooManyRequestsError(state: ErrorBannerState, key: string) {
        state.tooManyRequestsError = key;
    },
    clearTooManyRequestsWarning(state: ErrorBannerState) {
        state.tooManyRequestsWarning = undefined;
    },
    clearTooManyRequestsError(state: ErrorBannerState) {
        state.tooManyRequestsError = undefined;
    },
};
