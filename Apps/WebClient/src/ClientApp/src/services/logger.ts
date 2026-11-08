import { isSessionEnded } from "@/models/errors";

export type LogLevel =
    "verbose" | "debug" | "info" | "warn" | "error" | "silent";

const order: Record<LogLevel, number> = {
    verbose: 0,
    debug: 1,
    info: 2,
    warn: 3,
    error: 4,
    silent: 5,
};

export interface ILogger {
    setLevel(level: LogLevel): void;
    verbose(message: string): void;
    debug(message: string): void;
    info(message: string): void;
    warn(message: string): void;
    error(message: string, error?: unknown): void;
    /** While the network or service is down, failures are expected and the banner explains them: log them quietly. */
    setFailuresExpected(expected: boolean): void;
}

/** Maps the server's webClient.logLevel ("Debug", "Information", …) to a client level. */
export function logLevelFromConfig(value?: string): LogLevel {
    switch ((value ?? "").toLowerCase()) {
        case "verbose":
        case "trace":
            return "verbose";
        case "debug":
            return "debug";
        case "information":
        case "info":
            return "info";
        case "warning":
        case "warn":
            return "warn";
        case "error":
        case "critical":
            return "error";
        case "none":
            return "silent";
        default:
            return __DEV__ ? "debug" : "warn";
    }
}

/**
 * The only safe way to log a failure: its kind, status and codes. Never its message or contents, which can repeat what
 * a server said, and that can include personal health information.
 */
export function describeError(error: unknown): string {
    if (typeof error !== "object" || error === null) return typeof error;
    const { name, statusCode, status, code, errorCode, traceId } = error as {
        name?: unknown;
        statusCode?: unknown;
        status?: unknown;
        code?: unknown;
        errorCode?: unknown;
        traceId?: unknown;
    };
    const safe = (value: unknown) =>
        typeof value === "number" ||
        (typeof value === "string" && /^[\w.:-]{1,64}$/.test(value))
            ? String(value)
            : undefined;
    return (
        [
            safe(name),
            safe(statusCode ?? status),
            safe(code ?? errorCode),
            safe(traceId),
        ]
            .filter(Boolean)
            .join(" ") || "error"
    );
}

class ConsoleLogger implements ILogger {
    private level: LogLevel = __DEV__ ? "debug" : "warn";
    private failuresExpected = false;

    setFailuresExpected(expected: boolean) {
        this.failuresExpected = expected;
    }

    setLevel(level: LogLevel) {
        this.level = level;
    }
    private enabled(level: LogLevel) {
        return order[level] >= order[this.level];
    }
    verbose(message: string) {
        if (this.enabled("verbose")) console.debug(`[verbose] ${message}`);
    }
    debug(message: string) {
        if (this.enabled("debug")) console.debug(`[debug] ${message}`);
    }
    info(message: string) {
        if (this.enabled("info")) console.info(`[info] ${message}`);
    }
    warn(message: string) {
        if (this.failuresExpected || message.includes("SessionEndedError"))
            this.debug(message);
        else if (this.enabled("warn")) console.warn(`[warn] ${message}`);
    }
    error(message: string, error?: unknown) {
        // cancelled by signing out, which is not a failure
        if (
            this.failuresExpected ||
            message.includes("SessionEndedError") ||
            isSessionEnded(error)
        )
            this.debug(message);
        else if (this.enabled("error"))
            console.error(
                `[error] ${message}`,
                error === undefined ? "" : describeError(error)
            );
    }
}

export const logger: ILogger = new ConsoleLogger();
