import { ResultType } from "@/constants/resulttype";
import RequestResult from "@/models/requestResult";

export default abstract class RequestResultUtil {
    public static handleResult<T>(
        requestResult: RequestResult<T>,
        resolve: (value: T | PromiseLike<T>) => void,
        reject: (reason?: unknown) => void
    ): void {
        if (requestResult.resultStatus === ResultType.Success) {
            resolve(requestResult.resourcePayload);
        } else {
            reject(requestResult.resultError);
        }
    }
}
