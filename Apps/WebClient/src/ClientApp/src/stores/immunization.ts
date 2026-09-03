import { defineStore } from "pinia";
import { ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
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

const recommendationSort = (a: Recommendation, b: Recommendation): number =>
    DateSortUtility.descending(
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
                status: LoadStatus.LOADED,
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
                if (result.immunizations.length > 0) {
                    trackingService.trackEvent({
                        action: Action.Load,
                        text: Text.Data,
                        dataset: Dataset.Immunizations,
                    });
                }
                setImmunizations(hdid, result);
            })
            .catch((error: ResultError) => {
                handleError(hdid, error, ErrorType.Retrieve);
                throw error;
            });
    }

    return {
        immunizations,
        recommendations,
        immunizationsCount,
        immunizationsAreLoading,
        retrieveImmunizations,
    };
});
