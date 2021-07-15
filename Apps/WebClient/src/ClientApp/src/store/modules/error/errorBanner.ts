import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { ErrorBannerModule, ErrorBannerState } from "./types";

const state: ErrorBannerState = {
    isShowing: false,
    errors: [],
};

const namespaced = true;

export const errorBanner: ErrorBannerModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
