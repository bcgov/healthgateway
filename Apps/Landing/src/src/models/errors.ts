import { ActionType } from "@/constants/actionType";
import { ResultErrorDetails } from "@/models/requestResult";

export class HttpError extends Error {
    // The HTTP status code returned by the request.
    statusCode?: number;

    constructor(message: string, statusCode?: number) {
        super(message);
        this.statusCode = statusCode;
    }
}

export class ResultError extends Error {
    // Code associated with the error.
    errorCode: string;
    // The trace ID associated with the request.
    traceId: string;
    // The action code.
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

    /**
     * Transform the API model into a ResultError type which is a throwable JS Error object.
     * @param apiResultError Error model returned by the HealthGateway API
     * @returns
     */
    public static fromModel(apiResultError: ResultErrorDetails): ResultError {
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
