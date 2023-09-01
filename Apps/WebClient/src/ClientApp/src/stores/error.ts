import { defineStore } from "pinia";
import { ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { BannerError } from "@/models/errors";
import ErrorTranslator from "@/utility/errorTranslator";

export const useErrorStore = defineStore("error", () => {
    const genericErrorBanner = ref<{
        isShowing: boolean;
        errors: BannerError[];
    }>({ isShowing: false, errors: [] });
    const tooManyRequestsWarning = ref<string>();
    const tooManyRequestsError = ref<string>();

    function show() {
        genericErrorBanner.value.isShowing = true;
    }

    function addError(
        errorType: ErrorType,
        source: ErrorSourceType,
        traceId: string | undefined
    ) {
        const bannerError = ErrorTranslator.toBannerError(
            errorType,
            source,
            traceId
        );
        genericErrorBanner.value.isShowing = true;
        genericErrorBanner.value.errors.push(bannerError);
    }

    function addCustomError(
        title: string,
        source: ErrorSourceType,
        traceId: string | undefined
    ) {
        const bannerError = ErrorTranslator.toCustomBannerError(
            title,
            source,
            traceId
        );
        genericErrorBanner.value.isShowing = true;
        genericErrorBanner.value.errors.push(bannerError);
    }

    function clearErrors() {
        genericErrorBanner.value.errors = [];
        genericErrorBanner.value.isShowing = false;
    }

    function setTooManyRequestsWarning(key: string) {
        tooManyRequestsWarning.value = key;
    }

    function setTooManyRequestsError(key: string) {
        tooManyRequestsError.value = key;
    }

    function clearTooManyRequestsWarning() {
        tooManyRequestsWarning.value = undefined;
    }

    function clearTooManyRequestsError() {
        tooManyRequestsError.value = undefined;
    }

    return {
        genericErrorBanner,
        tooManyRequestsWarning,
        tooManyRequestsError,
        show,
        addError,
        addCustomError,
        clearErrors,
        setTooManyRequestsWarning,
        setTooManyRequestsError,
        clearTooManyRequestsWarning,
        clearTooManyRequestsError,
    };
});
