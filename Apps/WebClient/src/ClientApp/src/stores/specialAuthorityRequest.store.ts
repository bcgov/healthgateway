import { create } from "zustand";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import { Action, Dataset, Text } from "@/constants/tracking";
import { type DatasetState, LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type { MedicationRequest } from "@/models/medicationRequest";
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

type SpecialAuthorityRequestState = DatasetState<MedicationRequest[]>;
const defaultState: SpecialAuthorityRequestState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};

interface SpecialAuthorityRequestStoreState {
    states: Record<string, SpecialAuthorityRequestState>;
    retrieveSpecialAuthorityRequests: (
        hdid: string
    ) => Promise<RequestResult<MedicationRequest[]>>;
    reset: () => void;
}

/** Port of stores/specialAuthorityRequest.ts */
export const useSpecialAuthorityRequestStore =
    create<SpecialAuthorityRequestStoreState>((set, get) => {
        const inProgress = inProgressCalls();
        const getState = (hdid: string) =>
            getDatasetState(get().states, hdid, defaultState);
        return {
            states: {},
            retrieveSpecialAuthorityRequests: (hdid) =>
                inProgress.share(hdid, async () => {
                    if (getState(hdid).status === LoadStatus.LOADED) {
                        logger.debug(
                            "Special Authority requests found stored, not querying!"
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
                    set((s) => ({
                        states: setStateRequested(s.states, hdid, defaultState),
                    }));
                    try {
                        const result =
                            await services.specialAuthority.getPatientMedicationRequest(
                                hdid
                            );
                        if (result.resultStatus === ResultType.Error)
                            throw result.resultError
                                ? ResultError.fromModel(result.resultError)
                                : new ResultError(
                                      "SpecialAuthorityStore",
                                      "Unknown API error when retrieving special authority requests."
                                  );
                        if (result.resourcePayload.length > 0)
                            services.tracking.trackEvent({
                                action: Action.Load,
                                text: Text.Data,
                                dataset: Dataset.SpecialAuthorityRequests,
                            });
                        // setSpecialAuthorityRequests: anything but success is kept as an error state, without a banner
                        if (result.resultStatus === ResultType.Success)
                            set((s) => ({
                                states: setStateData(
                                    s.states,
                                    hdid,
                                    defaultState,
                                    result.resourcePayload
                                ),
                            }));
                        else {
                            const errorResult = result.resultError
                                ? ResultError.fromModel(result.resultError)
                                : new ResultError(
                                      "SpecialAuthorityStore",
                                      "Set Special Authority Error"
                                  );
                            set((s) => ({
                                states: setStateError(
                                    s.states,
                                    hdid,
                                    defaultState,
                                    errorResult
                                ),
                            }));
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
                                    ErrorSourceType.MedicationRequests,
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

const stateFor = (s: SpecialAuthorityRequestStoreState, hdid: string) =>
    s.states[hdid] ?? defaultState;
export const selectSpecialAuthorityRequests =
    (hdid: string) => (s: SpecialAuthorityRequestStoreState) =>
        stateFor(s, hdid).data;
export const selectSpecialAuthorityRequestsAreLoading =
    (hdid: string) => (s: SpecialAuthorityRequestStoreState) =>
        stateFor(s, hdid).status === LoadStatus.REQUESTED;
