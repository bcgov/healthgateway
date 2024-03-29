﻿import { defineStore } from "pinia";
import { ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ImmunizationDatasetState } from "@/models/datasetState";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import ImmunizationResult from "@/models/immunizationResult";
import { LoadStatus } from "@/models/storeOperations";
import { Action, Dataset, Text } from "@/plugins/extensions";
import {
    IImmunizationService,
    ILogger,
    ITrackingService,
} from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { DatasetMapUtils } from "@/stores/utils/DatasetMapUtils";
import DateSortUtility from "@/utility/dateSortUtility";

const defaultImmunizationDatasetState: ImmunizationDatasetState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    recommendations: [],
};

const immunizationSort = (a: ImmunizationEvent, b: ImmunizationEvent): number =>
    DateSortUtility.ascending(
        DateWrapper.fromIsoDate(a.dateOfImmunization),
        DateWrapper.fromIsoDate(b.dateOfImmunization)
    );

const recommendationSort = (a: Recommendation, b: Recommendation): number =>
    DateSortUtility.ascending(
        a.agentDueDate ? DateWrapper.fromIsoDate(a.agentDueDate) : undefined,
        b.agentDueDate ? DateWrapper.fromIsoDate(b.agentDueDate) : undefined
    );

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
                recommendations: immunizationResult.recommendations
                    .filter((x) => x.recommendedVaccinations)
                    .sort(recommendationSort),
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
        const trackingService = container.get<ITrackingService>(
            SERVICE_IDENTIFIER.TrackingService
        );
        if (getImmunizationDatasetState(hdid).status === LoadStatus.LOADED) {
            logger.debug(`Immunizations found stored, not querying!`);
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
                    throw result.resultError
                        ? ResultError.fromModel(result.resultError)
                        : new ResultError(
                              "ImmunizationStore",
                              "Unknown API error on retrieve immunizations"
                          );
                }
            })
            .then((payload) => {
                if (payload.loadState.refreshInProgress) {
                    logger.info(`Immunizations load deferred`);
                    setTimeout(() => {
                        logger.info(`Re-querying for immunizations`);
                        retrieveImmunizations(hdid);
                    }, 10000);
                } else if (payload.immunizations.length > 0) {
                    trackingService.trackEvent({
                        action: Action.Load,
                        text: Text.Data,
                        dataset: Dataset.Immunizations,
                    });
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
