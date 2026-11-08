import { create } from "zustand";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import { Dataset } from "@/constants/tracking";
import type {
    ClinicalDocument,
    ClinicalDocumentFile,
} from "@/models/clinicalDocument";
import { LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type { EncodedMedia } from "@/models/laboratory";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";

import { useErrorStore } from "./error.store";
import { createRequestResultStore } from "./utils/createRequestResultStore";

/** Port of stores/clinicalDocument.ts: the documents (per hdid) ... */
const clinicalDocuments = createRequestResultStore<ClinicalDocument>({
    name: "Clinical documents",
    fetch: (hdid) => services.clinicalDocument.getRecords(hdid),
    dataset: Dataset.ClinicalDocuments,
    errorSource: ErrorSourceType.ClinicalDocument,
});
export const useClinicalDocumentStore = clinicalDocuments.store;
export const selectClinicalDocuments = clinicalDocuments.selectData;
export const selectClinicalDocumentsAreLoading =
    clinicalDocuments.selectIsLoading;

interface ClinicalDocumentFileState {
    files: Record<string, ClinicalDocumentFile>;
    getFile: (fileId: string, hdid: string) => Promise<EncodedMedia>;
    reset: () => void;
}

/** ... and their files, fetched once each. */
export const useClinicalDocumentFileStore = create<ClinicalDocumentFileState>(
    (set, get) => {
        const put = (fileId: string, file: Partial<ClinicalDocumentFile>) =>
            set((s) => ({
                files: {
                    ...s.files,
                    [fileId]: {
                        ...(s.files[fileId] ?? {
                            fileId,
                            status: LoadStatus.NONE,
                        }),
                        ...file,
                    },
                },
            }));
        return {
            files: {},
            getFile: async (fileId, hdid) => {
                const cached = get().files[fileId];
                if (cached?.file) {
                    logger.debug("File found stored, not querying!");
                    return cached.file;
                }
                put(fileId, { status: LoadStatus.REQUESTED });
                try {
                    const result = await services.clinicalDocument.getFile(
                        fileId,
                        hdid
                    );
                    if (result.resultStatus === ResultType.Success)
                        put(fileId, {
                            file: result.resourcePayload,
                            status: LoadStatus.LOADED,
                            error: undefined,
                        });
                    else {
                        if (result.resultError)
                            throw ResultError.fromModel(result.resultError);
                        logger.warn(`Clinical document file retrieval failed!`);
                    }
                    return result.resourcePayload;
                } catch (error) {
                    rethrowIfSessionEnded(error);
                    logger.error(`ERROR: ${describeError(error)}`);
                    const resultError =
                        error instanceof ResultError ? error : undefined;
                    put(fileId, {
                        error: resultError,
                        status: LoadStatus.ERROR,
                    });
                    if (resultError?.statusCode === 429)
                        useErrorStore
                            .getState()
                            .setTooManyRequestsWarning("page");
                    else
                        useErrorStore
                            .getState()
                            .addError(
                                ErrorType.Download,
                                ErrorSourceType.ClinicalDocument,
                                resultError?.traceId
                            );
                    throw error;
                }
            },
            reset: () => set({ files: {} }),
        };
    }
);
