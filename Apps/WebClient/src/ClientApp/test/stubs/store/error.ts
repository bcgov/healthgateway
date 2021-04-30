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
    setError: voidMethod,
    addError: voidMethod,
    addResultError: voidMethod,
};

const errorBannerMutations: ErrorBannerMutations = {
    dissmiss: voidMethod,
    show: voidMethod,
    setError: voidMethod,
    addError: voidMethod,
};

const errorBannerStub: ErrorBannerModule = {
    namespaced: true,
    state: errorBannerState,
    getters: errorBannerGetters,
    actions: errorBannerActions,
    mutations: errorBannerMutations,
};

export default errorBannerStub;
