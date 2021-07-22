import ImmunizationResult from "@/models/immunizationResult";
import { LoadStatus } from "@/models/storeOperations";

import { ImmunizationMutations, ImmunizationState } from "./types";

export const mutations: ImmunizationMutations = {
    setRequested(state: ImmunizationState) {
        state.status =
            state.status === LoadStatus.DEFERRED
                ? LoadStatus.ASYNC_REQUESTED
                : LoadStatus.REQUESTED;
    },
    setImmunizationResult(
        state: ImmunizationState,
        immunizationResult: ImmunizationResult
    ) {
        state.immunizations = immunizationResult.immunizations;
        state.recommendations = immunizationResult.recommendations;
        state.error = undefined;
        if (immunizationResult.loadState.refreshInProgress) {
            state.status = LoadStatus.DEFERRED;
        } else {
            state.status = LoadStatus.LOADED;
        }
    },
    immunizationError(state: ImmunizationState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
