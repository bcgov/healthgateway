import { ResultType } from "@/constants/resulttype";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";

export default abstract class RequestResultUtil {
    public static handleResult<T>(requestResult: RequestResult<T>): T {
        if (requestResult.resultStatus === ResultType.Success) {
            return requestResult.resourcePayload;
        } else if (requestResult.resultError) {
            throw ResultError.fromApiResultError(requestResult.resultError);
        } else {
            throw new ResultError("RequestResultUtil", "Unknown API Error");
        }
    }
}
