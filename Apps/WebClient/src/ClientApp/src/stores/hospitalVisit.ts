import { defineStore } from "pinia";
import { ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { HospitalVisitState } from "@/models/datasetState";
import { HospitalVisit } from "@/models/encounter";
import { ResultError } from "@/models/errors";
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

    function hospitalVisitsCount(hdid: string): number {
        return getHospitalVisitsState(hdid).data.length;
    }

    function setHospitalVisits(
        hdid: string,
        hospitalVisits: HospitalVisit[]
    ): void {
        datasetMapUtil.setStateData(
            hospitalVisitsMap.value,
            hdid,
            hospitalVisits,
            {
                queued: false,
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

    function retrieveHospitalVisits(hdid: string): Promise<HospitalVisit[]> {
        const trackingService = container.get<ITrackingService>(
            SERVICE_IDENTIFIER.TrackingService
        );
        if (getHospitalVisitsState(hdid).status === LoadStatus.LOADED) {
            logger.debug(`Hospital Visits found stored, not querying!`);
            return Promise.resolve(hospitalVisits(hdid));
        }

        logger.debug(`Retrieving Hospital Visits`);
        datasetMapUtil.setStateRequested(hospitalVisitsMap.value, hdid);
        return hospitalVisitService
            .getHospitalVisits(hdid)
            .then((result) => {
                if (result.length > 0) {
                    trackingService.trackEvent({
                        action: Action.Load,
                        text: Text.Data,
                        dataset: Dataset.HospitalVisits,
                    });
                }
                logger.info(`Hospital Visits loaded.`);
                setHospitalVisits(hdid, result);

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
        hospitalVisitsCount,
        retrieveHospitalVisits,
    };
});
