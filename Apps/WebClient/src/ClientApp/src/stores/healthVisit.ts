import { defineStore } from "pinia";
import { ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { HealthVisitState } from "@/models/datasetState";
import { Encounter } from "@/models/encounter";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { Action, Dataset, Text } from "@/plugins/extensions";
import {
    IEncounterService,
    ILogger,
    ITrackingService,
} from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";

const defaultHealthVisitState: HealthVisitState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};

export const useHealthVisitStore = defineStore("healthVisit", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const healthVisitsService = container.get<IEncounterService>(
        SERVICE_IDENTIFIER.EncounterService
    );
    const datasetMapUtil = new DatasetMapUtils<Encounter[], HealthVisitState>(
        defaultHealthVisitState
    );

    const errorStore = useErrorStore();

    const healthVisitsMap = ref(new Map<string, HealthVisitState>());

    function getHealthVisitState(hdid: string): HealthVisitState {
        return datasetMapUtil.getDatasetState(healthVisitsMap.value, hdid);
    }

    function healthVisits(hdid: string): Encounter[] {
        return getHealthVisitState(hdid).data;
    }

    function healthVisitsAreLoading(hdid: string): boolean {
        return getHealthVisitState(hdid).status === LoadStatus.REQUESTED;
    }

    function healthVisitsCount(hdid: string): number {
        return getHealthVisitState(hdid).data.length;
    }

    function handleError(
        hdid: string,
        resultError: ResultError,
        errorType: ErrorType,
        errorSourceType: ErrorSourceType
    ): void {
        logger.error(`ERROR: ${JSON.stringify(resultError)}`);
        datasetMapUtil.setStateError(healthVisitsMap.value, hdid, resultError);
        if (resultError.statusCode === 429) {
            errorStore.setTooManyRequestsWarning("page");
        } else {
            errorStore.addError(
                errorType,
                errorSourceType,
                resultError.traceId
            );
        }
    }

    function retrieveHealthVisits(
        hdid: string
    ): Promise<RequestResult<Encounter[]>> {
        const trackingService = container.get<ITrackingService>(
            SERVICE_IDENTIFIER.TrackingService
        );
        if (getHealthVisitState(hdid).status === LoadStatus.LOADED) {
            logger.debug(`Health Visits found stored, not querying!`);
            const visits: Encounter[] = healthVisits(hdid);
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: visits,
                resultStatus: ResultType.Success,
                totalResultCount: visits.length,
            });
        }

        logger.debug(`Retrieving Health Visits`);
        datasetMapUtil.setStateRequested(healthVisitsMap.value, hdid);
        return healthVisitsService
            .getPatientEncounters(hdid)
            .then((result) => {
                const payload = result.resourcePayload;
                if (result.resultStatus === ResultType.Success) {
                    if (result.resourcePayload.length > 0) {
                        trackingService.trackEvent({
                            action: Action.Load,
                            text: Text.Data,
                            dataset: Dataset.HealthVisits,
                        });
                    }
                    logger.info(`Health Visits loaded.`);
                    datasetMapUtil.setStateData(
                        healthVisitsMap.value,
                        hdid,
                        payload
                    );
                } else {
                    if (result.resultError) {
                        throw ResultError.fromModel(result.resultError);
                    }
                    logger.warn(
                        `Health visits retrieval failed! ${JSON.stringify(
                            result
                        )}`
                    );
                }
                return result;
            })
            .catch((resultError: ResultError) => {
                handleError(
                    hdid,
                    resultError,
                    ErrorType.Retrieve,
                    ErrorSourceType.Encounter
                );
                throw resultError;
            });
    }

    return {
        healthVisits,
        healthVisitsAreLoading,
        healthVisitsCount,
        retrieveHealthVisits,
    };
});
