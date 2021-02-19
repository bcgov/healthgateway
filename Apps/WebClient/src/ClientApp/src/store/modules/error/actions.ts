import { ActionTree } from "vuex";

import BannerError from "@/models/bannerError";
import { ErrorBannerState, RootState } from "@/models/storeState";

export const actions: ActionTree<ErrorBannerState, RootState> = {
    dismiss(context) {
        context.commit("dismiss");
    },
    show(context) {
        context.commit("show");
    },
    setBannerError(context, error: BannerError) {
        context.commit("setError", error);
    },
    addBannerError(context, error: BannerError) {
        context.commit("addError", error);
    },
};
