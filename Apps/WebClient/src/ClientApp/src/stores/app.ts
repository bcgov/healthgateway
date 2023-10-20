import { defineStore } from "pinia";
import { computed, ref } from "vue";
import { useDisplay } from "vuetify";

import { AppErrorType } from "@/constants/errorType";
import { DateWrapper } from "@/models/dateWrapper";

export const useAppStore = defineStore("app", () => {
    const display = useDisplay();

    const appError = ref<AppErrorType>();
    const isIdle = ref(false);

    const isMobile = computed(() => !display.mdAndUp.value);
    const isPacificTime = computed(
        () =>
            DateWrapper.now().offset() === DateWrapper.now().toLocal().offset()
    );

    function setAppError(errorType: AppErrorType): void {
        appError.value = errorType;
    }

    function setIsIdle(value: boolean) {
        isIdle.value = value;
    }

    return {
        appError,
        isMobile,
        isIdle,
        isPacificTime,
        setAppError,
        setIsIdle,
    };
});
