import BannerError from "@/models/bannerError";
import { ResultError } from "@/models/requestResult";
import ErrorTranslator from "@/utility/errorTranslator";

import { ErrorBannerActions } from "./types";

export const actions: ErrorBannerActions = {
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
    addResultError(context, param: { message: string; error: ResultError }) {
        const bannerError = ErrorTranslator.toBannerError(
            param.message,
            param.error
        );
        context.commit("addError", bannerError);
    },
};
