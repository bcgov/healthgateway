import { defineStore } from "pinia";
import { ImmunizationDatasetState } from "@/models/datasetState";
import { LoadStatus } from "@/models/storeOperations";
import { ref } from "vue";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { container } from "@/ioc/container";
import { IImmunizationService, ILogger } from "@/services/interfaces";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";
import ImmunizationResult from "@/models/immunizationResult";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { useErrorStore } from "@/stores/error";
import { ResultType } from "@/constants/resulttype";
import EventTracker from "@/utility/eventTracker";
import { EntryType } from "@/constants/entryType";

const defaultImmunizationDatasetState: ImmunizationDatasetState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    recommendations: [],
};

function immunizationSort(a: ImmunizationEvent, b: ImmunizationEvent) {
    const firstDate = new DateWrapper(a.dateOfImmunization);
    const secondDate = new DateWrapper(b.dateOfImmunization);

    if (firstDate.isAfter(secondDate)) {
        return 1;
    }
    if (firstDate.isBefore(secondDate)) {
        return -1;
    }
    return 0;
}

export const useImmunizationStore = defineStore("immunization", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const immunizationService = container.get<IImmunizationService>(
        SERVICE_IDENTIFIER.ImmunizationService
    );
    const datasetMapUtil = new DatasetMapUtils<
        ImmunizationEvent[],
        ImmunizationDatasetState
    >(defaultImmunizationDatasetState);

    const errorStore = useErrorStore();

    const immunizationMap = ref(new Map<string, ImmunizationDatasetState>());

    function getImmunizationDatasetState(
        hdid: string
    ): ImmunizationDatasetState {
        return datasetMapUtil.getDatasetState(immunizationMap.value, hdid);
    }

    function immunizations(hdid: string): ImmunizationEvent[] {
        return getImmunizationDatasetState(hdid).data;
    }

    function covidImmunizations(hdid: string): ImmunizationEvent[] {
        return immunizations(hdid)
            .filter(
                (i) =>
                    i.targetedDisease?.toLowerCase().includes("covid") &&
                    i.valid
            )
            .sort(immunizationSort);
    }

    function recommendations(hdid: string): Recommendation[] {
        return getImmunizationDatasetState(hdid).recommendations;
    }

    function immunizationsCount(hdid: string): number {
        return getImmunizationDatasetState(hdid).data.length;
    }

    function immunizationsAreLoading(hdid: string): boolean {
        return (
            getImmunizationDatasetState(hdid).status === LoadStatus.REQUESTED
        );
    }

    function immunizationsAreDeferred(hdid: string): boolean {
        const datasetState = getImmunizationDatasetState(hdid);
        return (
            datasetState.status === LoadStatus.DEFERRED ||
            datasetState.status === LoadStatus.ASYNC_REQUESTED
        );
    }

    function immunizationsError(hdid: string): ResultError | undefined {
        return getImmunizationDatasetState(hdid).error;
    }

    function setImmunizations(
        hdid: string,
        immunizationResult: ImmunizationResult
    ) {
        datasetMapUtil.setStateData(
            immunizationMap.value,
            hdid,
            immunizationResult.immunizations,
            {
                recommendations: immunizationResult.recommendations,
                status: immunizationResult.loadState.refreshInProgress
                    ? LoadStatus.DEFERRED
                    : LoadStatus.LOADED,
            }
        );
    }

    function handleError(
        hdid: string,
        error: ResultError,
        errorType: ErrorType
    ) {
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        datasetMapUtil.setStateError(immunizationMap.value, hdid, error);
        if (error.statusCode === 429) {
            errorStore.setTooManyRequestsWarning("page");
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.Immunization,
                error.traceId
            );
        }
    }

    function retrieveImmunizations(hdid: string): Promise<void> {
        if (getImmunizationDatasetState(hdid).status === LoadStatus.LOADED) {
            logger.debug(`Immunizations found sored, not querying!`);
            return Promise.resolve();
        }
        logger.debug(`Retrieving immunizations`);
        datasetMapUtil.setStateRequested(immunizationMap.value, hdid);
        return immunizationService
            .getPatientImmunizations(hdid)
            .then((result) => {
                if (result.resultStatus === ResultType.Success) {
                    return result.resourcePayload;
                } else {
                    throw result.resultError;
                }
            })
            .then((payload) => {
                if (payload.loadState.refreshInProgress) {
                    logger.info(`Immunizations load deferred`);
                    setTimeout(() => {
                        logger.info(`Re-querying for immunizations`);
                        retrieveImmunizations(hdid);
                    }, 10000);
                } else {
                    EventTracker.loadData(
                        EntryType.Immunization,
                        payload.immunizations.length
                    );
                }
                setImmunizations(hdid, payload);
            })
            .catch((error: ResultError) => {
                handleError(hdid, error, ErrorType.Retrieve);
                throw error;
            });
    }

    return {
        immunizations,
        covidImmunizations,
        recommendations,
        immunizationsCount,
        immunizationsAreLoading,
        immunizationsAreDeferred,
        immunizationsError,
        retrieveImmunizations,
    };
});
