import { saveAs } from "file-saver";
import type { Ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import { LaboratoryReport } from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { ILogger } from "@/services/interfaces";
import type { useErrorStore } from "@/stores/error";

type ErrorStore = ReturnType<typeof useErrorStore>;

export async function downloadLaboratoryReport(
    getReport: () => Promise<RequestResult<LaboratoryReport>>,
    fileName: string,
    errorSource: ErrorSourceType,
    logger: ILogger,
    errorStore: ErrorStore,
    isLoading: Ref<boolean>
): Promise<void> {
    isLoading.value = true;

    try {
        const result = await getReport();
        const report = result.resourcePayload;
        const response = await fetch(
            `data:${report.mediaType};${report.encoding},${report.data}`
        );
        const blob = await response.blob();
        saveAs(blob, fileName);
    } catch (error) {
        logger.error(error instanceof Error ? error.message : String(error));
        if (error instanceof ResultError && error.statusCode === 429) {
            errorStore.setTooManyRequestsError("page");
        } else {
            errorStore.addError(
                ErrorType.Download,
                errorSource,
                error instanceof ResultError ? error.traceId : undefined
            );
        }
    } finally {
        isLoading.value = false;
    }
}
