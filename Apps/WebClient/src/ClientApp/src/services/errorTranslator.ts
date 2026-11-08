import { ErrorSourceType, ErrorType, ServiceCode } from "@/constants/errorType";
import { HttpError, ResultError, rethrowIfSessionEnded } from "@/models/errors";

export function toResultError(
    error: unknown,
    serviceCode: ServiceCode
): ResultError {
    rethrowIfSessionEnded(error);
    if (error instanceof ResultError) return error;
    if (error instanceof HttpError) {
        return new ResultError(
            `ClientApp-CI-${serviceCode}`,
            error.message || "An unexpected error occurred.",
            error.statusCode,
            error.traceId
        );
    }
    return new ResultError(
        `ClientApp-CI-${serviceCode}`,
        error instanceof Error ? error.message : "An unexpected error occurred."
    );
}

// ---- banner errors (ErrorTranslator.toBannerError / toCustomBannerError) ----

export interface BannerError {
    title: string;
    /** simplified source: lower case, first space → non-breaking hyphen (used in the error details line) */
    source: string;
    traceId?: string;
}

function pluralize(source: ErrorSourceType): string {
    switch (source) {
        case ErrorSourceType.MedicationRequests:
            return "special authorities";
        case ErrorSourceType.Patient:
            return "patient";
        case ErrorSourceType.Profile:
            return "profile";
        case ErrorSourceType.QuickLinks:
            return "quick links";
        case ErrorSourceType.TermsOfService:
            return "terms of service";
        case ErrorSourceType.PatientData:
            return "patient data";
        default:
            return `${source}s`;
    }
}

export function formatSourceType(
    source: ErrorSourceType,
    plural: boolean,
    simplify: boolean
): string {
    let formatted: string = plural ? pluralize(source) : source;
    // String.replace with a string pattern replaces the first space only, like the web client
    if (simplify) formatted = formatted.toLowerCase().replace(" ", "\u2011");
    return formatted;
}

export function errorTitle(type: ErrorType, source: ErrorSourceType): string {
    const formatted = formatSourceType(
        source,
        type === ErrorType.Retrieve,
        false
    );
    switch (type) {
        case ErrorType.Create:
            return "Unable to add " + formatted;
        case ErrorType.Retrieve:
            return "Unable to retrieve " + formatted;
        case ErrorType.Update:
            return "Unable to save changes to " + formatted;
        case ErrorType.Delete:
            return "Unable to remove " + formatted;
        case ErrorType.Download:
            return "Unable to download " + formatted;
        case ErrorType.Custom:
            return "An error has occurred";
    }
}

export const toBannerError = (
    type: ErrorType,
    source: ErrorSourceType,
    traceId?: string
): BannerError => ({
    title: errorTitle(type, source),
    source: formatSourceType(source, false, true),
    traceId,
});

export const toCustomBannerError = (
    title: string,
    source: ErrorSourceType,
    traceId?: string
): BannerError => ({
    title,
    source: formatSourceType(source, false, true),
    traceId,
});
