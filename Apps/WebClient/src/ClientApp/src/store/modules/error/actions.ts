import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import ErrorTranslator from "@/utility/errorTranslator";

import { ErrorBannerActions } from "./types";

export const actions: ErrorBannerActions = {
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
    clearErrors(context) {
        context.commit("clearErrors");
    },
    setTooManyRequestsWarning(
        context,
        params: {
            key: string;
        }
    ) {
        context.commit("setTooManyRequestsWarning", params.key);
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
