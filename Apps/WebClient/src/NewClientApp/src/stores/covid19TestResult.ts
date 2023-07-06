﻿import { defineStore } from "pinia";
import { ref } from "vue";

import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { Covid19TestResultState } from "@/models/datasetState";
import { ResultError } from "@/models/errors";
import {
    Covid19LaboratoryOrder,
    Covid19LaboratoryOrderResult,
} from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";

export const defaultCovid19TestResultState: Covid19TestResultState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};

export const useCovid19TestResultStore = defineStore(
    "covid19TestResult",
    () => {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
        const datasetMapUtil = new DatasetMapUtils<
            Covid19LaboratoryOrder[],
            Covid19TestResultState
        >(defaultCovid19TestResultState);

        const errorStore = useErrorStore();

        const covid19TestResultMap = ref(
            new Map<string, Covid19TestResultState>()
        );

        function getCovid19TestResultState(
            hdid: string
        ): Covid19TestResultState {
            return datasetMapUtil.getDatasetState(
                covid19TestResultMap.value,
                hdid
            );
        }

        function covid19LaboratoryOrders(
            hdid: string
        ): Covid19LaboratoryOrder[] {
            return getCovid19TestResultState(hdid).data;
        }

        function covid19LaboratoryOrdersCount(hdid: string): number {
            return covid19LaboratoryOrders(hdid).length;
        }

        function covid19LaboratoryOrdersAreLoading(hdid: string): boolean {
            return (
                getCovid19TestResultState(hdid).status === LoadStatus.REQUESTED
            );
        }

        function setCovid19LaboratoryOrders(
            hdid: string,
            orders: Covid19LaboratoryOrder[]
        ): void {
            datasetMapUtil.setStateData(
                covid19TestResultMap.value,
                hdid,
                orders
            );
        }

        function handleError(
            hdid: string,
            error: ResultError,
            errorType: ErrorType,
            errorSourceType: ErrorSourceType
        ) {
            logger.error(`ERROR: ${JSON.stringify(error)}`);

            datasetMapUtil.setStateError(
                covid19TestResultMap.value,
                hdid,
                error
            );

            if (error.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(errorType, errorSourceType, error.traceId);
            }
        }

        function retrieveCovid19LaboratoryOrders(
            hdid: string
        ): Promise<RequestResult<Covid19LaboratoryOrderResult>> {
            if (getCovid19TestResultState(hdid).status === LoadStatus.LOADED) {
                logger.debug(
                    "COVID-19 laboratory orders found stored, not querying!"
                );
                return Promise.resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {
                        loaded: true,
                        retryin: 0,
                        orders: covid19LaboratoryOrders(hdid),
                    },
                    resultStatus: ResultType.Success,
                    totalResultCount: covid19LaboratoryOrders(hdid).length,
                });
            }
            logger.debug("Retrieving COVID-19 laboratory orders");
            datasetMapUtil.setStateRequested(covid19TestResultMap.value, hdid);
            return laboratoryService
                .getCovid19LaboratoryOrders(hdid)
                .then((result) => {
                    const payload = result.resourcePayload;
                    if (
                        result.resultStatus === ResultType.Success &&
                        payload.loaded
                    ) {
                        logger.debug("COVID-19 Laboratory orders loaded");
                        setCovid19LaboratoryOrders(hdid, payload.orders);
                    } else if (
                        result.resultError?.actionCode == ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.debug("COVID-19 laboratory orders not loaded");
                        setTimeout(() => {
                            logger.info(
                                `Re-querying for COVID-19 laboratory orders`
                            );
                            retrieveCovid19LaboratoryOrders(hdid);
                        }, payload.retryin);
                    } else {
                        throw result.resultError;
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
            covid19LaboratoryOrders,
            covid19LaboratoryOrdersCount,
            covid19LaboratoryOrdersAreLoading,
            retrieveCovid19LaboratoryOrders,
        };
    }
);
