import { defineStore } from "pinia";
import { ref } from "vue";

import { Loader } from "@/constants/loader";

export const useLoadingStore = defineStore("loading", () => {
    const state = ref(new Map<Loader, Set<string>>());

    function isLoading(location: Loader): boolean {
        return (state.value.get(location)?.size ?? 0) > 0;
    }

    function setIsLoading(
        value: boolean,
        loader: Loader,
        reason = "default"
    ): void {
        const reasons = state.value.get(loader) ?? new Set<string>();
        if (value) {
            reasons.add(reason);
        } else {
            reasons.delete(reason);
        }

        if (reasons.size > 0) {
            state.value.set(loader, reasons);
        } else {
            clearIsLoading(loader);
        }
    }

    function clearIsLoading(location: Loader): void {
        state.value.delete(location);
    }

    function applyLoader<T>(
        loader: Loader,
        reason: string,
        promise: Promise<T>
    ): void {
        setIsLoading(true, loader, reason);
        promise.finally(() => setIsLoading(false, loader, reason));
    }

    return {
        state,
        isLoading,
        setIsLoading,
        clearIsLoading,
        applyLoader,
    };
});
