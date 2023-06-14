import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/errors";

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
