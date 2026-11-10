import type { ErrorSourceType } from "@/constants/errorType";
import { ErrorType, TooManyRequestsLocation } from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import type { EncodedMedia } from "@/models/laboratory";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";

import { saveBase64File } from "./saveFile";

/** Port of utility/encodedMediaDownloadUtility.ts downloadEncodedMedia (the API sends base64). */
export function downloadEncodedMedia(
    media: EncodedMedia,
    fileName: string
): Promise<void> {
    return saveBase64File(media.data, media.mediaType, fileName);
}

/** Port of handleReportDownloadError */
export function handleReportDownloadError(
    error: unknown,
    errorSource: ErrorSourceType
): void {
    const resultError = error instanceof ResultError ? error : undefined;
    logger.error(describeError(error));
    if (resultError?.statusCode === 429)
        useErrorStore
            .getState()
            .setTooManyRequestsError(TooManyRequestsLocation.Page);
    else
        useErrorStore
            .getState()
            .addError(ErrorType.Download, errorSource, resultError?.traceId);
}
