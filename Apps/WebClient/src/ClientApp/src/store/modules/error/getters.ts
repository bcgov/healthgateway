import { BannerError } from "@/models/errors";

import { ErrorBannerGetters, ErrorBannerState } from "./types";

export const getters: ErrorBannerGetters = {
    isShowing(state: ErrorBannerState): boolean {
        return state.isShowing;
    },
    errors(state: ErrorBannerState): BannerError[] {
        return state.errors;
    },
};
