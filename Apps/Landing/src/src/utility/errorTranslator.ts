import { ServiceCode } from "@/constants/serviceCodes";
import { HttpError, ResultError } from "@/models/errors";

export default abstract class ErrorTranslator {
    public static internalNetworkError(
        error: HttpError,
        service: ServiceCode,
        traceId?: string
    ): ResultError {
        return new ResultError(
            "ClientApp-CI-" + service,
            error.message,
            traceId ?? "",
            error.statusCode
        );
    }
}
