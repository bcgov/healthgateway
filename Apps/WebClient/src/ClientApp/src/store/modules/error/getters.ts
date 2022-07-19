import { BannerError } from "@/models/errors";

import { ErrorBannerGetters, ErrorBannerState } from "./types";

export const getters: ErrorBannerGetters = {
    isShowing(state: ErrorBannerState): boolean {
        return state.genericErrorBanner.isShowing;
    },
    errors(state: ErrorBannerState): BannerError[] {
        return state.genericErrorBanner.errors;
    },
    tooManyRequestsWarning(state: ErrorBannerState): boolean {
        return state.tooManyRequestsWarning;
    },
    tooManyRequestsError(state: ErrorBannerState): string | undefined {
        return state.tooManyRequestsError;
    },
};
