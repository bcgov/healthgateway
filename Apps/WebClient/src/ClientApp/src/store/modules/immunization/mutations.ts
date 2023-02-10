import { ImmunizationDatasetState } from "@/models/datasetState";
import { ResultError } from "@/models/errors";
import ImmunizationResult from "@/models/immunizationResult";
import { LoadStatus } from "@/models/storeOperations";

import { ImmunizationMutations, ImmunizationState } from "./types";
import {
    getImmunizationDatasetState,
    setImmunizationDatasetState,
} from "./util";

export const mutations: ImmunizationMutations = {
    setImmunizationsRequested(state: ImmunizationState, hdid: string) {
        const currentState = getImmunizationDatasetState(state, hdid);
        const nextState: ImmunizationDatasetState = {
            ...currentState,
            status:
                currentState.status === LoadStatus.DEFERRED
                    ? LoadStatus.ASYNC_REQUESTED
                    : LoadStatus.REQUESTED,
        };
        setImmunizationDatasetState(state, hdid, nextState);
    },

    setImmunizations(
        state: ImmunizationState,
        payload: { hdid: string; immunizationResult: ImmunizationResult }
    ) {
        const { hdid, immunizationResult } = payload;
        const currentState = getImmunizationDatasetState(state, hdid);
        const nextState: ImmunizationDatasetState = {
            ...currentState,
            data: immunizationResult.immunizations,
            recommendations: immunizationResult.recommendations,
            error: undefined,
            statusMessage: "success",
            status: immunizationResult.loadState.refreshInProgress
                ? LoadStatus.DEFERRED
                : LoadStatus.LOADED,
        };
        setImmunizationDatasetState(state, hdid, nextState);
    },
    setImmunizationsError(
        state: ImmunizationState,
        payload: { hdid: string; error: ResultError }
    ) {
        const { hdid, error } = payload;
        const currentState = getImmunizationDatasetState(state, hdid);
        const nextState: ImmunizationDatasetState = {
            ...currentState,
            error: error,
            statusMessage: error.resultMessage,
            status: LoadStatus.ERROR,
        };
        setImmunizationDatasetState(state, hdid, nextState);
    },
};
