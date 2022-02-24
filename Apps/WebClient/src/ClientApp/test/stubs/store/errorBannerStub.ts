import { voidMethod } from "@test/stubs/util";

import BannerError from "@/models/bannerError";
import {
    ErrorBannerActions,
    ErrorBannerGetters,
    ErrorBannerModule,
    ErrorBannerMutations,
    ErrorBannerState,
} from "@/store/modules/error/types";

const errorBannerState: ErrorBannerState = {
    isShowing: false,
    errors: [],
};

const errorBannerGetters: ErrorBannerGetters = {
    isShowing(): boolean {
        return false;
    },
    errors(): BannerError[] {
        return [];
    },
};

const errorBannerActions: ErrorBannerActions = {
    dismiss: voidMethod,
    show: voidMethod,
    addError: voidMethod,
    addCustomError: voidMethod,
    clearError: voidMethod,
};

const errorBannerMutations: ErrorBannerMutations = {
    dismiss: voidMethod,
    show: voidMethod,
    addError: voidMethod,
    clearError: voidMethod,
};

const errorBannerStub: ErrorBannerModule = {
    namespaced: true,
    state: errorBannerState,
    getters: errorBannerGetters,
    actions: errorBannerActions,
    mutations: errorBannerMutations,
};

export default errorBannerStub;
