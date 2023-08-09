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
}
