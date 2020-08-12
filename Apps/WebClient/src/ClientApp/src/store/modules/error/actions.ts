import { ActionTree } from "vuex";
import { RootState, ErrorBannerState } from "@/models/storeState";
import BannerError from "@/models/bannerError";

export const actions: ActionTree<ErrorBannerState, RootState> = {
    dismiss(context) {
        context.commit("dismiss");
    },
    show(context) {
        context.commit("show");
    },
    setError(context, error: BannerError) {
        context.commit("setError", error);
    },
    addError(context, error: BannerError) {
        context.commit("addError", error);
    },
};
