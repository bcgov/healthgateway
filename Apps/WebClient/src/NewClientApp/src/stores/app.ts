import { defineStore } from "pinia";
import { ref } from "vue";

import { AppErrorType } from "@/constants/errorType";

export const useAppStore = defineStore("app", () => {
    const appError = ref<AppErrorType>();
    const isMobile = ref(false);
    const isIdle = ref(false);

    function setAppError(errorType: AppErrorType): void {
        appError.value = errorType;
    }

    function setIsMobile(value: boolean) {
        isMobile.value = value;
    }

    function setIsIdle(value: boolean) {
        isIdle.value = value;
    }

    return {
        appError,
        isMobile,
        isIdle,
        setAppError,
        setIsMobile,
        setIsIdle,
    };
});
