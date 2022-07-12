import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { ErrorBannerModule, ErrorBannerState } from "./types";

const state: ErrorBannerState = {
    genericErrorBanner: { isShowing: false, errors: [] },
    tooManyRequestsWarning: false,
    tooManyRequestsError: undefined,
};

const namespaced = true;

export const errorBanner: ErrorBannerModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
