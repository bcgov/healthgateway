import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ServiceCode } from "@/constants/serviceCodes";
import { BannerError, HttpError, ResultError } from "@/models/errors";

export default class ErrorTranslator {
    public static toBannerError(
        errorType: ErrorType,
        source: ErrorSourceType,
        traceId: string | undefined
    ): BannerError {
        const title = this.getErrorTitle(errorType, source);
        const formattedSource = this.formatSourceType(source, false, true);

        return {
            title,
            source: formattedSource,
            traceId,
        };
    }

    public static toCustomBannerError(
        title: string,
        source: ErrorSourceType,
        traceId: string | undefined
    ): BannerError {
        const formattedSource = this.formatSourceType(source, false, true);

        return {
            title,
            source: formattedSource,
            traceId,
        };
    }

    public static internalNetworkError(
        error: HttpError,
        service: ServiceCode
    ): ResultError {
        return {
            errorCode: "ClientApp-CI-" + service,
            resultMessage: error.message,
            traceId: "",
            statusCode: error.statusCode,
        };
    }

    public static moduleDisabledError(service: ServiceCode): ResultError {
        return {
            errorCode: "ClientApp-I-" + service,
            resultMessage: "Module Disabled",
            traceId: "",
        };
    }

    private static getErrorTitle(
        errorType: ErrorType,
        source: ErrorSourceType
    ): string {
        const formattedSource = this.formatSourceType(
            source,
            errorType === ErrorType.Retrieve,
            false
        );

        switch (errorType) {
            case ErrorType.Create:
                return "Unable to add " + formattedSource;
            case ErrorType.Retrieve:
                return "Unable to retrieve " + formattedSource;
            case ErrorType.Update:
                return "Unable to save changes to " + formattedSource;
            case ErrorType.Delete:
                return "Unable to remove " + formattedSource;
            case ErrorType.Download:
                return "Unable to download " + formattedSource;
            case ErrorType.Custom:
                return "An error has occurred";
        }
    }

    private static pluralizeErrorSourceType(source: ErrorSourceType): string {
        switch (source) {
            case ErrorSourceType.MedicationRequests:
                return "special authorities";
            case ErrorSourceType.TermsOfService:
                return "terms of service";
            default:
                return `${source}s`;
        }
    }

    private static formatSourceType(
        source: ErrorSourceType,
        pluralize: boolean,
        simplify: boolean
    ): string {
        let formattedSource: string = source;
        if (pluralize) {
            formattedSource = this.pluralizeErrorSourceType(source);
        }

        if (simplify) {
            formattedSource = formattedSource.toLowerCase().replace(" ", "‑");
        }

        return formattedSource;
    }
}
