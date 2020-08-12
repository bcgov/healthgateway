import { MutationTree } from "vuex";
import { ErrorBannerState } from "@/models/storeState";
import BannerError from "@/models/bannerError";

export const mutations: MutationTree<ErrorBannerState> = {
    dissmiss(state: ErrorBannerState) {
        console.log("ErrorBannerState:dissmiss");
        state.errors = [];
        state.isShowing = !state.isShowing;
    },
    show(state: ErrorBannerState) {
        console.log("ErrorBannerState:show");
        state.isShowing = true;
    },
    setError(state: ErrorBannerState, bannerError: BannerError) {
        console.log("ErrorBannerState:setError");
        state.isShowing = true;
        state.errors = [];
        state.errors.push(bannerError);
    },
    addError(state: ErrorBannerState, bannerError: BannerError) {
        console.log("ErrorBannerState:addError");
        state.isShowing = true;
        state.errors.push(bannerError);
    },
};
