import { ILogger } from "@/services/interfaces";

export default abstract class PromiseUtility {
    /**
     * Runs a promise without returning it and logs any rejected result.
     * @param promise The promise to run in the background.
     * @param logger The logger used for rejected results.
     * @param operation The operation being performed.
     */
    public static runInBackground<T>(
        promise: Promise<T>,
        logger: ILogger,
        operation: string
    ): void {
        void promise.catch((error: unknown) => {
            logger.warn(`${operation} failed: ${JSON.stringify(error)}`);
        });
    }

    /**
     * Awaits an operation and logs its failure without allowing it to escape.
     * @param promise The operation to await.
     * @param logger The logger used for failures.
     * @param message The error message prefix.
     */
    public static async runAndLogError<T>(
        promise: Promise<T>,
        logger: ILogger,
        message: string
    ): Promise<void> {
        try {
            await promise;
        } catch (error) {
            logger.error(`${message}: ${error}`);
        }
    }

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
