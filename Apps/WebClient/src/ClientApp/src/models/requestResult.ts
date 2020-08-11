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
    resultError?: ResultError;
}

export interface ResultError {
    // The error code  associated to the request
    errorCode: string;
    // The message associated to the error request
    errorDetail: string;
}
