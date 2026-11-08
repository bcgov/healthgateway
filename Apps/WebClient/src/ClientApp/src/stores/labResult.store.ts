import { create } from "zustand";

import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import { Action, Dataset, Text } from "@/constants/tracking";
import { type DatasetState, LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type {
    LaboratoryOrder,
    LaboratoryOrderResult,
} from "@/models/laboratory";
import type { RequestResult } from "@/models/requestResult";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";

import { useErrorStore } from "./error.store";
import {
    getDatasetState,
    setStateData,
    setStateError,
    setStateRequested,
} from "./utils/datasetMap";
import { inProgressCalls } from "./utils/inProgressCalls";

export interface LabResultState extends DatasetState<LaboratoryOrder[]> {
    queued: boolean;
}

const defaultState: LabResultState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    queued: false,
};

interface LabResultStoreState {
    states: Record<string, LabResultState>;
    retrieveLabResults: (
        hdid: string
    ) => Promise<RequestResult<LaboratoryOrderResult>>;
    reset: () => void;
}

/** Port of stores/labResult.ts, including the timed re-query while PHSA refreshes. */
export const useLabResultStore = create<LabResultStoreState>((set, get) => {
    const inProgress = inProgressCalls();
    const getState = (hdid: string) =>
        getDatasetState(get().states, hdid, defaultState);
    let retryTimer: ReturnType<typeof setTimeout> | undefined;

    function handleError(hdid: string, error: unknown) {
        logger.error(`ERROR: ${describeError(error)}`);
        const resultError = error instanceof ResultError ? error : undefined;
        set((s) => ({
            states: setStateError(s.states, hdid, defaultState, resultError),
        }));
        if (resultError?.statusCode === 429)
            useErrorStore.getState().setTooManyRequestsWarning("page");
        else
            useErrorStore
                .getState()
                .addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Laboratory,
                    resultError?.traceId
                );
    }

    const retrieveLabResults = (
        hdid: string
    ): Promise<RequestResult<LaboratoryOrderResult>> =>
        inProgress.share(hdid, async () => {
            if (getState(hdid).status === LoadStatus.LOADED) {
                logger.debug("Lab results found stored, not querying!");
                const state = getState(hdid);
                return {
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {
                        loaded: true,
                        queued: state.queued,
                        retryin: 0,
                        orders: state.data,
                    },
                    resultStatus: ResultType.Success,
                    totalResultCount: state.data.length,
                };
            }
            logger.debug("Retrieving Lab results");
            set((s) => ({
                states: setStateRequested(s.states, hdid, defaultState),
            }));
            try {
                const result =
                    await services.laboratory.getLaboratoryOrders(hdid);
                const payload = result.resourcePayload;
                if (payload.orders.length > 0)
                    services.tracking.trackEvent({
                        action: Action.Load,
                        text: Text.Data,
                        dataset: Dataset.LabResults,
                    });
                if (
                    result.resultStatus === ResultType.Success &&
                    payload.loaded
                ) {
                    logger.debug("Lab results loaded");
                    set((s) => ({
                        states: setStateData(
                            s.states,
                            hdid,
                            defaultState,
                            payload.orders,
                            { queued: payload.queued }
                        ),
                    }));
                } else if (
                    result.resultError?.actionCode === ActionType.Refresh &&
                    !payload.loaded &&
                    payload.retryin > 0
                ) {
                    logger.debug(
                        "Refresh in progress... partially loaded lab results"
                    );
                    set((s) => ({
                        states: setStateData(
                            s.states,
                            hdid,
                            defaultState,
                            payload.orders,
                            {
                                queued: payload.queued,
                                status: LoadStatus.REQUESTED,
                            }
                        ),
                    }));
                    clearTimeout(retryTimer);
                    retryTimer = setTimeout(() => {
                        logger.info("Re-querying for lab results");
                        // PromiseUtility.runInBackground
                        retrieveLabResults(hdid).catch((error: unknown) =>
                            logger.warn(
                                `Laboratory results retry: ${describeError(error)}`
                            )
                        );
                    }, payload.retryin);
                } else {
                    if (result.resultError)
                        throw ResultError.fromModel(result.resultError);
                    logger.warn(`Laboratory results retrieval failed!`);
                }
                return result;
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(hdid, error);
                throw error;
            }
        });

    return {
        states: {},
        retrieveLabResults,
        reset: () => {
            inProgress.clear();
            clearTimeout(retryTimer);
            set({ states: {} });
        },
    };
});

const stateFor = (s: LabResultStoreState, hdid: string) =>
    s.states[hdid] ?? defaultState;
export const selectLabResults = (hdid: string) => (s: LabResultStoreState) =>
    stateFor(s, hdid).data;
export const selectLabResultsAreLoading =
    (hdid: string) => (s: LabResultStoreState) =>
        stateFor(s, hdid).status === LoadStatus.REQUESTED;
export const selectLabResultsAreQueued =
    (hdid: string) => (s: LabResultStoreState) =>
        stateFor(s, hdid).queued;
