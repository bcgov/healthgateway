import { defineStore } from "pinia";
import { ref } from "vue";

import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { LabResultState } from "@/models/datasetState";
import { ResultError } from "@/models/errors";
import { LaboratoryOrder, LaboratoryOrderResult } from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { Action, Dataset, Text } from "@/plugins/extensions";
import {
    ILaboratoryService,
    ILogger,
    ITrackingService,
} from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";

export const defaultLabResultState: LabResultState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    queued: false,
};

export const useLabResultStore = defineStore("labResult", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const laboratoryService = container.get<ILaboratoryService>(
        SERVICE_IDENTIFIER.LaboratoryService
    );
    const datasetMapUtil = new DatasetMapUtils<
        LaboratoryOrder[],
        LabResultState
    >(defaultLabResultState);

    const errorStore = useErrorStore();

    const labResultMap = ref(new Map<string, LabResultState>());

    function getLabResultState(hdid: string): LabResultState {
        return datasetMapUtil.getDatasetState(labResultMap.value, hdid);
    }

    function labResults(hdid: string): LaboratoryOrder[] {
        return getLabResultState(hdid).data;
    }

    function labResultsCount(hdid: string): number {
        return labResults(hdid).length;
    }

    function labResultsAreLoading(hdid: string): boolean {
        return getLabResultState(hdid).status === LoadStatus.REQUESTED;
    }

    function labResultsAreQueued(hdid: string): boolean {
        return getLabResultState(hdid).queued;
    }

    function setLabResults(
        hdid: string,
        laboratoryOrderResult: LaboratoryOrderResult
    ): void {
        datasetMapUtil.setStateData(
            labResultMap.value,
            hdid,
            laboratoryOrderResult.orders,
            {
                queued: laboratoryOrderResult.queued,
            }
        );
    }

    function setLabResultsRefreshInProgress(
        hdid: string,
        laboratoryOrderResult: LaboratoryOrderResult
    ): void {
        datasetMapUtil.setStateData(
            labResultMap.value,
            hdid,
            laboratoryOrderResult.orders,
            {
                queued: laboratoryOrderResult.queued,
                status: LoadStatus.REQUESTED,
            }
        );
    }

    function handleError(
        hdid: string,
        error: ResultError,
        errorType: ErrorType,
        errorSourceType: ErrorSourceType
    ) {
        logger.error(`ERROR: ${JSON.stringify(error)}`);

        datasetMapUtil.setStateError(labResultMap.value, hdid, error);

        if (error.statusCode === 429) {
            errorStore.setTooManyRequestsWarning("page");
        } else {
            errorStore.addError(errorType, errorSourceType, error.traceId);
        }
    }

    function retrieveLabResults(
        hdid: string
    ): Promise<RequestResult<LaboratoryOrderResult>> {
        const trackingService = container.get<ITrackingService>(
            SERVICE_IDENTIFIER.TrackingService
        );
        if (getLabResultState(hdid).status === LoadStatus.LOADED) {
            logger.debug("Lab results found stored, not querying!");
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: {
                    loaded: true,
                    queued: getLabResultState(hdid).queued,
                    retryin: 0,
                    orders: labResults(hdid),
                },
                resultStatus: ResultType.Success,
                totalResultCount: labResults(hdid).length,
            });
        }
        logger.debug("Retrieving Lab results");
        datasetMapUtil.setStateRequested(labResultMap.value, hdid);
        return laboratoryService
            .getLaboratoryOrders(hdid)
            .then((result) => {
                if (result.resourcePayload.orders.length > 0) {
                    trackingService.trackEvent({
                        action: Action.Load,
                        text: Text.Data,
                        dataset: Dataset.LabResults,
                    });
                }
                const payload = result.resourcePayload;
                if (
                    result.resultStatus === ResultType.Success &&
                    payload.loaded
                ) {
                    logger.debug("Lab results loaded");
                    setLabResults(hdid, payload);
                } else if (
                    result.resultError?.actionCode == ActionType.Refresh &&
                    !payload.loaded &&
                    payload.retryin > 0
                ) {
                    logger.debug(
                        "Refresh in progress... partially loaded lab results"
                    );
                    setLabResultsRefreshInProgress(hdid, payload);
                    setTimeout(() => {
                        logger.info(`Re-querying for lab results`);
                        retrieveLabResults(hdid);
                    }, payload.retryin);
                } else {
                    if (result.resultError) {
                        throw ResultError.fromResultErrorDetails(
                            result.resultError
                        );
                    }
                    logger.warn(
                        `Laboratory results retrieval failed! ${JSON.stringify(
                            result
                        )}`
                    );
                }
                return result;
            })
            .catch((error: ResultError) => {
                handleError(
                    hdid,
                    error,
                    ErrorType.Retrieve,
                    ErrorSourceType.Laboratory
                );
                throw error;
            });
    }

    return {
        labResults,
        labResultsCount,
        labResultsAreLoading,
        labResultsAreQueued,
        retrieveLabResults,
    };
});
