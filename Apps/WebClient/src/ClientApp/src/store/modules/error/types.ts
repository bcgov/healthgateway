import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import BannerError from "@/models/bannerError";
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
    addError(
        context: StoreContext,
        params: {
            errorType: ErrorType;
            source: string;
            traceId: string | undefined;
        }
    ): void;
    addCustomError(
        context: StoreContext,
        params: {
            title: string;
            source: string;
            traceId: string | undefined;
        }
    ): void;
}

export interface ErrorBannerMutations extends MutationTree<ErrorBannerState> {
    dismiss(state: ErrorBannerState): void;
    show(state: ErrorBannerState): void;
    addError(state: ErrorBannerState, bannerError: BannerError): void;
}

export interface ErrorBannerModule extends Module<ErrorBannerState, RootState> {
    namespaced: boolean;
    state: ErrorBannerState;
    getters: ErrorBannerGetters;
    actions: ErrorBannerActions;
    mutations: ErrorBannerMutations;
}
