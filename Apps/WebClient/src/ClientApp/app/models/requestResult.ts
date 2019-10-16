export default interface RequestResult {
  // The request resource payload
  resourcePayload: any;
  // The total number of records for pagination
  totalResultCount: number;
  // The current page index for pagination
  pageIndex: number;
  // The current page size for pagnation
  pageSize: number;
  // The error message (could be empty)
  errorMessage: string;
}
