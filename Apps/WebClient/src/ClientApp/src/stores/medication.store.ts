import { create } from "zustand";

import { ActionType } from "@/constants/actionType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import { Action, Dataset, Text } from "@/constants/tracking";
import { type DatasetState, LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type { MedicationStatement } from "@/models/medication";
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

export interface MedicationState extends DatasetState<MedicationStatement[]> {
    protectiveWordAttempts: number;
}

const defaultState: MedicationState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    protectiveWordAttempts: 0,
};

const formatDinPin = (din: string | null | undefined): string =>
    (din ?? "").padStart(8, "0");

interface MedicationStoreState {
    states: Record<string, MedicationState>;
    retrieveMedications: (
        hdid: string,
        protectiveWord?: string
    ) => Promise<RequestResult<MedicationStatement[]>>;
    reset: () => void;
}

/** Port of stores/medication.ts */
export const useMedicationStore = create<MedicationStoreState>((set, get) => {
    const inProgress = inProgressCalls();
    const getState = (hdid: string) =>
        getDatasetState(get().states, hdid, defaultState);

    function setMedications(
        hdid: string,
        result: RequestResult<MedicationStatement[]>
    ) {
        if (result.resultStatus === ResultType.Success) {
            const payload = result.resourcePayload ?? [];
            // normalize DIN / PIN
            for (const medication of payload)
                if (medication?.medicationSummary)
                    medication.medicationSummary.din = formatDinPin(
                        medication.medicationSummary.din
                    );
            set((s) => ({
                states: setStateData(s.states, hdid, defaultState, payload, {
                    protectiveWordAttempts: 0,
                }),
            }));
        } else if (
            result.resultStatus === ResultType.ActionRequired &&
            result.resultError?.actionCode === ActionType.Protected
        ) {
            const attempts = getState(hdid).protectiveWordAttempts;
            set((s) => ({
                states: setStateData(
                    s.states,
                    hdid,
                    defaultState,
                    result.resourcePayload ?? [],
                    {
                        protectiveWordAttempts: attempts + 1,
                        status: LoadStatus.PROTECTED,
                    }
                ),
            }));
        } else {
            const resultError = result.resultError
                ? ResultError.fromModel(result.resultError)
                : new ResultError(
                      "MedicationStore",
                      "Unknown API error when settings medications."
                  );
            set((s) => ({
                states: setStateError(
                    s.states,
                    hdid,
                    defaultState,
                    resultError,
                    "Error returned from the medications call"
                ),
            }));
        }
    }

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
                    ErrorSourceType.MedicationStatements,
                    resultError?.traceId
                );
    }

    return {
        states: {},
        retrieveMedications: (hdid, protectiveWord) =>
            inProgress.share(protectiveWord ? undefined : hdid, async () => {
                if (getState(hdid).status === LoadStatus.LOADED) {
                    logger.debug("Medications found stored, not querying!");
                    const data = getState(hdid).data;
                    return {
                        pageIndex: 0,
                        pageSize: 0,
                        resourcePayload: data,
                        resultStatus: ResultType.Success,
                        totalResultCount: data.length,
                    };
                }
                logger.debug("Retrieving medications");
                set((s) => ({
                    states: setStateRequested(s.states, hdid, defaultState),
                }));
                try {
                    const result =
                        await services.medication.getPatientMedicationStatements(
                            hdid,
                            protectiveWord
                        );
                    if (result.resultStatus === ResultType.Error)
                        throw result.resultError
                            ? ResultError.fromModel(result.resultError)
                            : new ResultError(
                                  "MedicationStore",
                                  "Unknown API error when retrieving medications."
                              );
                    if (
                        result.resultStatus === ResultType.Success &&
                        result.resourcePayload.length > 0
                    )
                        services.tracking.trackEvent({
                            action: Action.Load,
                            text: Text.Data,
                            dataset: Dataset.Medications,
                        });
                    setMedications(hdid, result);
                    return result;
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
});

const stateFor = (s: MedicationStoreState, hdid: string) =>
    s.states[hdid] ?? defaultState;

export const selectMedications = (hdid: string) => (s: MedicationStoreState) =>
    stateFor(s, hdid).data;
export const selectMedicationsAreLoading =
    (hdid: string) => (s: MedicationStoreState) =>
        stateFor(s, hdid).status === LoadStatus.REQUESTED;
export const selectMedicationsAreProtected =
    (hdid: string) => (s: MedicationStoreState) =>
        stateFor(s, hdid).status === LoadStatus.PROTECTED;
export const selectProtectiveWordAttempts =
    (hdid: string) => (s: MedicationStoreState) =>
        stateFor(s, hdid).protectiveWordAttempts;
