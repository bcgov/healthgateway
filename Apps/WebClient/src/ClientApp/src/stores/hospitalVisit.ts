import { defineStore } from "pinia";
import { ref } from "vue";

import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { HospitalVisitState } from "@/models/datasetState";
import { HospitalVisit } from "@/models/encounter";
import { ResultError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { Action, Dataset, Text } from "@/plugins/extensions";
import {
    IHospitalVisitService,
    ILogger,
    ITrackingService,
} from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";

const defaultHospitalVisitState: HospitalVisitState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    queued: false,
};

export const useHospitalVisitStore = defineStore("hospitalVisit", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const hospitalVisitService = container.get<IHospitalVisitService>(
        SERVICE_IDENTIFIER.HospitalVisitService
    );
    const datasetMapUtil = new DatasetMapUtils<
        HospitalVisit[],
        HospitalVisitState
    >(defaultHospitalVisitState);

    const errorStore = useErrorStore();

    const hospitalVisitsMap = ref(new Map<string, HospitalVisitState>());

    function getHospitalVisitsState(hdid: string): HospitalVisitState {
        return datasetMapUtil.getDatasetState(hospitalVisitsMap.value, hdid);
    }

    function hospitalVisits(hdid: string): HospitalVisit[] {
        return getHospitalVisitsState(hdid).data;
    }

    function hospitalVisitsAreLoading(hdid: string): boolean {
        return getHospitalVisitsState(hdid).status === LoadStatus.REQUESTED;
    }

    function hospitalVisitsAreQueued(hdid: string): boolean {
        return getHospitalVisitsState(hdid).queued;
    }

    function hospitalVisitsCount(hdid: string): number {
        return getHospitalVisitsState(hdid).data.length;
    }

    function setHospitalVisitRefreshInProgress(
        hdid: string,
        hospitalVisitResult: HospitalVisitResult
    ) {
        const hospitalVisitState = getHospitalVisitsState(hdid);
        hospitalVisitsMap.value.set(hdid, {
            ...hospitalVisitState,
            data: hospitalVisitResult.hospitalVisits,
            error: undefined,
            statusMessage: "",
            status: LoadStatus.REQUESTED,
            queued: hospitalVisitResult.queued,
        });
    }

    function setHospitalVisits(
        hdid: string,
        hospitalVisitResult: HospitalVisitResult
    ): void {
        datasetMapUtil.setStateData(
            hospitalVisitsMap.value,
            hdid,
            hospitalVisitResult.hospitalVisits,
            {
                queued: hospitalVisitResult.queued,
            }
        );
    }

    function handleError(
        hdid: string,
        resultError: ResultError,
        errorType: ErrorType,
        errorSourceType: ErrorSourceType
    ): void {
        logger.error(`ERROR: ${JSON.stringify(resultError)}`);
        datasetMapUtil.setStateError(
            hospitalVisitsMap.value,
            hdid,
            resultError
        );
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

    function retrieveHospitalVisits(
        hdid: string
    ): Promise<RequestResult<HospitalVisitResult>> {
        const trackingService = container.get<ITrackingService>(
            SERVICE_IDENTIFIER.TrackingService
        );
        if (getHospitalVisitsState(hdid).status === LoadStatus.LOADED) {
            logger.debug(`Hospital Visits found stored, not querying!`);
            const visits: HospitalVisit[] = hospitalVisits(hdid);
            const hospitalVisitsQueued: boolean = hospitalVisitsAreQueued(hdid);
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: {
                    loaded: true,
                    queued: hospitalVisitsQueued,
                    retryin: 0,
                    hospitalVisits: visits,
                },
                resultStatus: ResultType.Success,
                totalResultCount: visits.length,
            });
        }

        logger.debug(`Retrieving Hospital Visits`);
        datasetMapUtil.setStateRequested(hospitalVisitsMap.value, hdid);
        return hospitalVisitService
            .getHospitalVisits(hdid)
            .then((result) => {
                const payload = result.resourcePayload;
                if (
                    result.resultStatus === ResultType.Success &&
                    payload.loaded
                ) {
                    if (result.totalResultCount > 0) {
                        trackingService.trackEvent({
                            action: Action.Load,
                            text: Text.Data,
                            dataset: Dataset.HospitalVisits,
                        });
                    }
                    logger.info(`Hospital Visits loaded.`);
                    setHospitalVisits(hdid, payload);
                } else if (
                    result.resultError?.actionCode === ActionType.Refresh &&
                    !payload.loaded &&
                    payload.retryin > 0
                ) {
                    logger.info(
                        `Refresh in progress.... partially load hospital visits`
                    );
                    setHospitalVisitRefreshInProgress(hdid, payload);
                    setTimeout(() => {
                        logger.info("Re-querying for hospital visits");
                        retrieveHospitalVisits(hdid);
                    }, payload.retryin);
                } else {
                    if (result.resultError) {
                        throw result.resultError;
                    }
                    logger.warn(`Hospital Visits retrieval failed.`);
                }
                return result;
            })
            .catch((resultError: ResultError) => {
                handleError(
                    hdid,
                    resultError,
                    ErrorType.Retrieve,
                    ErrorSourceType.HospitalVisit
                );
                throw resultError;
            });
    }

    return {
        hospitalVisits,
        hospitalVisitsAreLoading,
        hospitalVisitsAreQueued,
        hospitalVisitsCount,
        retrieveHospitalVisits,
    };
});
