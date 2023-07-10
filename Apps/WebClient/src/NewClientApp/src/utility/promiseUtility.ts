import { Loader } from "@/constants/loader";
import { useLoadingStore } from "@/stores/loading";

export default abstract class PromiseUtility {
    /**
     * Wraps a promise so it won't return or reject until a minimum amount of time has elapsed.
     * @param promise The provided Promise.
     * @param milliseconds The time to wait.
     * @returns A new Promise.
     */
    public static async withMinimumDelay<T>(
        promise: Promise<T>,
        milliseconds: number
    ): Promise<T> {
        // wait for the provided promise and the timeout promise to complete
        const [result] = await Promise.allSettled([
            promise,
            PromiseUtility.delay(milliseconds),
        ]);

        // throw or return the result of the provided promise
        if (result.status === "rejected") {
            throw result.reason;
        } else {
            return result.value;
        }
    }

    /**
     * Returns a promise that resolves after a specified amount of time.
     * @param milliseconds The time to wait.
     * @returns A new Promise.
     */
    public static delay(milliseconds: number): Promise<unknown> {
        return new Promise((resolve) => setTimeout(resolve, milliseconds));
    }

    /**
     * Wraps a promise so a loader is enabled beforehand and disabled afterward.
     * @param promise The provided Promise.
     * @param loader A loader to invoke.
     * @param reason A reason for invoking the loader.
     * @returns A new Promise.
     */
    public static async withLoader<T>(
        promise: Promise<T>,
        loader: Loader,
        reason?: string
    ): Promise<T> {
        const loadingStore = useLoadingStore();

        try {
            loadingStore.setIsLoading(true, loader, reason);
            return await promise;
        } finally {
            loadingStore.setIsLoading(false, loader, reason);
        }
    }
}
