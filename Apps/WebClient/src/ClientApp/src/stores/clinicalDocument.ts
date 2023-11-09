import { defineStore } from "pinia";
import { ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    ClinicalDocument,
    ClinicalDocumentFile,
} from "@/models/clinicalDocument";
import { ClinicalDocumentDatasetState } from "@/models/datasetState";
import EncodedMedia from "@/models/encodedMedia";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { Action, Dataset, Text } from "@/plugins/extensions";
import {
    IClinicalDocumentService,
    ILogger,
    ITrackingService,
} from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";

const defaultClinicalDocumentDatasetState: ClinicalDocumentDatasetState = {
    data: [],
    statusMessage: "",
    status: LoadStatus.NONE,
    error: undefined,
};

export const useClinicalDocumentStore = defineStore("clinicalDocument", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const clinicalDocumentService = container.get<IClinicalDocumentService>(
        SERVICE_IDENTIFIER.ClinicalDocumentService
    );
    const datasetMapUtil = new DatasetMapUtils<
        ClinicalDocument[],
        ClinicalDocumentDatasetState
    >(defaultClinicalDocumentDatasetState);

    const errorStore = useErrorStore();

    const clinicalDocumentMap = ref(
        new Map<string, ClinicalDocumentDatasetState>()
    );
    const files = ref(new Map<string, ClinicalDocumentFile>());

    function getClinicalDocumentState(
        hdid: string
    ): ClinicalDocumentDatasetState {
        return datasetMapUtil.getDatasetState(clinicalDocumentMap.value, hdid);
    }

    function clinicalDocuments(hdid: string): ClinicalDocument[] {
        return getClinicalDocumentState(hdid).data;
    }

    function clinicalDocumentsAreLoading(hdid: string): boolean {
        return getClinicalDocumentState(hdid).status === LoadStatus.REQUESTED;
    }

    function clinicalDocumentsCount(hdid: string): number {
        return getClinicalDocumentState(hdid).data.length;
    }

    function setFileRequested(fileId: string) {
        files.value.set(fileId, {
            fileId,
            status: LoadStatus.REQUESTED,
        });
    }

    function setFile(fileId: string, file: EncodedMedia) {
        files.value.set(fileId, {
            fileId,
            file,
            error: undefined,
            status: LoadStatus.LOADED,
        });
    }

    function setFileError(fileId: string, error: ResultError) {
        files.value.set(fileId, {
            fileId,
            error,
            file: undefined,
            status: LoadStatus.ERROR,
        });
    }

    function retrieveClinicalDocuments(
        hdid: string
    ): Promise<RequestResult<ClinicalDocument[]>> {
        const trackingService = container.get<ITrackingService>(
            SERVICE_IDENTIFIER.TrackingService
        );
        if (getClinicalDocumentState(hdid).status === LoadStatus.LOADED) {
            logger.debug(`Clinical documents found stored, not querying!`);
            const records = clinicalDocuments(hdid);
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: records,
                resultStatus: ResultType.Success,
                totalResultCount: records.length,
            });
        }

        logger.debug("Retrieving clinical documents");
        datasetMapUtil.setStateRequested(clinicalDocumentMap.value, hdid);
        return clinicalDocumentService
            .getRecords(hdid)
            .then((result) => {
                if (result.resultStatus === ResultType.Success) {
                    trackingService.trackEvent({
                        action: Action.Load,
                        text: Text.Data,
                        dataset: Dataset.ClinicalDocuments,
                    });
                    datasetMapUtil.setStateData(
                        clinicalDocumentMap.value,
                        hdid,
                        result.resourcePayload
                    );
                } else {
                    if (result.resultError) {
                        throw result.resultError;
                    }
                    logger.warn(
                        `Clinical documents retrieval failed! ${JSON.stringify(
                            result
                        )}`
                    );
                }
                return result;
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Retrieve, hdid);
                throw error;
            });
    }

    function getFile(fileId: string, hdid: string): Promise<EncodedMedia> {
        const file = files.value.get(fileId);
        if (file?.file) {
            logger.debug(`File found stored, not querying!`);
            return Promise.resolve(file.file);
        }

        setFileRequested(fileId);
        return clinicalDocumentService
            .getFile(fileId, hdid)
            .then((result) => {
                const payload = result.resourcePayload;
                if (result.resultStatus === ResultType.Success) {
                    setFile(fileId, payload);
                } else {
                    if (result.resultError) {
                        throw result.resultError;
                    }
                    logger.warn(
                        `Clinical document file retrieval failed! ${JSON.stringify(
                            result
                        )}`
                    );
                }
                return result.resourcePayload;
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Download, hdid, fileId);
                throw error;
            });
    }

    function handleError(
        error: ResultError,
        errorType: ErrorType,
        hdid: string,
        fileId?: string
    ) {
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        if (fileId) {
            setFileError(fileId, error);
        } else {
            datasetMapUtil.setStateError(
                clinicalDocumentMap.value,
                hdid,
                error
            );
        }

        if (error.statusCode === 429) {
            errorStore.setTooManyRequestsWarning("page");
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.ClinicalDocument,
                error.traceId
            );
        }
    }

    return {
        files,
        clinicalDocuments,
        clinicalDocumentsCount,
        clinicalDocumentsAreLoading,
        retrieveClinicalDocuments,
        getFile,
    };
});
