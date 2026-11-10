import type { ActionType } from "@/constants/actionType";

import type { ResultErrorDetails } from "./requestResult";

export class HttpError extends Error {
    constructor(
        message: string,
        public readonly statusCode?: number,
        public readonly traceId?: string
    ) {
        super(message);
        this.name = "HttpError";
    }
}

/**
 * How a call cancelled by signing out ends. It belongs to a login that is over, so nothing may be written or reported
 * because of it: `catch` blocks hand it on or return, while `finally` cleanup still runs.
 */
export class SessionEndedError extends Error {
    constructor() {
        super("The session ended");
        this.name = "SessionEndedError";
    }
}

export const isSessionEnded = (error: unknown) =>
    error instanceof SessionEndedError;

export function rethrowIfSessionEnded(error: unknown): void {
    if (error instanceof SessionEndedError) throw error;
}

/** Error surfaced to stores/views; mirrors the web client's ResultError. */
export class ResultError extends Error {
    /** the action the API asks for (ActionType), e.g. PROTECTED */
    public actionCode?: ActionType;

    constructor(
        public readonly errorCode: string,
        message: string,
        public readonly statusCode?: number,
        public readonly traceId?: string
    ) {
        super(message);
        this.name = "ResultError";
    }

    /** ResultError.fromModel: from the API's resultError */
    public static fromModel(apiResultError: ResultErrorDetails): ResultError {
        const resultError = new ResultError(
            apiResultError.errorCode,
            apiResultError.resultMessage ?? "Unknown API Error",
            apiResultError.statusCode,
            apiResultError.traceId
        );
        resultError.actionCode = apiResultError.actionCode;
        return resultError;
    }
}

export function isTooManyRequestsError(error: unknown): boolean {
    return (
        (error instanceof HttpError || error instanceof ResultError) &&
        error.statusCode === 429
    );
}
