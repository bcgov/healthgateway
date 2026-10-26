import { saveAs } from "file-saver";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import EncodedMedia from "@/models/encodedMedia";
import { ResultError } from "@/models/errors";
import { ILogger } from "@/services/interfaces";
import type { useErrorStore } from "@/stores/error";

type ErrorStore = ReturnType<typeof useErrorStore>;

export async function downloadEncodedMedia(
    media: EncodedMedia,
    fileName: string
): Promise<void> {
    const response = await fetch(
        `data:${media.mediaType};${media.encoding},${media.data}`
    );
    const blob = await response.blob();
    saveAs(blob, fileName);
}

export function handleReportDownloadError(
    error: unknown,
    errorSource: ErrorSourceType,
    logger: ILogger,
    errorStore: ErrorStore
): void {
    const resultError = error instanceof ResultError ? error : undefined;
    logger.error(
        resultError?.message ??
            (error instanceof Error ? error.message : String(error))
    );

    if (resultError?.statusCode === 429) {
        errorStore.setTooManyRequestsError("page");
    } else {
        errorStore.addError(
            ErrorType.Download,
            errorSource,
            resultError?.traceId
        );
    }
}
