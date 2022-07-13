import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { BannerError } from "@/models/errors";
import { RootState } from "@/store/types";

export interface ErrorBannerState {
    genericErrorBanner: {
        isShowing: boolean;
        errors: BannerError[];
    };
    tooManyRequestsWarning?: string;
    tooManyRequestsError?: string;
}

export interface ErrorBannerGetters
    extends GetterTree<ErrorBannerState, RootState> {
    isShowing(state: ErrorBannerState): boolean;
    errors(state: ErrorBannerState): BannerError[];
    tooManyRequestsWarning(state: ErrorBannerState): string | undefined;
    tooManyRequestsError(state: ErrorBannerState): string | undefined;
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
    clearError(context: StoreContext): void;
    setTooManyRequestsWarning(
        context: StoreContext,
        params: {
            key: string;
        }
    ): void;
    setTooManyRequestsError(
        context: StoreContext,
        params: {
            key: string;
        }
    ): void;
    clearTooManyRequests(context: StoreContext): void;
}

export interface ErrorBannerMutations extends MutationTree<ErrorBannerState> {
    dismiss(state: ErrorBannerState): void;
    show(state: ErrorBannerState): void;
    addError(state: ErrorBannerState, bannerError: BannerError): void;
    clearError(state: ErrorBannerState): void;
    setTooManyRequestsWarning(state: ErrorBannerState, key: string): void;
    setTooManyRequestsError(state: ErrorBannerState, key: string): void;
    clearTooManyRequests(state: ErrorBannerState): void;
}

export interface ErrorBannerModule extends Module<ErrorBannerState, RootState> {
    namespaced: boolean;
    state: ErrorBannerState;
    getters: ErrorBannerGetters;
    actions: ErrorBannerActions;
    mutations: ErrorBannerMutations;
}
