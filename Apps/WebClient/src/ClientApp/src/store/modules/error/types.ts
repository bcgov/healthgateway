import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import BannerError from "@/models/bannerError";
import { ResultError } from "@/models/requestResult";
import { RootState } from "@/store/types";

export interface ErrorBannerState {
    isShowing: boolean;
    errors: BannerError[];
}

export interface ErrorBannerGetters
    extends GetterTree<ErrorBannerState, RootState> {
    isShowing(state: ErrorBannerState): boolean;
    errors(state: ErrorBannerState): BannerError[];
}

type StoreContext = ActionContext<ErrorBannerState, RootState>;
export interface ErrorBannerActions
    extends ActionTree<ErrorBannerState, RootState> {
    dismiss(context: StoreContext): void;
    show(context: StoreContext): void;
    setError(context: StoreContext, error: BannerError): void;
    addError(context: StoreContext, error: BannerError): void;
    addResultError(
        context: StoreContext,
        param: { message: string; error: ResultError }
    ): void;
}

export interface ErrorBannerMutations extends MutationTree<ErrorBannerState> {
    dismiss(state: ErrorBannerState): void;
    show(state: ErrorBannerState): void;
    setError(state: ErrorBannerState, bannerError: BannerError): void;
    addError(state: ErrorBannerState, bannerError: BannerError): void;
}

export interface ErrorBannerModule extends Module<ErrorBannerState, RootState> {
    namespaced: boolean;
    state: ErrorBannerState;
    getters: ErrorBannerGetters;
    actions: ErrorBannerActions;
    mutations: ErrorBannerMutations;
}
