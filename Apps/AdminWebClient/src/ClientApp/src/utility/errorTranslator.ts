import { HttpError, ResultError } from "@/models/errors";

export default abstract class ErrorTranslator {
    public static internalNetworkError(error: HttpError): ResultError {
        return {
            errorCode: "AdminClientApp-CI",
            resultMessage: error.message,
            traceId: "",
            statusCode: error.statusCode,
        };
    }
}
