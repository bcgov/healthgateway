import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import ErrorTranslator from "@/utility/errorTranslator";

import { ErrorBannerActions } from "./types";

export const actions: ErrorBannerActions = {
    dismiss(context) {
        context.commit("dismiss");
    },
    show(context) {
        context.commit("show");
    },
    addError(
        context,
        params: {
            errorType: ErrorType;
            source: ErrorSourceType;
            traceId: string | undefined;
        }
    ) {
        const bannerError = ErrorTranslator.toBannerError(
            params.errorType,
            params.source,
            params.traceId
        );
        context.commit("addError", bannerError);
    },
    addCustomError(
        context,
        params: {
            title: string;
            source: ErrorSourceType;
            traceId: string | undefined;
        }
    ) {
        const bannerError = ErrorTranslator.toCustomBannerError(
            params.title,
            params.source,
            params.traceId
        );
        context.commit("addError", bannerError);
    },
    clearError(context) {
        context.commit("clearError");
    },
    setTooManyRequestsWarning(context) {
        context.commit("setTooManyRequestsWarning");
    },
    setTooManyRequestsError(
        context,
        params: {
            key: string;
        }
    ) {
        context.commit("setTooManyRequestsError", params.key);
    },
    clearTooManyRequests(context) {
        context.commit("clearTooManyRequests");
    },
};
