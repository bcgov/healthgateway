import { Module } from "vuex";

import { ErrorBannerState, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: ErrorBannerState = {
    isShowing: false,
    errors: [],
};

const namespaced = true;

export const errorBanner: Module<ErrorBannerState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
