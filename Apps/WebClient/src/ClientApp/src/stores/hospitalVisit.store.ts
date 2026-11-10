import { create } from "zustand";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Action, Dataset, Text } from "@/constants/tracking";
import { type DatasetState, LoadStatus } from "@/models/datasetState";
import type { HospitalVisit } from "@/models/encounter";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
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

type HospitalVisitState = DatasetState<HospitalVisit[]>;
const defaultState: HospitalVisitState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};

interface HospitalVisitStoreState {
    states: Record<string, HospitalVisitState>;
    retrieveHospitalVisits: (hdid: string) => Promise<HospitalVisit[]>;
    reset: () => void;
}

/** Port of stores/hospitalVisit.ts */
export const useHospitalVisitStore = create<HospitalVisitStoreState>(
    (set, get) => {
        const inProgress = inProgressCalls();
        const getState = (hdid: string) =>
            getDatasetState(get().states, hdid, defaultState);
        return {
            states: {},
            retrieveHospitalVisits: (hdid) =>
                inProgress.share(hdid, async () => {
                    if (getState(hdid).status === LoadStatus.LOADED) {
                        logger.debug(
                            "Hospital Visits found stored, not querying!"
                        );
                        return getState(hdid).data;
                    }
                    logger.debug("Retrieving Hospital Visits");
                    set((s) => ({
                        states: setStateRequested(s.states, hdid, defaultState),
                    }));
                    try {
                        const result =
                            await services.hospitalVisit.getHospitalVisits(
                                hdid
                            );
                        if (result.length > 0)
                            services.tracking.trackEvent({
                                action: Action.Load,
                                text: Text.Data,
                                dataset: Dataset.HospitalVisits,
                            });
                        logger.info("Hospital Visits loaded.");
                        set((s) => ({
                            states: setStateData(
                                s.states,
                                hdid,
                                defaultState,
                                result
                            ),
                        }));
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
                                    ErrorSourceType.HospitalVisit,
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
    }
);

const stateFor = (s: HospitalVisitStoreState, hdid: string) =>
    s.states[hdid] ?? defaultState;
export const selectHospitalVisits =
    (hdid: string) => (s: HospitalVisitStoreState) =>
        stateFor(s, hdid).data;
export const selectHospitalVisitsAreLoading =
    (hdid: string) => (s: HospitalVisitStoreState) =>
        stateFor(s, hdid).status === LoadStatus.REQUESTED;
