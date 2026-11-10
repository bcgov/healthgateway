import { create } from "zustand";

import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import { type DatasetState, LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type {
    Covid19LaboratoryOrder,
    Covid19LaboratoryOrderResult,
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

type Covid19TestResultState = DatasetState<Covid19LaboratoryOrder[]>;

const defaultState: Covid19TestResultState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};

interface Covid19TestResultStoreState {
    states: Record<string, Covid19TestResultState>;
    retrieveCovid19TestResults: (
        hdid: string
    ) => Promise<RequestResult<Covid19LaboratoryOrderResult>>;
    reset: () => void;
}

/** Port of stores/covid19TestResult.ts (stays "requested" while it waits to re-query a refreshing PHSA). */
export const useCovid19TestResultStore = create<Covid19TestResultStoreState>(
    (set, get) => {
        const inProgress = inProgressCalls();
        const getState = (hdid: string) =>
            getDatasetState(get().states, hdid, defaultState);
        let retryTimer: ReturnType<typeof setTimeout> | undefined;

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
                        ErrorSourceType.Laboratory,
                        resultError?.traceId
                    );
        }

        const retrieveCovid19TestResults = (
            hdid: string
        ): Promise<RequestResult<Covid19LaboratoryOrderResult>> =>
            inProgress.share(hdid, async () => {
                if (getState(hdid).status === LoadStatus.LOADED) {
                    logger.debug(
                        "COVID-19 laboratory orders found stored, not querying!"
                    );
                    const data = getState(hdid).data;
                    return {
                        pageIndex: 0,
                        pageSize: 0,
                        resourcePayload: {
                            loaded: true,
                            retryin: 0,
                            orders: data,
                        },
                        resultStatus: ResultType.Success,
                        totalResultCount: data.length,
                    };
                }
                logger.debug("Retrieving COVID-19 test results");
                set((s) => ({
                    states: setStateRequested(s.states, hdid, defaultState),
                }));
                try {
                    const result =
                        await services.laboratory.getCovid19LaboratoryOrders(
                            hdid
                        );
                    const payload = result.resourcePayload;
                    if (
                        result.resultStatus === ResultType.Success &&
                        payload.loaded
                    ) {
                        logger.debug("COVID-19 test results loaded");
                        set((s) => ({
                            states: setStateData(
                                s.states,
                                hdid,
                                defaultState,
                                payload.orders
                            ),
                        }));
                    } else if (
                        result.resultError?.actionCode === ActionType.Refresh &&
                        !payload.loaded &&
                        payload.retryin > 0
                    ) {
                        logger.debug("COVID-19 test results not loaded");
                        clearTimeout(retryTimer);
                        retryTimer = setTimeout(() => {
                            logger.info(
                                "Re-querying for COVID-19 test results"
                            );
                            retrieveCovid19TestResults(hdid).catch(
                                (error: unknown) =>
                                    logger.warn(
                                        `COVID-19 test results retry: ${describeError(error)}`
                                    )
                            );
                        }, payload.retryin);
                    } else {
                        if (result.resultError)
                            throw ResultError.fromModel(result.resultError);
                        logger.warn(`COVID-19 test results retrieval failed!`);
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
            retrieveCovid19TestResults,
            reset: () => {
                inProgress.clear();
                clearTimeout(retryTimer);
                set({ states: {} });
            },
        };
    }
);

const stateFor = (s: Covid19TestResultStoreState, hdid: string) =>
    s.states[hdid] ?? defaultState;
export const selectCovid19TestResults =
    (hdid: string) => (s: Covid19TestResultStoreState) =>
        stateFor(s, hdid).data;
export const selectCovid19TestResultsAreLoading =
    (hdid: string) => (s: Covid19TestResultStoreState) =>
        stateFor(s, hdid).status === LoadStatus.REQUESTED;
