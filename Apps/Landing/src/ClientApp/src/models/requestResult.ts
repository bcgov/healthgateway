import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";

export default interface RequestResult<T> {
    // The request resource payload
    resourcePayload: T;
    // The total number of records for pagination
    totalResultCount: number;
    // The current page index for pagination
    pageIndex: number;
    // The current page size for pagnation
    pageSize: number;
    //The status of the request
    resultStatus: ResultType;
    // The result error associated to the request (could be empty)
    resultError?: ResultErrorDetails;
}

// HG API binding interface for ErrorResult model
export interface ResultErrorDetails {
    // Message that will always be populated when ResultType is Error.
    resultMessage?: string;
    // Code that will always be populated when ResultType is Error.
    errorCode: string;
    // The trace ID associated with the request.
    traceId: string;
    // The action code that will be set when ResultType is ActionRequired.
    actionCode?: ActionType;
    // The HTTP status code returned by the request.
    statusCode?: number;
}
