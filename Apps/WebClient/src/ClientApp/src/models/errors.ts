import { ActionType } from "@/constants/actionType";

export interface BannerError {
    title: string;
    traceId?: string;
    source: string;
}

export interface CustomBannerError {
    title: string;
    description: string;
    detail?: string;
}

export class HttpError extends Error {
    // The HTTP status code returned by the request.
    statusCode?: number;

    constructor(message: string, statusCode?: number) {
        super(message);
        this.statusCode = statusCode;
    }
}

// HG API binding interface for ErrorResult model
export interface ResultErrorDetails {
    // API's ResultMessage mapping property. This should be treated as private and the message should be mapped to and preferred.
    resultMessage?: string;
    // The error code associated with the request. Will always be populated when ResultType is Error.
    errorCode: string;
    // The trace ID associated with the request.
    traceId: string;
    // The action code that will be set when ResultType is ActionRequired.
    actionCode?: ActionType;
    // The HTTP status code returned by the request.
    statusCode?: number;
}

export class ResultError extends Error {
    // The error code associated with the request. Will always be populated when ResultType is Error.
    errorCode: string;
    // The trace ID associated with the request.
    traceId: string;
    // The action code that will be set when ResultType is ActionRequired.
    actionCode?: ActionType;
    // The HTTP status code returned by the request.
    statusCode?: number;

    constructor(
        errorCode: string,
        resultMessage: string,
        traceId: string = "",
        statusCode?: number
    ) {
        super(resultMessage);
        this.errorCode = errorCode;
        this.traceId = traceId;
        this.statusCode = statusCode;
    }

    public static fromResultErrorDetails(
        apiResultError: ResultErrorDetails
    ): ResultError {
        const resultError = new ResultError(
            apiResultError.errorCode,
            apiResultError.resultMessage ?? "Unknown API Error",
            apiResultError.traceId,
            apiResultError.statusCode
        );
        resultError.actionCode = apiResultError.actionCode;
        return resultError;
    }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function instanceOfResultError(object: any): object is ResultError {
    return "errorCode" in object && "traceId" in object && "message" in object;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function isTooManyRequestsError(object: any): boolean {
    return instanceOfResultError(object) && object.statusCode === 429;
}
