import BannerError from "@/models/bannerError";
import {
    ErrorBannerActions,
    ErrorBannerGetters,
    ErrorBannerModule,
    ErrorBannerMutations,
    ErrorBannerState,
} from "@/store/modules/error/types";

import { stubbedVoid } from "../../utility/stubUtil";

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
    dismiss: stubbedVoid,
    show: stubbedVoid,
    setError: stubbedVoid,
    addError: stubbedVoid,
    addResultError: stubbedVoid,
};

const errorBannerMutations: ErrorBannerMutations = {
    dissmiss: stubbedVoid,
    show: stubbedVoid,
    setError: stubbedVoid,
    addError: stubbedVoid,
};

const errorBannerStub: ErrorBannerModule = {
    namespaced: true,
    state: errorBannerState,
    getters: errorBannerGetters,
    actions: errorBannerActions,
    mutations: errorBannerMutations,
};

export default errorBannerStub;
