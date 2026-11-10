import type { ActionType } from "@/constants/actionType";
import type { ResultType } from "@/constants/resultType";

/** Port of models/requestResult.ts: the HG API envelope. */
export interface RequestResult<T> {
    resourcePayload: T;
    totalResultCount: number;
    pageIndex: number;
    pageSize: number;
    resultStatus: ResultType;
    resultError?: ResultErrorDetails;
}

/** HG API binding for ErrorResult */
export interface ResultErrorDetails {
    resultMessage?: string;
    errorCode: string;
    traceId: string;
    actionCode?: ActionType;
    statusCode?: number;
}
