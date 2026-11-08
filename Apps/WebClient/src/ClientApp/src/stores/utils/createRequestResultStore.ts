import { create } from "zustand";

import type { ErrorSourceType } from "@/constants/errorType";
import { ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import { Action, type Dataset, Text } from "@/constants/tracking";
import { type DatasetState, LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type { RequestResult } from "@/models/requestResult";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";

import { useErrorStore } from "../error.store";
import {
    getDatasetState,
    setStateData,
    setStateError,
    setStateRequested,
} from "./datasetMap";
import { inProgressCalls } from "./inProgressCalls";

export interface RequestResultStoreState<T> {
    states: Record<string, DatasetState<T[]>>;
    retrieve: (hdid: string) => Promise<RequestResult<T[]>>;
    reset: () => void;
}

/**
 * The store shape the web client repeats for health visits, Special Authority requests and the like: one
 * per-hdid dataset loaded from a RequestResult API. Success stores the payload (and tracks a load when it is
 * not empty); an error result is thrown as a ResultError; a 429 sets the page warning, other errors a banner.
 */
export function createRequestResultStore<T>(options: {
    /** log noun, e.g. "Health Visits" */
    name: string;
    fetch: (hdid: string) => Promise<RequestResult<T[]>>;
    dataset: Dataset;
    errorSource: ErrorSourceType;
}) {
    const defaultState: DatasetState<T[]> = {
        data: [],
        status: LoadStatus.NONE,
        statusMessage: "",
        error: undefined,
    };
    const store = create<RequestResultStoreState<T>>((set, get) => {
        const inProgress = inProgressCalls();
        const getState = (hdid: string) =>
            getDatasetState(get().states, hdid, defaultState);
        return {
            states: {},
            retrieve: (hdid) =>
                inProgress.share(hdid, async () => {
                    if (getState(hdid).status === LoadStatus.LOADED) {
                        logger.debug(
                            `${options.name} found stored, not querying!`
                        );
                        const data = getState(hdid).data;
                        return {
                            pageIndex: 0,
                            pageSize: 0,
                            resourcePayload: data,
                            resultStatus: ResultType.Success,
                            totalResultCount: data.length,
                        };
                    }
                    logger.debug(`Retrieving ${options.name}`);
                    set((s) => ({
                        states: setStateRequested(s.states, hdid, defaultState),
                    }));
                    try {
                        const result = await options.fetch(hdid);
                        if (result.resultStatus === ResultType.Success) {
                            if (result.resourcePayload.length > 0)
                                services.tracking.trackEvent({
                                    action: Action.Load,
                                    text: Text.Data,
                                    dataset: options.dataset,
                                });
                            logger.info(`${options.name} loaded.`);
                            set((s) => ({
                                states: setStateData(
                                    s.states,
                                    hdid,
                                    defaultState,
                                    result.resourcePayload
                                ),
                            }));
                        } else {
                            if (result.resultError)
                                throw ResultError.fromModel(result.resultError);
                            logger.warn(`${options.name} retrieval failed!`);
                        }
                        return result;
                    } catch (error) {
                        rethrowIfSessionEnded(error);
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
                            useErrorStore
                                .getState()
                                .setTooManyRequestsWarning("page");
                        else
                            useErrorStore
                                .getState()
                                .addError(
                                    ErrorType.Retrieve,
                                    options.errorSource,
                                    resultError?.traceId
                                );
                        throw error;
                    }
                }),
            reset: () => {
                inProgress.clear();
                set({ states: {} });
            },
        };
    });
    const stateFor = (s: RequestResultStoreState<T>, hdid: string) =>
        s.states[hdid] ?? defaultState;
    return {
        store,
        selectData: (hdid: string) => (s: RequestResultStoreState<T>) =>
            stateFor(s, hdid).data,
        selectIsLoading: (hdid: string) => (s: RequestResultStoreState<T>) =>
            stateFor(s, hdid).status === LoadStatus.REQUESTED,
    };
}
