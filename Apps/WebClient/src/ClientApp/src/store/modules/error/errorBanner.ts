import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { RootState, ErrorBannerState } from "@/models/storeState";

export const state: ErrorBannerState = {
    isShowing: false,
    errors: [],
};

const namespaced: boolean = true;

export const errorBanner: Module<ErrorBannerState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
