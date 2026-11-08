import { ResultType } from "@/constants/resultType";
import { ResultError } from "@/models/errors";
import type { RequestResult } from "@/models/requestResult";

/** Port of utility/requestResultUtil.ts handleResult: the payload on success, else the API error thrown. */
export function handleResult<T>(requestResult: RequestResult<T>): T {
    if (requestResult.resultStatus === ResultType.Success)
        return requestResult.resourcePayload;
    if (requestResult.resultError)
        throw ResultError.fromModel(requestResult.resultError);
    throw new ResultError("RequestResultUtil", "Unknown API Error");
}
