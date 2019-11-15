import { ResultType } from "@/constants/resulttype";

export default interface RequestResult {
  // The request resource payload
  resourcePayload: any;
  // The total number of records for pagination
  totalResultCount: number;
  // The current page index for pagination
  pageIndex: number;
  // The current page size for pagnation
  pageSize: number;
  //The status of the request
  resultStatus: ResultType;
  // The message associated to the request (could be empty)
  resultMessage: string;
}
