import { defineStore } from "pinia";
import { LabResultState } from "@/models/datasetState";
import { LoadStatus } from "@/models/storeOperations";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import { container } from "@/ioc/container";
import { ref } from "vue";
import { LaboratoryOrder, LaboratoryOrderResult } from "@/models/laboratory";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";
import { useErrorStore } from "@/stores/error";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import { ActionType } from "@/constants/actionType";

export const defaultLabResultState: LabResultState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    queued: false,
};

export const useLaboratoryStore = defineStore("laboratory", () => {
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

    function laboratoryOrders(hdid: string): LaboratoryOrder[] {
        return getLabResultState(hdid).data;
    }

    function laboratoryOrdersCount(hdid: string): number {
        return laboratoryOrders(hdid).length;
    }

    function laboratoryResultsAreLoading(hdid: string): boolean {
        return getLabResultState(hdid).status === LoadStatus.REQUESTED;
    }

    function laboratoryOrdersAreQueued(hdid: string): boolean {
        return getLabResultState(hdid).queued;
    }

    function setLaboratoryOrders(
        hdid: string,
        laboratoryOrderResult: LaboratoryOrderResult
    ): void {
        datasetMapUtil.setStateData(
            labResultMap.value,
            hdid,
            laboratoryOrderResult.orders,
            {
                queue: laboratoryOrderResult.queued,
            }
        );
    }

    function setLaboratoryOrdersRefreshInProgress(
        hdid: string,
        laboratoryOrderResult: LaboratoryOrderResult
    ): void {
        datasetMapUtil.setStateData(
            labResultMap.value,
            hdid,
            laboratoryOrderResult.orders,
            {
                queue: laboratoryOrderResult.queued,
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

    function retrieveLaboratoryOrders(
        hdid: string
    ): Promise<RequestResult<LaboratoryOrderResult>> {
        if (getLabResultState(hdid).status === LoadStatus.LOADED) {
            logger.debug("Laboratory order found stored, not querying!");
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: {
                    loaded: true,
                    queued: getLabResultState(hdid).queued,
                    retryin: 0,
                    orders: laboratoryOrders(hdid),
                },
                resultStatus: ResultType.Success,
                totalResultCount: laboratoryOrders(hdid).length,
            });
        }
        logger.debug("Retrieving laboratory orders");
        datasetMapUtil.setStateRequested(labResultMap.value, hdid);
        return laboratoryService
            .getLaboratoryOrders(hdid)
            .then((result) => {
                const payload = result.resourcePayload;
                if (
                    result.resultStatus === ResultType.Success &&
                    payload.loaded
                ) {
                    logger.debug("Laboratory orders loaded");
                    setLaboratoryOrders(hdid, payload);
                } else if (
                    result.resultError?.actionCode == ActionType.Refresh &&
                    !payload.loaded &&
                    payload.retryin > 0
                ) {
                    logger.debug(
                        "Refresh in progress... partially load laboratory orders"
                    );
                    setLaboratoryOrdersRefreshInProgress(hdid, payload);
                    setTimeout(() => {
                        logger.info(`Re-querying for laboratory orders`);
                        retrieveLaboratoryOrders(hdid);
                    }, payload.retryin);
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
        laboratoryOrders,
        laboratoryOrdersCount,
        laboratoryResultsAreLoading,
        laboratoryOrdersAreQueued,
        retrieveLaboratoryOrders,
    };
});
