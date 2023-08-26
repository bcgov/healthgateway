import { ResultType } from "@/constants/resulttype";
import RequestResult from "@/models/requestResult";

export default abstract class RequestResultUtil {
    public static handleResult<T>(requestResult: RequestResult<T>): T {
        if (requestResult.resultStatus === ResultType.Success) {
            return requestResult.resourcePayload;
        } else {
            throw requestResult.resultError;
        }
    }
}
