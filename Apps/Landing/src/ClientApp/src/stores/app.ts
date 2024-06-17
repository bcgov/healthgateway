import { defineStore } from "pinia";
import { ref } from "vue";

import { AppErrorType } from "@/constants/errorType";

export const useAppStore = defineStore("app", () => {
    const appError = ref<AppErrorType>();

    function setAppError(errorType: AppErrorType): void {
        appError.value = errorType;
    }

    return {
        appError,
        setAppError,
    };
});
