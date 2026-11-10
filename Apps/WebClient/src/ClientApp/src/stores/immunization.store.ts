import { create } from "zustand";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Action, Dataset, Text } from "@/constants/tracking";
import { type DatasetState, LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type {
    ImmunizationEvent,
    ImmunizationResult,
    Recommendation,
} from "@/models/immunization";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { descending, fromIsoDate } from "@/utils/dateUtil";

import { useErrorStore } from "./error.store";
import {
    getDatasetState,
    setStateData,
    setStateError,
    setStateRequested,
} from "./utils/datasetMap";
import { inProgressCalls } from "./utils/inProgressCalls";

export interface ImmunizationDatasetState extends DatasetState<
    ImmunizationEvent[]
> {
    recommendations: Recommendation[];
}

const defaultState: ImmunizationDatasetState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    recommendations: [],
};

const recommendationSort = (a: Recommendation, b: Recommendation): number =>
    descending(
        a.agentDueDate ? fromIsoDate(a.agentDueDate) : undefined,
        b.agentDueDate ? fromIsoDate(b.agentDueDate) : undefined
    );

interface ImmunizationStoreState {
    /** dataset state per hdid (immunizationMap) */
    states: Record<string, ImmunizationDatasetState>;
    retrieveImmunizations: (hdid: string) => Promise<void>;
    reset: () => void;
}

/** Port of stores/immunization.ts (DatasetMapUtils as plain per-hdid records). */
export const useImmunizationStore = create<ImmunizationStoreState>(
    (set, get) => {
        const inProgress = inProgressCalls();
        const getState = (hdid: string): ImmunizationDatasetState =>
            getDatasetState(get().states, hdid, defaultState);

        function setImmunizations(hdid: string, result: ImmunizationResult) {
            set((s) => ({
                states: setStateData(
                    s.states,
                    hdid,
                    defaultState,
                    result.immunizations,
                    {
                        recommendations: result.recommendations
                            .filter((x) => x.recommendedVaccinations)
                            .sort(recommendationSort),
                        status: LoadStatus.LOADED,
                    }
                ),
            }));
        }

        function handleError(hdid: string, error: unknown) {
            logger.error(`ERROR: ${describeError(error)}`);
            const resultError =
                error instanceof ResultError ? error : undefined;
            set((s) => ({
                states: setStateError(
                    s.states,
                    hdid,
                    defaultState,
                    resultError
                ),
            }));
            if (resultError?.statusCode === 429)
                useErrorStore.getState().setTooManyRequestsWarning("page");
            else
                useErrorStore
                    .getState()
                    .addError(
                        ErrorType.Retrieve,
                        ErrorSourceType.Immunization,
                        resultError?.traceId
                    );
        }

        return {
            states: {},
            retrieveImmunizations: (hdid) =>
                inProgress.share(hdid, async () => {
                    if (getState(hdid).status === LoadStatus.LOADED) {
                        logger.debug(
                            "Immunizations found stored, not querying!"
                        );
                        return;
                    }
                    logger.debug("Retrieving immunizations");
                    set((s) => ({
                        states: setStateRequested(s.states, hdid, defaultState),
                    }));
                    try {
                        const result =
                            await services.immunization.getPatientImmunizations(
                                hdid
                            );
                        if (result.immunizations.length > 0)
                            services.tracking.trackEvent({
                                action: Action.Load,
                                text: Text.Data,
                                dataset: Dataset.Immunizations,
                            });
                        setImmunizations(hdid, result);
                    } catch (error) {
                        rethrowIfSessionEnded(error);
                        handleError(hdid, error);
                        throw error;
                    }
                }),
            reset: () => {
                inProgress.clear();
                set({ states: {} });
            },
        };
    }
);

const stateFor = (s: ImmunizationStoreState, hdid: string) =>
    s.states[hdid] ?? defaultState;

/** immunizations(hdid) */
export const selectImmunizations =
    (hdid: string) => (s: ImmunizationStoreState) =>
        stateFor(s, hdid).data;
/** recommendations(hdid) */
export const selectRecommendations =
    (hdid: string) => (s: ImmunizationStoreState) =>
        stateFor(s, hdid).recommendations;
/** immunizationsAreLoading(hdid) */
export const selectImmunizationsAreLoading =
    (hdid: string) => (s: ImmunizationStoreState) =>
        stateFor(s, hdid).status === LoadStatus.REQUESTED;
