import BannerError from "@/models/bannerError";
import {
    ErrorBannerActions,
    ErrorBannerGetters,
    ErrorBannerModule,
    ErrorBannerMutations,
    ErrorBannerState,
} from "@/store/modules/error/types";

var errorBannerState: ErrorBannerState = {
    isShowing: false,
    errors: [],
};

var errorBannerGetters: ErrorBannerGetters = {
    isShowing(): boolean {
        return false;
    },
    errors(): BannerError[] {
        return [];
    },
};

var errorBannerActions: ErrorBannerActions = {
    dismiss(): void {},
    show(): void {},
    setError(): void {},
    addError(): void {},
    addResultError(): void {},
};

var errorBannerMutations: ErrorBannerMutations = {
    dissmiss(): void {},
    show(): void {},
    setError(): void {},
    addError(): void {},
};

var errorBannerStub: ErrorBannerModule = {
    namespaced: true,
    state: errorBannerState,
    getters: errorBannerGetters,
    actions: errorBannerActions,
    mutations: errorBannerMutations,
};

export default errorBannerStub;
